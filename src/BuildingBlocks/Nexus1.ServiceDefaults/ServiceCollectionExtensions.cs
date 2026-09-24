using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nexus1.ServiceDefaults;

/// <summary>
/// Host-composition-root registration only (ADR-013/ADR-014) — the actual
/// OpenTelemetry SDK wiring belongs here, the same shape as the existing
/// AddHealthChecks() call in each host's Program.cs. The instrumentation
/// catalogue callers reference directly (ActivitySource, NexusRuntimeMetrics,
/// SafeTags, ...) lives in Nexus1.BuildingBlocks.Observability, not here.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNexusObservability(this IServiceCollection services, NexusObservabilityOptions options)
    {
        services.AddSingleton<NexusRuntimeMetrics>();
        services.AddSingleton<NexusDiagnosticsMetrics>();
        services.AddSingleton<OutboxMetricState>();

        services
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
            .WithMetrics(metrics => metrics
                .AddMeter(NexusRuntimeMetrics.MeterName)
                // Diagnosis/RAG-pipeline meter (Appendix I; ADR-038).
                .AddMeter(NexusDiagnosticsMetrics.MeterName)
                // Bucket boundaries scoped to what this project actually
                // measures (ch.52 52-AA): sub-second broker attempts through
                // low tens-of-seconds workflow durations, not the book's
                // separate transport/workflow bucket schemas — one histogram
                // view stands in for both since MessageDuration is the only
                // sub-workflow duration histogram this step instruments.
                .AddView(MetricNames.MessageDuration, new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = [.005, .01, .025, .05, .1, .25, .5, 1, 2.5, 5, 10],
                })
                .AddView(MetricNames.WorkflowDuration, new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = [.1, .25, .5, 1, 2.5, 5, 10, 30, 60, 120, 300],
                })
                // Diagnosis latency is in MILLISECONDS and spans warm (~29s) to cold
                // (~122s) model loads (ADR-038); boundaries chosen to resolve that
                // bimodal split around the 30s warm and 130s cold clusters.
                .AddView(MetricNames.DiagnosisDuration, new ExplicitBucketHistogramConfiguration
                {
                    Boundaries = [100, 500, 1000, 5000, 10000, 20000, 25000, 30000, 40000, 60000, 100000, 130000, 180000],
                })
                .AddOtlpExporter((exporter, metricReader) =>
                {
                    exporter.Endpoint = options.OtlpEndpoint;
                    // Same "evidence" rationale as AlwaysOnSampler above: the
                    // OTLP metrics SDK's default periodic export interval is
                    // 60s, tuned for production, not for a local campaign
                    // that scrapes moments after a stimulus and needs the
                    // export to have already happened. 2s is a campaign
                    // constant, not a production-latency decision this
                    // project makes no claim about (caught running the
                    // RootCause complete metrics campaign — a scrape 5s
                    // after the stimulus saw nothing, though the export was
                    // genuinely queued and appeared moments later).
                    metricReader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 2000;
                })
                // Prometheus scrape exporter (Appendix I; ADR-038), added ALONGSIDE
                // the OTLP exporter above -- OTLP keeps feeding the collector for
                // tracing and the ch.52 campaigns; Prometheus scrapes /metrics on the
                // host that maps the endpoint (RootCause.Host only, for now). Every
                // registered meter above becomes scrapeable, so the existing
                // Nexus1.Runtime messaging metrics are exposed too, as a side benefit.
                .AddPrometheusExporter());

        return services;
    }
}
