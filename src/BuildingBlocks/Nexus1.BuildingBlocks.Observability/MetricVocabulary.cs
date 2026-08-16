using System.Collections.Frozen;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Reviewed, closed label-value domains (ch.52 52-B/52-F). Values are
/// deliberately reused from what this project already established for
/// tracing rather than a second, divergent vocabulary: Outcomes matches the
/// COMMITTED/REJECTED/ABSTAINED/DUPLICATE_MATCH set every owner span already
/// tags via `nexus1.outcome.code` (plus FAILED, ch.52 52-Q's distinct
/// "operational failure" dimension), and Components matches
/// <see cref="NexusActivitySources.All"/> exactly — the same seven names
/// already reviewed for trace sources.
/// </summary>
public static class MetricVocabulary
{
    /// <summary>
    /// "publish"/"process" are the two messaging-lifecycle stages this
    /// project's shared publisher/consumer path actually has (ch.52 52-K,
    /// reduced from create/send/receive/process/settle to what exists);
    /// "retry-dispatch" is background rework, not a fresh attempt;
    /// "alarm-to-verdict" names the one end-to-end workflow this project
    /// measures (ch.52 52-T) rather than introducing a fifth tag key
    /// (`nexus1.workflow`) for a single-element domain.
    /// </summary>
    public static readonly FrozenSet<string> Operations = new[]
    {
        "publish", "process", "retry-dispatch", "alarm-to-verdict",
    }.ToFrozenSet();

    public static readonly FrozenSet<string> Outcomes = new[]
    {
        "COMMITTED", "REJECTED", "ABSTAINED", "DUPLICATE_MATCH", "FAILED",
    }.ToFrozenSet();

    public static readonly FrozenSet<string> Components = NexusActivitySources.All.ToFrozenSet();

    public static readonly FrozenSet<string> ErrorTypes = ErrorClassifier.Vocabulary;
}
