using System.Diagnostics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Bounded tag projection for metric measurements (ch.52 52-B). Callers
/// build this only through <see cref="MetricLabelPolicy.TryFor"/>, never by
/// constructing it directly from unreviewed strings — the four keys
/// (`nexus1.operation`, `nexus1.outcome`, `nexus1.component`, `error.type`)
/// match the same bounded discipline already established for trace
/// attributes (<see cref="SafeTags"/>), reused here rather than duplicated.
/// </summary>
public readonly record struct MetricLabels(string Operation, string Outcome, string Component, string? ErrorType = null)
{
    public TagList ToTagList()
    {
        var tags = new TagList
        {
            { "nexus1.operation", Operation },
            { "nexus1.outcome", Outcome },
            { "nexus1.component", Component },
        };

        if (ErrorType is not null)
        {
            tags.Add("error.type", ErrorType);
        }

        return tags;
    }
}

/// <summary>
/// The admission gate ch.52 52-F requires: a measurement whose operation,
/// outcome or component falls outside the reviewed vocabulary is never
/// admitted as a new series. <see cref="TryFor"/> returning false means the
/// caller records nothing on the real instrument and instead increments
/// <c>NexusRuntimeMetrics.TelemetryRejected</c> — one bounded, always-safe
/// counter — rather than inventing a placeholder label value that would
/// itself need to be a reviewed vocabulary member.
/// </summary>
public static class MetricLabelPolicy
{
    public static bool TryFor(string operation, string outcome, string component, out MetricLabels labels)
    {
        if (MetricVocabulary.Operations.Contains(operation)
            && MetricVocabulary.Outcomes.Contains(outcome)
            && MetricVocabulary.Components.Contains(component))
        {
            labels = new MetricLabels(operation, outcome, component);
            return true;
        }

        labels = default;
        return false;
    }

    /// <summary>error.type is always machine-classified (<see cref="ErrorClassifier"/>), never caller-supplied text, so it is safe-by-construction — this overload attaches it without re-validating a value that cannot be out of vocabulary.</summary>
    public static bool TryFor(string operation, string outcome, string component, string errorType, out MetricLabels labels)
    {
        if (!TryFor(operation, outcome, component, out labels))
        {
            return false;
        }

        labels = labels with { ErrorType = errorType };
        return true;
    }
}
