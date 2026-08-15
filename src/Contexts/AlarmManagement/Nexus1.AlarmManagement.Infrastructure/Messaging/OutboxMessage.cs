namespace Nexus1.AlarmManagement.Infrastructure.Messaging;

/// <summary>
/// Persistence-only — not a domain concept, no Entity&lt;TId&gt;/IAggregateRoot.
/// Column shape matches the identity/integrity columns From_Services_To_Runtime's
/// messaging.OutboxMessage specifies (ADR-008); the retry/lease columns
/// (State enum's Retryable/Quarantined values, AttemptCount, NextAttemptAtUtc,
/// LockedBy, LockExpiresAtUtc) are phase (b) — ProcessedAtUtc is phase (a)'s
/// reduced stand-in (null = pending, non-null = dispatched).
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
        Producer = null!;
        EventType = null!;
        RoutingKey = null!;
        ContentType = null!;
        EnvelopeBytes = null!;
        EnvelopeSha256 = null!;
    }

    public OutboxMessage(
        Guid messageId, string producer, string eventType, int schemaVersion, string routingKey,
        string contentType, DateTime occurredAtUtc, DateTime storedAtUtc, byte[] envelopeBytes, byte[] envelopeSha256)
    {
        MessageId = messageId;
        Producer = producer;
        EventType = eventType;
        SchemaVersion = schemaVersion;
        RoutingKey = routingKey;
        ContentType = contentType;
        OccurredAtUtc = occurredAtUtc;
        StoredAtUtc = storedAtUtc;
        EnvelopeBytes = envelopeBytes;
        EnvelopeSha256 = envelopeSha256;
    }

    public Guid MessageId { get; private set; }

    public string Producer { get; private set; }

    public string EventType { get; private set; }

    public int SchemaVersion { get; private set; }

    public string RoutingKey { get; private set; }

    public string ContentType { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime StoredAtUtc { get; private set; }

    public byte[] EnvelopeBytes { get; private set; }

    public byte[] EnvelopeSha256 { get; private set; }

    public DateTime? ProcessedAtUtc { get; private set; }

    public void MarkProcessed(DateTime processedAtUtc) => ProcessedAtUtc = processedAtUtc;
}
