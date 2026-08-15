namespace Nexus1.Audit.Infrastructure.Messaging;

/// <summary>Mirrors Nexus1.RootCause.Infrastructure.Messaging.PoisonMessage exactly (ADR-009/ADR-010).</summary>
public sealed class PoisonMessage
{
    private PoisonMessage()
    {
        ConsumerName = null!;
        EventType = null!;
        TerminalReason = null!;
        EnvelopeSha256 = null!;
    }

    public PoisonMessage(
        Guid poisonMessageId, string consumerName, Guid messageId, byte[] envelopeSha256, string eventType,
        int schemaVersion, string terminalReason, int retryAttempts, DateTime firstFailedAtUtc, DateTime quarantinedAtUtc)
    {
        PoisonMessageId = poisonMessageId;
        ConsumerName = consumerName;
        MessageId = messageId;
        EnvelopeSha256 = envelopeSha256;
        EventType = eventType;
        SchemaVersion = schemaVersion;
        TerminalReason = terminalReason;
        RetryAttempts = retryAttempts;
        FirstFailedAtUtc = firstFailedAtUtc;
        QuarantinedAtUtc = quarantinedAtUtc;
    }

    public Guid PoisonMessageId { get; private set; }

    public string ConsumerName { get; private set; }

    public Guid MessageId { get; private set; }

    public byte[] EnvelopeSha256 { get; private set; }

    public string EventType { get; private set; }

    public int SchemaVersion { get; private set; }

    public string TerminalReason { get; private set; }

    public int RetryAttempts { get; private set; }

    public DateTime FirstFailedAtUtc { get; private set; }

    public DateTime QuarantinedAtUtc { get; private set; }
}
