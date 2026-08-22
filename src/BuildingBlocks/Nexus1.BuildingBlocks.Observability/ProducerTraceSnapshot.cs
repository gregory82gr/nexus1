using System.Diagnostics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Optional trace coordinates captured beside an outbox row at the business
/// commit (ch.51 51-I/51-K). Nullable so sampling or an absent listener
/// never blocks the commit; stored beside the outbox row (not read from
/// ambient Activity.Current by the dispatcher, which runs later/elsewhere);
/// used only as an ActivityLink, never as authority — business cause
/// remains CausationId (ch.51 "SNAPSHOT RULES").
/// </summary>
public sealed record ProducerTraceSnapshot(string TraceId, string SpanId, byte TraceFlags, string? TraceState)
{
    public static ProducerTraceSnapshot? Capture(Activity? activity) =>
        activity is null
            ? null
            : new ProducerTraceSnapshot(
                activity.TraceId.ToHexString(),
                activity.SpanId.ToHexString(),
                (byte)activity.ActivityTraceFlags,
                activity.TraceStateString);

    public ActivityContext ToActivityContext() =>
        new(ActivityTraceId.CreateFromString(TraceId), ActivitySpanId.CreateFromString(SpanId), (ActivityTraceFlags)TraceFlags, TraceState);
}
