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

    /// <summary>Matches Executable Asset 25-B exactly: only x-queue-type at declare time — dead-lettering is policy, not an argument (ADR-009).</summary>
    public static void DeclareQuorumQueue(IModel channel, string queueName, string bindingRoutingKey)
    {
        DeclareEventsExchange(channel);

        var arguments = new Dictionary<string, object> { ["x-queue-type"] = "quorum" };
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        channel.QueueBind(queueName, EventsExchange, bindingRoutingKey);
    }

    /// <summary>
    /// Matches Executable Asset 25-B's per-subscription dead queue: named
    /// "&lt;liveQueueName&gt;.dead", bound to nexus.dead with that same name as
    /// routing key — the policy-driven x-dead-letter-routing-key on the live
    /// queue must match this exactly for dead-lettered messages to land here
    /// rather than being unroutable (ADR-009).
    /// </summary>
    public static void DeclareDeadQueue(IModel channel, string liveQueueName)
    {
        DeclareDeadExchange(channel);

        var deadQueueName = $"{liveQueueName}.dead";
        var arguments = new Dictionary<string, object> { ["x-queue-type"] = "quorum" };
        channel.QueueDeclare(deadQueueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
        channel.QueueBind(deadQueueName, DeadExchange, deadQueueName);
    }
}
