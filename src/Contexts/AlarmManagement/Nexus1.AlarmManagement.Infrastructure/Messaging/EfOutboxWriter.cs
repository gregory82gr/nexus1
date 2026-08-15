using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Messaging;

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

        var outboxMessage = new OutboxMessage(
            messageId, Producer, eventType, schemaVersion, routingKey, "application/json",
            occurredAtUtc, DateTime.UtcNow, envelope.EnvelopeBytes, envelope.EnvelopeSha256);

        dbContext.OutboxMessages.Add(outboxMessage);
    }
}
