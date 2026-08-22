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

    // AlarmManagement owner operations (INTERNAL spans).
    public const string AlarmAcknowledge = "alarm acknowledge";
    public const string AlarmDefine = "alarm define";
    public const string AlarmFloodCommit = "alarm flood commit"; // matches ch.51's own illustrative name (51-B)
    public const string AlarmEvaluateReading = "alarm evaluate reading";

    // Audit owner operation (INTERNAL span).
    public const string AuditEvidenceRecord = "audit evidence record";

    // Compliance owner operation (INTERNAL span).
    public const string ComplianceReviewOpen = "compliance review open";

    // Reporting owner operations — one per reducer (INTERNAL spans).
    public const string ReportingApplyOpened = "reporting apply case-opened";
    public const string ReportingApplyVerdictIssued = "reporting apply verdict-issued";

    /// <summary>
    /// Background work (INTERNAL spans). RetryDispatch is intentionally
    /// reused across every context's retry dispatcher — same conceptual
    /// operation, disambiguated by each context's own ActivitySource/
    /// resource identity, not by inventing four names for one operation
    /// type (cardinality invariant, ch.51).
    /// </summary>
    public const string RetryDispatch = "retry dispatch attempt";

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
