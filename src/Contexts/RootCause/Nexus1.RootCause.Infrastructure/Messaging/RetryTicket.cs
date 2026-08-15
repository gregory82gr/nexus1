namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Persistence-only, reduced from From_Services_To_Runtime's messaging.RetryTicket
/// (Executable Asset 29-O) per ADR-009: plain (unencrypted) EnvelopeBytes —
/// no ProtectedEnvelope/ProtectionKeyId, since this repo has no key-management
/// infrastructure anywhere else and OutboxMessage already stores envelopes
/// unencrypted; no LeaseOwner/LeaseUntilUtc — single-instance RootCause.Host
/// has nothing to lease-protect against, same rationale phase (a) already
/// used for OutboxMessage; no ReplayGeneration/State-machine columns — no
/// operator replay workflow exists yet. PublishedAtUtc is this table's
/// reduced stand-in for the book's fuller state machine (null = due/pending,
/// non-null = dispatched by RetryDispatcher), mirroring OutboxMessage's own
/// ProcessedAtUtc pattern.
/// </summary>
public sealed class RetryTicket
{
    private RetryTicket()
    {
        ConsumerName = null!;
        PolicyId = null!;
        FailureCode = null!;
        Producer = null!;
        EventType = null!;
        OriginalRoutingKey = null!;
        EnvelopeBytes = null!;
        EnvelopeSha256 = null!;
    }

    public RetryTicket(
        Guid retryTicketId, string consumerName, Guid messageId, int attempt, string policyId, string failureCode,
        DateTime firstFailedAtUtc, DateTime dueAtUtc, string producer, string eventType, int schemaVersion,
        string originalRoutingKey, byte[] envelopeBytes, byte[] envelopeSha256, DateTime createdAtUtc)
    {
        RetryTicketId = retryTicketId;
        ConsumerName = consumerName;
        MessageId = messageId;
        Attempt = attempt;
        PolicyId = policyId;
        FailureCode = failureCode;
        FirstFailedAtUtc = firstFailedAtUtc;
        DueAtUtc = dueAtUtc;
        Producer = producer;
        EventType = eventType;
        SchemaVersion = schemaVersion;
        OriginalRoutingKey = originalRoutingKey;
        EnvelopeBytes = envelopeBytes;
        EnvelopeSha256 = envelopeSha256;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid RetryTicketId { get; private set; }

    public string ConsumerName { get; private set; }

    public Guid MessageId { get; private set; }

    public int Attempt { get; private set; }

    public string PolicyId { get; private set; }

    public string FailureCode { get; private set; }

    public DateTime FirstFailedAtUtc { get; private set; }

    public DateTime DueAtUtc { get; private set; }

    public string Producer { get; private set; }

    public string EventType { get; private set; }

    public int SchemaVersion { get; private set; }

    public string OriginalRoutingKey { get; private set; }

    public byte[] EnvelopeBytes { get; private set; }

    public byte[] EnvelopeSha256 { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public void MarkPublished(DateTime publishedAtUtc) => PublishedAtUtc = publishedAtUtc;
}
