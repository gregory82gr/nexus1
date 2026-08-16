using System.Diagnostics.Metrics;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

/// <summary>Cached-observable-gauge proof (ch.52 52-H/52-M) — the collection callback never touches a database, only Volatile-safe published state; RecordObservableInstruments() triggers the same callback path a real collector's scrape/export would.</summary>
public sealed class OutboxMetricStateTests
{
    private sealed record CapturedGauge(string InstrumentName, object Value, string? Component);

    /// <summary>
    /// MeterListener triggers callbacks for every currently-published
    /// observable instrument across the process, not just ones created by
    /// "this" factory — an undisposed listener/Meter from an earlier test
    /// leaks its gauges into a later test's RecordObservableInstruments()
    /// call (the same class of cross-test leakage the tracing
    /// ActivityListener pattern hit). Disposing both at the end of every
    /// test is what keeps this deterministic.
    /// </summary>
    private sealed class ListenedState : IDisposable
    {
        private readonly TestMeterFactory _factory;
        private readonly MeterListener _listener;
        private readonly List<CapturedGauge> _captured = [];

        public ListenedState()
        {
            _factory = new TestMeterFactory();
            _listener = new MeterListener
            {
                InstrumentPublished = (instrument, listenerHandle) =>
                {
                    if (instrument.Meter.Name == NexusRuntimeMetrics.MeterName)
                    {
                        listenerHandle.EnableMeasurementEvents(instrument);
                    }
                },
            };
            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                _captured.Add(new CapturedGauge(instrument.Name, value, FindComponent(tags))));
            _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                _captured.Add(new CapturedGauge(instrument.Name, value, FindComponent(tags))));

            State = new OutboxMetricState(_factory);
            _listener.Start();
        }

        public OutboxMetricState State { get; }

        public List<CapturedGauge> Read()
        {
            _captured.Clear();
            _listener.RecordObservableInstruments();
            return _captured;
        }

        public void Dispose()
        {
            _listener.Dispose();
            _factory.Dispose();
        }
    }

    private static string? FindComponent(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == "nexus1.component")
            {
                return (string?)tag.Value;
            }
        }
        return null;
    }

    [Fact]
    public void A_component_with_no_published_snapshot_yet_emits_no_measurement()
    {
        using var session = new ListenedState();

        Assert.Empty(session.Read());
    }

    [Fact]
    public void Publish_makes_pending_and_oldest_age_observable()
    {
        using var session = new ListenedState();
        var observedUtc = DateTimeOffset.UtcNow;
        var oldestUtc = observedUtc.AddSeconds(-30);

        session.State.Publish(NexusActivitySources.RootCause, new OutboxMetricSnapshot(3, oldestUtc, observedUtc));

        var captured = session.Read();

        var pending = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxPending);
        Assert.Equal(3L, pending.Value);
        Assert.Equal(NexusActivitySources.RootCause, pending.Component);

        var oldestAge = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxOldestAge);
        Assert.InRange((double)oldestAge.Value, 29.5, 31.0);
    }

    [Fact]
    public void Zero_pending_is_only_meaningful_beside_a_fresh_snapshot_age()
    {
        using var session = new ListenedState();
        session.State.Publish(NexusActivitySources.RootCause, new OutboxMetricSnapshot(0, null, DateTimeOffset.UtcNow));

        var captured = session.Read();

        var pending = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxPending);
        Assert.Equal(0L, pending.Value);

        var snapshotAge = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxSnapshotAge);
        Assert.InRange((double)snapshotAge.Value, 0.0, 2.0);

        var oldestAge = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxOldestAge);
        Assert.Equal(0.0, oldestAge.Value);
    }

    [Fact]
    public void A_stale_unrefreshed_snapshot_shows_growing_snapshot_age_not_a_reset_to_zero()
    {
        using var session = new ListenedState();
        var staleObservedUtc = DateTimeOffset.UtcNow.AddMinutes(-10);
        session.State.Publish(NexusActivitySources.RootCause, new OutboxMetricSnapshot(5, staleObservedUtc, staleObservedUtc));

        var captured = session.Read();

        var snapshotAge = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxSnapshotAge);
        Assert.True((double)snapshotAge.Value >= 590);

        var pending = Assert.Single(captured, c => c.InstrumentName == MetricNames.OutboxPending);
        Assert.Equal(5L, pending.Value);
    }

    [Fact]
    public void Multiple_components_each_get_their_own_tagged_series()
    {
        using var session = new ListenedState();
        var now = DateTimeOffset.UtcNow;
        session.State.Publish(NexusActivitySources.RootCause, new OutboxMetricSnapshot(1, now, now));
        session.State.Publish(NexusActivitySources.AlarmManagement, new OutboxMetricSnapshot(2, now, now));

        var captured = session.Read();

        var pendingByComponent = captured.Where(c => c.InstrumentName == MetricNames.OutboxPending).ToList();
        Assert.Equal(2, pendingByComponent.Count);
        Assert.Contains(pendingByComponent, c => c.Component == NexusActivitySources.RootCause && (long)c.Value == 1);
        Assert.Contains(pendingByComponent, c => c.Component == NexusActivitySources.AlarmManagement && (long)c.Value == 2);
    }

    [Fact]
    public void CurrentOrDefault_returns_the_last_published_snapshot()
    {
        using var session = new ListenedState();
        Assert.Null(session.State.CurrentOrDefault(NexusActivitySources.RootCause));

        var snapshot = new OutboxMetricSnapshot(7, null, DateTimeOffset.UtcNow);
        session.State.Publish(NexusActivitySources.RootCause, snapshot);

        Assert.Equal(snapshot, session.State.CurrentOrDefault(NexusActivitySources.RootCause));
    }
}
