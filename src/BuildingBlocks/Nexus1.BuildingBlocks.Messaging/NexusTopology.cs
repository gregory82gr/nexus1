using RabbitMQ.Client;

namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// Naming and topology adopted exactly from From_Services_To_Runtime ch.25.2
/// (ADR-008) — one shared topic exchange for all producers, quorum queues
/// per consumer subscription, no per-event-type exchange fan-out.
/// </summary>
public static class NexusTopology
{
    public const string EventsExchange = "nexus.events";
    public const string DeadExchange = "nexus.dead";

    public static void DeclareEventsExchange(IModel channel) =>
        channel.ExchangeDeclare(EventsExchange, ExchangeType.Topic, durable: true, autoDelete: false);

    public static void DeclareDeadExchange(IModel channel) =>
        channel.ExchangeDeclare(DeadExchange, ExchangeType.Topic, durable: true, autoDelete: false);

    /// <summary>Phase (a): no dead-letter-exchange argument yet — DLQ wiring is phase (b) (ADR-008).</summary>
    public static void DeclareQuorumQueue(IModel channel, string queueName, string bindingRoutingKey)
    {
        DeclareEventsExchange(channel);

        var arguments = new Dictionary<string, object> { ["x-queue-type"] = "quorum" };
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        channel.QueueBind(queueName, EventsExchange, bindingRoutingKey);
    }
}
