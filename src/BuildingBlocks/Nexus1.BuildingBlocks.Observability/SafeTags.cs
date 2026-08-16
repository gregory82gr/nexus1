using System.Diagnostics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Bounded, allow-listed attribute projection (ch.51 51-E/51-C). Payloads,
/// headers, tokens, claims, SQL text with values and exception messages
/// have no tag helper here and no approved attribute definition — this is
/// TB-07's confidentiality rule (ch.43 43-47) enforced in code, not by
/// convention.
/// </summary>
public static class SafeTags
{
    /// <summary>
    /// No correlation-id parameter: this codebase has no per-request
    /// "business context" concept threading a CorrelationId through every
    /// command (that is ch.49's territory, not read/built here) — tagging a
    /// freshly-fabricated id would misrepresent it as a real join key.
    /// MessageId is included only where a handler genuinely has one (e.g.
    /// the auto-open path processing an inbound message).
    /// </summary>
    public static ActivityTagsCollection ForOwnerOperation(Guid? messageId, string outcomeCode)
    {
        var tags = new ActivityTagsCollection
        {
            ["nexus1.outcome.code"] = outcomeCode,
        };

        if (messageId.HasValue)
        {
            tags["nexus1.message.id"] = messageId.Value.ToString("D");
        }

        return tags;
    }

    public static ActivityTagsCollection ForMessagePublish(Guid messageId, string eventType, string destinationName) => new()
    {
        ["nexus1.message.id"] = messageId.ToString("D"),
        ["nexus1.event.type"] = eventType,
        ["messaging.destination.name"] = destinationName,
    };

    public static ActivityTagsCollection ForMessageProcess(Guid messageId, string eventType) => new()
    {
        ["nexus1.message.id"] = messageId.ToString("D"),
        ["nexus1.event.type"] = eventType,
    };
}

/// <summary>
/// Safe, bounded error recording (ch.51 51-O). Exception.Message, stack
/// traces and provider text are omitted from the default trace profile —
/// only the classified error type is admitted (<see cref="ErrorClassifier"/>,
/// shared with metrics rather than duplicated, ch.52 52-Q), a closed,
/// reviewed five-value vocabulary rather than the raw CLR type name, which
/// is unbounded across every exception type any dependency could throw.
/// </summary>
public static class SafeError
{
    public static void Record(Activity? activity, Exception exception)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("error.type", ErrorClassifier.Classify(exception));
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
