using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Infrastructure;
using Nexus1.RootCause.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Composition root only — no business logic (dependency law, Nexus1.ArchitectureTests).
var rootCauseConnectionString = builder.Configuration.GetConnectionString("RootCauseDb")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:RootCauseDb configuration.");

builder.Services.AddBuildingBlocksApplication();

var rabbitMqOptions = new RabbitMqOptions(
    builder.Configuration["RabbitMq:HostName"] ?? "localhost",
    int.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672"),
    builder.Configuration["RabbitMq:UserName"] ?? "guest",
    builder.Configuration["RabbitMq:Password"] ?? "guest",
    builder.Configuration["RabbitMq:VirtualHost"] ?? "/");
builder.Services.AddNexusMessaging(rabbitMqOptions);

builder.Services.AddRootCauseApplication();
builder.Services.AddRootCauseInfrastructure(rootCauseConnectionString);

builder.Services
    .AddHealthChecks()
    .AddCheck<DbContextHealthCheck<RootCauseDbContext>>("rootcause-db");

var app = builder.Build();

// Liveness: process is up, no dependency checks (ADR-007).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Readiness: can this host actually reach its database.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

app.Run();
