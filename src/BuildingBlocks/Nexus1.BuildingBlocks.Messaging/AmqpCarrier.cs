using System.Diagnostics;
using System.Text;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// Bounded W3C trace-context carrier over AMQP headers (ch.51 51-K).
/// Diagnostic only — validates grammar and rejects oversized headers rather
/// than trusting caller input, per TB-07's "trace-context injection:
/// diagnostic only; validate grammar" (ch.43 43-46). Never authenticates or
/// authorizes; a malformed or absent carrier degrades to a new diagnostic
/// root under route policy, it never rewrites business identity.
/// </summary>
public static class AmqpCarrier
{
    private const int MaxTraceParentLength = 55;
    private const int MaxTraceStateLength = 512;

    public static void Inject(ActivityContext context, IDictionary<string, object> headers)
    {
        if (context == default)
        {
            return;
        }

        var recorded = context.TraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00";
        var traceParent = $"00-{context.TraceId}-{context.SpanId}-{recorded}";
        if (traceParent.Length <= MaxTraceParentLength)
        {
            headers["traceparent"] = Encoding.ASCII.GetBytes(traceParent);
        }

        if (!string.IsNullOrEmpty(context.TraceState) && context.TraceState.Length <= MaxTraceStateLength)
        {
            headers["tracestate"] = Encoding.ASCII.GetBytes(context.TraceState);
        }
    }

    public static ActivityContext Extract(IDictionary<string, object>? headers)
    {
        if (headers is null || !TryReadHeaderString(headers, "traceparent", MaxTraceParentLength, out var traceParent))
        {
            return default;
        }

        var parts = traceParent.Split('-');
        if (parts.Length != 4 || parts[0] != "00" || parts[1].Length != 32 || parts[2].Length != 16 || parts[3].Length != 2)
        {
            return default;
        }

        try
        {
            var traceId = ActivityTraceId.CreateFromString(parts[1]);
            var spanId = ActivitySpanId.CreateFromString(parts[2]);
            var flags = parts[3] == "01" ? ActivityTraceFlags.Recorded : ActivityTraceFlags.None;
            var traceState = TryReadHeaderString(headers, "tracestate", MaxTraceStateLength, out var state) ? state : null;

            return new ActivityContext(traceId, spanId, flags, traceState, isRemote: true);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Malformed hex — treated the same as an absent carrier (51-AE:
            // "traceparent invalid -> reject/ignore by route policy; no inferred cause").
            return default;
        }
    }

    private static bool TryReadHeaderString(IDictionary<string, object> headers, string key, int maxLength, out string value)
    {
        value = string.Empty;
        if (!headers.TryGetValue(key, out var raw))
        {
            return false;
        }

        var bytes = raw switch
        {
            byte[] b => b,
            string s => Encoding.ASCII.GetBytes(s),
            _ => null,
        };

        if (bytes is null || bytes.Length == 0 || bytes.Length > maxLength)
        {
            return false;
        }

        value = Encoding.ASCII.GetString(bytes);
        return true;
    }
}
