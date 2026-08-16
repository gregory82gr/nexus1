using System.Diagnostics.Metrics;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

/// <summary>MeterListener-based instrument proof (ch.52 52-AB) — names, types, values and tags verified in process, no collector involved.</summary>
public sealed class NexusRuntimeMetricsTests
{
    private sealed record CapturedMeasurement(string InstrumentName, object Value, IReadOnlyDictionary<string, object?> Tags);

    private static List<CapturedMeasurement> CaptureMeasurements(Action<NexusRuntimeMetrics> record)
    {
        using var factory = new TestMeterFactory();
        var metrics = new NexusRuntimeMetrics(factory);

        var captured = new List<CapturedMeasurement>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listenerHandle) =>
        {
            if (instrument.Meter.Name == NexusRuntimeMetrics.MeterName)
            {
                listenerHandle.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            captured.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            captured.Add(new CapturedMeasurement(instrument.Name, value, ToDictionary(tags))));
        listener.Start();

        record(metrics);

        return captured;
    }

    private static Dictionary<string, object?> ToDictionary(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var dictionary = new Dictionary<string, object?>();
        foreach (var tag in tags)
        {
            dictionary[tag.Key] = tag.Value;
        }
        return dictionary;
    }

    [Fact]
    public void MessageAttempts_records_the_bounded_labels()
    {
        var captured = CaptureMeasurements(metrics =>
        {
            Assert.True(MetricLabelPolicy.TryFor("publish", "COMMITTED", NexusActivitySources.Messaging, out var labels));
            metrics.MessageAttempts.Add(1, labels.ToTagList());
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.MessageAttempts);
        Assert.Equal(1L, measurement.Value);
        Assert.Equal("publish", measurement.Tags["nexus1.operation"]);
        Assert.Equal("COMMITTED", measurement.Tags["nexus1.outcome"]);
        Assert.Equal(NexusActivitySources.Messaging, measurement.Tags["nexus1.component"]);
    }

    [Fact]
    public void MessageDuration_records_elapsed_seconds()
    {
        var captured = CaptureMeasurements(metrics =>
        {
            Assert.True(MetricLabelPolicy.TryFor("process", "COMMITTED", NexusActivitySources.Messaging, out var labels));
            metrics.MessageDuration.Record(0.042, labels.ToTagList());
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.MessageDuration);
        Assert.Equal(0.042, measurement.Value);
    }

    [Fact]
    public void InboxOutcomes_records_one_terminal_observation_per_admission()
    {
        var captured = CaptureMeasurements(metrics =>
        {
            Assert.True(MetricLabelPolicy.TryFor("process", "DUPLICATE_MATCH", NexusActivitySources.RootCause, out var labels));
            metrics.InboxOutcomes.Add(1, labels.ToTagList());
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.InboxOutcomes);
        Assert.Equal("DUPLICATE_MATCH", measurement.Tags["nexus1.outcome"]);
    }

    [Fact]
    public void WorkflowDuration_records_the_single_reviewed_workflow_operation()
    {
        var captured = CaptureMeasurements(metrics =>
        {
            Assert.True(MetricLabelPolicy.TryFor("alarm-to-verdict", "COMMITTED", NexusActivitySources.RootCause, out var labels));
            metrics.WorkflowDuration.Record(4.2, labels.ToTagList());
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == MetricNames.WorkflowDuration);
        Assert.Equal(4.2, measurement.Value);
        Assert.Equal("alarm-to-verdict", measurement.Tags["nexus1.operation"]);
    }

    [Fact]
    public void TelemetryRejected_is_where_an_admission_gate_failure_goes_instead_of_a_fabricated_series()
    {
        var captured = CaptureMeasurements(metrics =>
        {
            var admitted = MetricLabelPolicy.TryFor("not-a-reviewed-operation", "COMMITTED", NexusActivitySources.RootCause, out _);
            Assert.False(admitted);
            metrics.TelemetryRejected.Add(1);
        });

        var measurement = Assert.Single(captured, m => m.InstrumentName == "nexus1.telemetry.rejected");
        Assert.Equal(1L, measurement.Value);
        Assert.DoesNotContain(captured, m => m.InstrumentName == MetricNames.MessageAttempts);
    }

    [Fact]
    public void Recording_is_safe_when_no_listener_subscribes()
    {
        using var factory = new TestMeterFactory();
        var metrics = new NexusRuntimeMetrics(factory);

        Assert.True(MetricLabelPolicy.TryFor("publish", "COMMITTED", NexusActivitySources.Messaging, out var labels));
        metrics.MessageAttempts.Add(1, labels.ToTagList());
    }
}
