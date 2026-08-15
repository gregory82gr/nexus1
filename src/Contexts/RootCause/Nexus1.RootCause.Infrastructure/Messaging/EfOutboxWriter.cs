using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

internal sealed class EfOutboxWriter(RootCauseDbContext dbContext) : IOutboxWriter
{
    private const string Producer = "root-cause";

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
