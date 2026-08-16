using System.Diagnostics;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.AlarmManagement.Infrastructure.Messaging;

internal sealed class EfOutboxWriter(AlarmManagementDbContext dbContext) : IOutboxWriter
{
    private const string Producer = "alarm-management";

    public void Enqueue(
        string eventType, int schemaVersion, string routingKey, DateTime occurredAtUtc,
        object payload, Guid? correlationId = null, Guid? causationId = null)
    {
        var messageId = Guid.NewGuid();
        var envelope = MessageEnvelopeFactory.Build(
            messageId, eventType, schemaVersion, occurredAtUtc, Producer,
            correlationId ?? Guid.NewGuid(), causationId, payload);

        // Captured beside the row at commit time, not read later by the
        // dispatcher — the caller's own owner span, if any (ch.51 51-I).
        var traceSnapshot = ProducerTraceSnapshot.Capture(Activity.Current);

        var outboxMessage = new OutboxMessage(
            messageId, Producer, eventType, schemaVersion, routingKey, "application/json",
            occurredAtUtc, DateTime.UtcNow, envelope.EnvelopeBytes, envelope.EnvelopeSha256, traceSnapshot);

        dbContext.OutboxMessages.Add(outboxMessage);
    }
}
