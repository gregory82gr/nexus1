namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// The telemetry corner (From Flood to Cause, App C.3 / Ch.5): append-only
/// continuous samples per channel. The book puts this in a clustered
/// columnstore for metric scans at scale; the skeleton's incident-sized data
/// does not warrant that yet (deferred, ADR-032) -- a plain table keyed by
/// (ChannelId, TimestampUtc). The corroborator reads these to confirm an
/// edge's timing fit and to check whether an independent witness channel moved.
/// </summary>
public sealed class HistorianSample
{
    public int ChannelId { get; init; }

    public DateTime TimestampUtc { get; init; }

    public double Value { get; init; }

    /// <summary>Quality flag (0 = good), App C.3 Quality TINYINT.</summary>
    public byte Quality { get; init; }
}
