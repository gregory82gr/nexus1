namespace Nexus1.Reporting.Infrastructure.Messaging;

/// <summary>
/// Mirrors Nexus1.Audit/Compliance.Infrastructure.Messaging.InboxReceipt
/// exactly (ADR-008/ADR-012) — transport-level dedup only. The semantic
/// half of dedup for both event types lives on RootCauseCaseSummary's own
/// PK and PendingVerdict's own PK, not here.
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
