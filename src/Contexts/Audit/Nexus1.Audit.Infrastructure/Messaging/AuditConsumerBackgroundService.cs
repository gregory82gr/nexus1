using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using RabbitMQ.Client.Events;

namespace Nexus1.Audit.Infrastructure.Messaging;

/// <summary>
/// Thin hosting loop — mirrors AlarmFloodConsumerBackgroundService exactly,
/// including the CONSUMER span/carrier extraction (ADR-010/ADR-013). Queue
/// name and binding routing key are ch.34's frozen shape (Executable Asset
/// 34-U), not ch.25's broader illustrative topology.
/// </summary>
public sealed class AuditConsumerBackgroundService(
    RabbitMqConnectionManager connectionManager,
    RabbitMqOptions rabbitMqOptions,
    AuditVerdictMessageHandler messageHandler,
    ILogger<AuditConsumerBackgroundService> logger) : BackgroundService
{
    private const string QueueName = "audit.root-cause-verdicts.v1";
    private const string BindingRoutingKey = "root-cause.root-cause-verdict-issued.v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connectionManager.CreateChannel();
        NexusTopology.DeclareQuorumQueue(channel, QueueName, BindingRoutingKey);
        NexusTopology.DeclareDeadQueue(channel, QueueName);

        await new RabbitMqDeadLetterPolicyProvisioner(rabbitMqOptions)
            .EnsureAsync("nexus-live-queue-safety-" + QueueName, QueueName, stoppingToken);

        channel.BasicQos(0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, delivery) =>
        {
            try
            {
                if (!Guid.TryParse(delivery.BasicProperties.MessageId, out var messageId))
                {
                    throw new InvalidOperationException($"Message has no valid MessageId property: '{delivery.BasicProperties.MessageId}'.");
                }

                var eventType = delivery.BasicProperties.Type ?? "unknown";
                var parentContext = AmqpCarrier.Extract(delivery.BasicProperties.Headers);
                using var activity = NexusActivitySources.MessagingSource.StartActivity(
                    SpanNames.ForProcess(eventType),
                    ActivityKind.Consumer,
                    parentContext,
                    tags: SafeTags.ForMessageProcess(messageId, eventType));

                MessageHandlingOutcome outcome;
                try
                {
                    outcome = await messageHandler.HandleAsync(messageId, delivery.Body.ToArray(), stoppingToken);
                }
                catch (Exception ex)
                {
                    SafeError.Record(activity, ex);
                    throw;
                }
                switch (outcome)
                {
                    case MessageHandlingOutcome.Ack:
                        channel.BasicAck(delivery.DeliveryTag, multiple: false);
                        break;
                    case MessageHandlingOutcome.NackNoRequeue:
                        logger.LogError(
                            "Message {MessageId} quarantined after exhausting retries; routing to dead-letter.",
                            delivery.BasicProperties.MessageId);
                        channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
                        break;
                    case MessageHandlingOutcome.NackRequeue:
                    default:
                        logger.LogWarning("Ambiguous inbox outcome for message {MessageId}; nacking for redelivery.", delivery.BasicProperties.MessageId);
                        channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process message {MessageId}; nacking for redelivery.", delivery.BasicProperties.MessageId);
                channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(
            queue: QueueName, autoAck: false, consumerTag: string.Empty, noLocal: false,
            exclusive: false, arguments: null, consumer: consumer);

        var stopped = new TaskCompletionSource();
        await using (stoppingToken.Register(() => stopped.TrySetResult()))
        {
            await stopped.Task;
        }
    }
}
