namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Closed, reviewed operation-name catalogue (ch.51 51-D). Names describe
/// operations, never instances — a query groups spans by name without
/// parsing identifiers (ch.51 "CARDINALITY INVARIANT").
/// </summary>
public static class SpanNames
{
    // RootCause owner operations (INTERNAL spans).
    public const string RootCauseCaseOpen = "root-cause case open";
    public const string AddHypothesis = "root-cause add hypothesis";
    public const string AddEvidence = "root-cause add evidence";
    public const string RootCauseVerdictCommit = "root-cause verdict commit";

    // Background work (INTERNAL spans).
    public const string RetryDispatch = "retry dispatch attempt";
    public const string OutboxDispatch = "outbox dispatch attempt";

    /// <summary>
    /// Messaging spans are named from the message's own reviewed EventType
    /// constant, not a per-context static list — RabbitMqBrokerPublisher and
    /// the consumer background services are shared code used by every
    /// context, unlike the book's single-event-type dispatcher example
    /// (51-J). EventType is itself a closed, low-cardinality, compile-time
    /// constant at every call site (never a MessageId or other per-instance
    /// locator), so deriving the name from it does not violate the
    /// cardinality invariant.
    /// </summary>
    public static string ForPublish(string eventType) => $"publish {eventType}";

    public static string ForProcess(string eventType) => $"process {eventType}";
}
