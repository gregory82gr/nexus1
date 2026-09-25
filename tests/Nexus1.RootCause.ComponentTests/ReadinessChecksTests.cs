using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nexus1.RootCause.Explain;
using Nexus1.RootCause.Infrastructure.Persistence;
using Nexus1.ServiceDefaults;
using RootCauseInfrastructure = Nexus1.RootCause.Infrastructure.ServiceCollectionExtensions;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// ADR-044: the two readiness checks RootCause.Host was missing. The explain-connection check is
/// exercised through the same registration path the host uses (AddRootCauseReadOnlyRetrieval +
/// AddKeyedDbContextCheck) against a real LocalDB; the Ollama check against a stubbed HTTP
/// handler (models present / model missing / connection refused).
/// </summary>
public class ReadinessChecksTests : RootCauseComponentTestDatabase
{
    private const string Unreachable = "Server=tcp:127.0.0.1,1499;Database=RootCauseDb;User Id=probe;Password=probe;Connect Timeout=2;";

    // ----- rootcause-explain-db: the keyed read-only context -----
    // Each test first gives its database the REAL RootCauseDb layout: migration history lives in
    // dbo.__EFMigrationsHistory_RootCause (the name AddRootCauseInfrastructure configures), not the
    // default table the test base class migrates into. Without that, a context that forgot the
    // custom history table would pass here and fail against the real database -- which is exactly
    // what happened (ADR-044).

    [Fact]
    public async Task Explain_connection_check_is_healthy_against_the_real_migrations_history_layout()
    {
        // Found live (ADR-044): the keyed context was built with a bare UseSqlServer, so it looked
        // for the default __EFMigrationsHistory table instead of __EFMigrationsHistory_RootCause and
        // reported all applied migrations as pending under the real RootCauseDb. Retrieval never
        // reads migration history, so nothing noticed until the readiness check did.
        await UseRealHistoryLayoutAsync();

        var report = await RunAsync(readOnly: ConnectionString);

        var entry = report.Entries["rootcause-explain-db"];
        Assert.True(entry.Status == HealthStatus.Healthy, entry.Description);
    }

    [Fact]
    public async Task Explain_connection_check_is_unhealthy_when_the_keyed_context_cannot_connect()
    {
        await UseRealHistoryLayoutAsync();

        var report = await RunAsync(readOnly: Unreachable);

        Assert.Equal(HealthStatus.Unhealthy, report.Entries["rootcause-explain-db"].Status);
        Assert.Equal(HealthStatus.Unhealthy, report.Status);
    }

    [Fact]
    public async Task Explain_connection_check_checks_the_keyed_connection_not_the_primary_one()
    {
        // The trap ADR-044 names: both registrations are RootCauseDbContext. The primary one is
        // healthy, the keyed read-only one is not -- a plain AddCheck<DbContextHealthCheck<T>> would
        // report Healthy here; the keyed check must not.
        await UseRealHistoryLayoutAsync();

        var report = await RunAsync(readOnly: Unreachable, alsoCheckPrimary: true);

        Assert.Equal(HealthStatus.Healthy, report.Entries["rootcause-db"].Status);
        Assert.Equal(HealthStatus.Unhealthy, report.Entries["rootcause-explain-db"].Status);
    }

    // ----- ollama: reachability + model presence, NOT warmth -----

    [Fact]
    public async Task Ollama_check_is_healthy_when_both_models_are_listed_and_says_it_is_not_a_warmth_check()
    {
        var result = await CheckOllamaAsync(StubHandler.Tags("nexus-dslm:latest", "nomic-embed-text:latest", "qwen2.5:3b-instruct-q4_K_M"));

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("NOT a warmth check", result.Description);
    }

    [Fact]
    public async Task Ollama_check_is_unhealthy_when_a_configured_model_is_missing()
    {
        // nexus-dslm-old shares the prefix but is a different model -- must not count.
        var result = await CheckOllamaAsync(StubHandler.Tags("nexus-dslm-old:latest", "nomic-embed-text:latest"));

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("not installed: nexus-dslm", result.Description);
    }

    [Fact]
    public async Task Ollama_check_is_unhealthy_when_the_server_refuses_the_connection()
    {
        var result = await CheckOllamaAsync(StubHandler.Refused());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.IsType<HttpRequestException>(result.Exception);
    }

    [Fact]
    public async Task Ollama_check_reports_its_own_timeout_reason_when_the_server_never_answers()
    {
        var result = await CheckOllamaAsync(StubHandler.Hangs());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("did not answer /api/tags within 2s", result.Description);
    }

    // ----- helpers -----

    private async Task UseRealHistoryLayoutAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("EXEC sp_rename 'dbo.__EFMigrationsHistory', '__EFMigrationsHistory_RootCause';");
    }

    private async Task<HealthReport> RunAsync(string readOnly, bool alsoCheckPrimary = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Both registrations exactly as RootCause.Host's Program.cs makes them.
        RootCauseInfrastructure.AddRootCauseInfrastructure(services, ConnectionString);
        RootCauseInfrastructure.AddRootCauseReadOnlyRetrieval(services, readOnly);

        var checks = services.AddHealthChecks();
        if (alsoCheckPrimary)
        {
            checks.AddCheck<DbContextHealthCheck<RootCauseDbContext>>("rootcause-db");
        }

        checks.AddKeyedDbContextCheck<RootCauseDbContext>("rootcause-explain-db", RootCauseInfrastructure.ReadOnlyDbContextKey, TimeSpan.FromSeconds(5));

        await using var provider = services.BuildServiceProvider();
        return await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();
    }

    private static Task<HealthCheckResult> CheckOllamaAsync(HttpMessageHandler handler)
    {
        var check = new OllamaReachabilityHealthCheck(new HttpClient(handler), new OllamaOptions(), TimeSpan.FromSeconds(2));
        return check.CheckHealthAsync(new HealthCheckContext());
    }

    private sealed class HangingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("unreachable");
        }
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public static StubHandler Tags(params string[] names) => new(request =>
        {
            Assert.Equal("/api/tags", request.RequestUri!.AbsolutePath);
            var json = "{\"models\":[" + string.Join(",", names.Select(n => $"{{\"name\":\"{n}\"}}")) + "]}";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        });

        public static HangingHandler Hangs() => new();

        public static StubHandler Refused() => new(_ => throw new HttpRequestException("No connection could be made because the target machine actively refused it."));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
