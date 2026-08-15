using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Messaging;
using RabbitMQ.Client.Events;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Thin hosting loop — the actual dedup/domain-reaction logic lives in
/// AlarmFloodMessageHandler, testable without a live RabbitMQ delivery.
/// </summary>
public sealed class AlarmFloodConsumerBackgroundService(
    RabbitMqConnectionManager connectionManager,
    AlarmFloodMessageHandler messageHandler,
    ILogger<AlarmFloodConsumerBackgroundService> logger) : BackgroundService
{
    private const string QueueName = "rootcause.alarm-events.v1";
    private const string BindingRoutingKey = "alarm-management.alarm-flood-detected.v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connectionManager.CreateChannel();
        NexusTopology.DeclareQuorumQueue(channel, QueueName, BindingRoutingKey);
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

                var shouldAck = await messageHandler.HandleAsync(messageId, delivery.Body.ToArray(), stoppingToken);
                if (shouldAck)
                {
                    channel.BasicAck(delivery.DeliveryTag, multiple: false);
                }
                else
                {
                    logger.LogWarning("Ambiguous inbox outcome for message {MessageId}; nacking for redelivery.", delivery.BasicProperties.MessageId);
                    channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
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
