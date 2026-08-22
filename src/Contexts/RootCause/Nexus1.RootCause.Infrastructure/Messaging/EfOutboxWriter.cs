using System.Diagnostics;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
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

        // Captured beside the row at commit time, not read later by the
        // dispatcher — the caller's own owner span, if any (ch.51 51-I).
        var traceSnapshot = ProducerTraceSnapshot.Capture(Activity.Current);

        var outboxMessage = new OutboxMessage(
            messageId, Producer, eventType, schemaVersion, routingKey, "application/json",
            occurredAtUtc, DateTime.UtcNow, envelope.EnvelopeBytes, envelope.EnvelopeSha256, traceSnapshot);

        dbContext.OutboxMessages.Add(outboxMessage);
    }
}
