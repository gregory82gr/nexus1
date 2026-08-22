namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Closed, reviewed instrument-name catalogue (ch.52 52-C). Scoped to what
/// this project actually has a real seam for: no `nexus1.edge.requests`
/// (ADR-007 deferred the BFF, so there is no HTTP edge to count attempts
/// against yet) and no `nexus1.projection.lag` in this step (RootCause, the
/// proof context, owns no projection — Reporting does, deferred to when
/// tracing/metrics extend there).
/// </summary>
public static class MetricNames
{
    public const string MessageAttempts = "nexus1.message.attempts";
    public const string MessageDuration = "nexus1.message.duration";
    public const string OutboxPending = "nexus1.outbox.pending";
    public const string OutboxOldestAge = "nexus1.outbox.oldest_age";

    /// <summary>Not from the book's core catalogue — the added third outbox
    /// gauge that makes staleness a first-class, separately-exported state
    /// (ch.52 52-M's "ZERO IS DATA"/"staleness exported separately" rules)
    /// rather than folding it into the pending/oldest-age values themselves.</summary>
    public const string OutboxSnapshotAge = "nexus1.outbox.snapshot_age";
    public const string InboxOutcomes = "nexus1.inbox.outcomes";
    public const string WorkflowDuration = "nexus1.workflow.duration";
}
