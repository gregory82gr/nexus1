namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// One owner read's worth of outbox state (ch.52 52-G's "SNAPSHOT
/// CONSISTENCY" — count, oldest timestamp and observed time come from one
/// query, not three joined at different instants).
/// </summary>
public sealed record OutboxMetricSnapshot(long Pending, DateTimeOffset? OldestOccurredUtc, DateTimeOffset ObservedUtc)
{
    public double OldestAgeSeconds => OldestOccurredUtc is null
        ? 0
        : Math.Max(0, (ObservedUtc - OldestOccurredUtc.Value).TotalSeconds);
}

/// <summary>Per-context outbox reader (ch.52 52-G) — one EF query implementation per context, "duplication until proven" like the retry/poison readers.</summary>
public interface IOutboxMetricSnapshotReader
{
    ValueTask<OutboxMetricSnapshot> ReadAsync(CancellationToken cancellationToken);
}
