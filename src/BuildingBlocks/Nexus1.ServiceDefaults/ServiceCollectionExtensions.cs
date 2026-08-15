using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Observability;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nexus1.ServiceDefaults;

/// <summary>
/// Host-composition-root registration only (ADR-013) — the actual
/// OpenTelemetry SDK wiring belongs here, the same shape as the existing
/// AddHealthChecks() call in each host's Program.cs. The instrumentation
/// catalogue callers reference directly (ActivitySource, SafeTags, ...)
/// lives in Nexus1.BuildingBlocks.Observability, not here.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNexusObservability(this IServiceCollection services, NexusObservabilityOptions options) => services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName: options.ServiceName, serviceNamespace: "nexus1"))
        .WithTracing(tracing => tracing
            // AlwaysOn = ch.51's "evidence" sampling profile (51-T) — deterministic
            // local/campaign proof, not a production sampling-rate decision.
            .SetSampler(new AlwaysOnSampler())
            .AddSource([.. NexusActivitySources.All])
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation()
            .AddOtlpExporter(exporter => exporter.Endpoint = options.OtlpEndpoint))
        .Services;
}
