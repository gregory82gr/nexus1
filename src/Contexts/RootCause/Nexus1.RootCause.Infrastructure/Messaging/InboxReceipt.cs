namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Persistence-only, dedup key (ConsumerName, MessageId) per
/// From_Services_To_Runtime ch.28 (Executable Asset 28-F), adopted close to
/// verbatim — dedup isn't a retry concern, so unlike the outbox table
/// nothing here is deferred to phase (b) (ADR-008). "ConsumerName" scopes
/// the dedup key per subscription, not globally.
/// </summary>
public sealed class InboxReceipt
{
    private InboxReceipt()
    {
        ConsumerName = null!;
        Producer = null!;
        EventType = null!;
    }

    public InboxReceipt(
        string consumerName, Guid messageId, string producer, string eventType, int schemaVersion,
        DateTime occurredAtUtc, DateTime receivedAtUtc)
    {
        ConsumerName = consumerName;
        MessageId = messageId;
        Producer = producer;
        EventType = eventType;
        SchemaVersion = schemaVersion;
        OccurredAtUtc = occurredAtUtc;
        ReceivedAtUtc = receivedAtUtc;
        CompletedAtUtc = receivedAtUtc;
    }

    public string ConsumerName { get; private set; }

    public Guid MessageId { get; private set; }

    public string Producer { get; private set; }

    public string EventType { get; private set; }

    public int SchemaVersion { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime ReceivedAtUtc { get; private set; }

    public DateTime CompletedAtUtc { get; private set; }
}
