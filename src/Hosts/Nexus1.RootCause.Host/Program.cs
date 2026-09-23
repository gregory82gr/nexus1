using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Explain;
using Nexus1.RootCause.Infrastructure;
using Nexus1.RootCause.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Composition root only — no business logic (dependency law, Nexus1.ArchitectureTests).
var rootCauseConnectionString = builder.Configuration.GetConnectionString("RootCauseDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:RootCauseDb configuration.");

builder.Services.AddBuildingBlocksApplication();

var otlpEndpoint = new Uri(builder.Configuration["Otel:OtlpEndpoint"] ?? "http://localhost:4317");
builder.Services.AddNexusObservability(new NexusObservabilityOptions("Nexus1.RootCause.Host", otlpEndpoint));

var rabbitMqOptions = new RabbitMqOptions(
    builder.Configuration["RabbitMq:HostName"] ?? "localhost",
    int.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672"),
    builder.Configuration["RabbitMq:UserName"] ?? "guest",
    builder.Configuration["RabbitMq:Password"] ?? "guest",
    builder.Configuration["RabbitMq:VirtualHost"] ?? "/");
builder.Services.AddNexusMessaging(rabbitMqOptions);

builder.Services.AddRootCauseApplication();
builder.Services.AddRootCauseInfrastructure(rootCauseConnectionString);

// Served-model stage (ADR-033) + the split read-only retrieval connection (ADR-034).
// The engine keeps the read-write nexus1_app connection above; retrieval + the H3/H4
// validator run through the read-only nexus1_explain connection (H7); the model
// itself has no database access at all.
var ollamaOptions = new OllamaOptions
{
    Endpoint = new Uri(builder.Configuration["Ollama:Endpoint"] ?? "http://127.0.0.1:11434"),
    ChatModelId = builder.Configuration["Ollama:ChatModel"] ?? "nexus-dslm",
    EmbeddingModelId = builder.Configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text",
    RequestTimeout = TimeSpan.FromSeconds(
        double.TryParse(builder.Configuration["Ollama:RequestTimeoutSeconds"], out var ollamaTimeoutSeconds) ? ollamaTimeoutSeconds : 150),
};
builder.Services.AddRootCauseExplain(ollamaOptions);

var rootCauseExplainConnectionString = builder.Configuration.GetConnectionString("RootCauseExplainDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:RootCauseExplainDb configuration (the read-only nexus1_explain login, ADR-034).");
builder.Services.AddRootCauseReadOnlyRetrieval(rootCauseExplainConnectionString);

builder.Services
    .AddHealthChecks()
    .AddCheck<DbContextHealthCheck<RootCauseDbContext>>("rootcause-db");

var app = builder.Build();

// Explicit dev provisioning (ADR-034): `dotnet run -- provision` seeds the fixture
// and populates corpus embeddings, then exits -- it never runs on a normal start,
// so the request path assumes real data already exists rather than fabricating it.
if (args.Contains("provision", StringComparer.OrdinalIgnoreCase))
{
    var provisionLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RootCause.Provisioning");
    await Nexus1.RootCause.Host.Provisioning.RunAsync(app.Services, provisionLogger, CancellationToken.None);
    return;
}

// Liveness: process is up, no dependency checks (ADR-007).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this host actually reach its database.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

// Run the fixed-incident One-Truth Pipeline and return its sealed result (ADR-034).
// Synchronous (CPU inference is 26-70s). An abstention is a first-class 200; an
// unknown incident is 404; genuine infrastructure failure (model unreachable, DB
// down) is 503 -- never a fabricated verdict. This lambda is transport only: it
// dispatches to the Application handler and maps the Result to HTTP status.
app.MapPost("/api/v1/root-cause/incidents/{incidentId}/diagnoses", async (
    string incidentId,
    [FromServices] RunFixedIncidentDiagnosisCommandHandler handler,
    [FromServices] ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await handler.Handle(new RunFixedIncidentDiagnosisCommand(incidentId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error });
    }
    catch (Exception ex)
    {
        loggerFactory.CreateLogger("RootCause.Diagnosis").LogError(ex, "Diagnosis pipeline failed for {IncidentId}", incidentId);
        return Results.Problem(
            title: "Diagnosis pipeline could not run",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// Read-only causal-graph topology for the incident (ADR-036) -- the nodes and
// edges the console draws as Figure 29.1. GET (no writes, no engine run); reads
// the seeded Component/Edge via nexus1_app. EVT-2026-0418 only; else 404.
app.MapGet("/api/v1/root-cause/incidents/{incidentId}/graph", async (
    string incidentId,
    [FromServices] GetIncidentGraphQueryHandler handler,
    CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetIncidentGraphQuery(incidentId), cancellationToken);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound(new { error = result.Error });
});

app.Run();
