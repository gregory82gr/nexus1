using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Cached observable state shared across every context's outbox (ch.52
/// 52-H). One instrument registration on the shared Meter, callbacks that
/// never touch a database — a background refresh worker per context calls
/// <see cref="Publish"/> on its own schedule; the collection callback only
/// ever reads what was last published (<c>Volatile</c>-safe via the
/// snapshot record's immutability and the concurrent dictionary).
/// A component with no published snapshot yet emits no measurement for
/// that component — absence, not a fabricated zero, is "no data yet".
/// </summary>
public sealed class OutboxMetricState
{
    private readonly ConcurrentDictionary<string, OutboxMetricSnapshot> _snapshots = new();

    public OutboxMetricState(IMeterFactory factory)
    {
        var meter = factory.Create(NexusRuntimeMetrics.MeterName);

        meter.CreateObservableGauge(MetricNames.OutboxPending, ObservePending, "{item}");
        meter.CreateObservableGauge(MetricNames.OutboxOldestAge, ObserveOldestAge, "s");
        meter.CreateObservableGauge(MetricNames.OutboxSnapshotAge, ObserveSnapshotAge, "s");
    }

    public void Publish(string component, OutboxMetricSnapshot snapshot) => _snapshots[component] = snapshot;

    /// <summary>Last-good value on a failed/timed-out refresh (ch.52 52-N's "LAST-GOOD RULE") — the caller simply does not call Publish, leaving the prior snapshot (and its aging ObservedUtc, visible via the snapshot-age gauge) in place rather than resetting to zero.</summary>
    public OutboxMetricSnapshot? CurrentOrDefault(string component) =>
        _snapshots.TryGetValue(component, out var snapshot) ? snapshot : null;

    private IEnumerable<Measurement<long>> ObservePending() => Observe(s => (long)s.Snapshot.Pending);

    private IEnumerable<Measurement<double>> ObserveOldestAge() => ObserveDouble(s => s.Snapshot.OldestAgeSeconds);

    private IEnumerable<Measurement<double>> ObserveSnapshotAge() =>
        ObserveDouble(s => Math.Max(0, (DateTimeOffset.UtcNow - s.Snapshot.ObservedUtc).TotalSeconds));

    private IEnumerable<Measurement<long>> Observe(Func<(string Component, OutboxMetricSnapshot Snapshot), long> select)
    {
        foreach (var pair in _snapshots)
        {
            if (MetricVocabulary.Components.Contains(pair.Key))
            {
                yield return new Measurement<long>(select((pair.Key, pair.Value)), new KeyValuePair<string, object?>("nexus1.component", pair.Key));
            }
        }
    }

    private IEnumerable<Measurement<double>> ObserveDouble(Func<(string Component, OutboxMetricSnapshot Snapshot), double> select)
    {
        foreach (var pair in _snapshots)
        {
            if (MetricVocabulary.Components.Contains(pair.Key))
            {
                yield return new Measurement<double>(select((pair.Key, pair.Value)), new KeyValuePair<string, object?>("nexus1.component", pair.Key));
            }
        }
    }
}
