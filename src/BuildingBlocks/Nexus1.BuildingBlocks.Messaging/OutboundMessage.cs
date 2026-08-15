using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Messaging;

public sealed record OutboundMessage(
    Guid MessageId,
    string EventType,
    int SchemaVersion,
    string Producer,
    string RoutingKey,
    byte[] EnvelopeBytes,
    byte[] EnvelopeSha256,
    Guid? CorrelationId,
    Guid? CausationId,
    ProducerTraceSnapshot? TraceSnapshot = null);
