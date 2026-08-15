using System.Diagnostics;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

public sealed class ProducerTraceSnapshotTests
{
    [Fact]
    public void Capture_returns_null_for_a_null_activity()
    {
        Assert.Null(ProducerTraceSnapshot.Capture(null));
    }

    [Fact]
    public void Capture_and_round_trip_preserve_trace_and_span_identity()
    {
        using var source = new ActivitySource("test-source-producer-snapshot");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = source.StartActivity("commit");
        Assert.NotNull(activity);

        var snapshot = ProducerTraceSnapshot.Capture(activity);

        Assert.NotNull(snapshot);
        Assert.Equal(activity!.TraceId.ToHexString(), snapshot!.TraceId);
        Assert.Equal(activity.SpanId.ToHexString(), snapshot.SpanId);
        Assert.Equal((byte)activity.ActivityTraceFlags, snapshot.TraceFlags);

        var restored = snapshot.ToActivityContext();
        Assert.Equal(activity.TraceId, restored.TraceId);
        Assert.Equal(activity.SpanId, restored.SpanId);
    }
}
