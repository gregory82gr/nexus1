using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// One DI-managed instrument set for the process lifetime (ch.52 52-A/52-E).
/// Scoped to this project's actual seams: no `nexus1.edge.requests` counter
/// (no BFF exists, ADR-007) and no projection-lag histogram in this step
/// (RootCause, the proof context, owns no projection). `TelemetryRejected`
/// is this project's addition, not from the book's core catalogue — the
/// admission gate (<see cref="MetricLabelPolicy"/>, ch.52 52-F) needs
/// somewhere bounded to count a rejected measurement, and a metric that can
/// itself never go out of budget is the only safe destination.
/// </summary>
public sealed class NexusRuntimeMetrics
{
    public const string MeterName = "Nexus1.Runtime";

    public Counter<long> MessageAttempts { get; }

    public Histogram<double> MessageDuration { get; }

    public Counter<long> InboxOutcomes { get; }

    public Histogram<double> WorkflowDuration { get; }

    public Counter<long> TelemetryRejected { get; }

    public NexusRuntimeMetrics(IMeterFactory factory)
    {
        var meter = factory.Create(MeterName);

        MessageAttempts = meter.CreateCounter<long>(MetricNames.MessageAttempts, "{attempt}");
        MessageDuration = meter.CreateHistogram<double>(MetricNames.MessageDuration, "s");
        InboxOutcomes = meter.CreateCounter<long>(MetricNames.InboxOutcomes, "{message}");
        WorkflowDuration = meter.CreateHistogram<double>(MetricNames.WorkflowDuration, "s");
        TelemetryRejected = meter.CreateCounter<long>("nexus1.telemetry.rejected", "{rejection}");
    }
}
