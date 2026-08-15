using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// Mirrors Nexus1.AlarmManagement.Infrastructure.Messaging.OutboxRelay
/// exactly (ADR-008/ADR-010). A publish failure leaves the row unprocessed
/// (ProcessedAtUtc still null) for redelivery on the next pass.
/// </summary>
public sealed class OutboxRelay(
    RootCauseDbContext dbContext,
    IBrokerPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<OutboxRelay> logger)
{
    public async Task<int> RelayOnceAsync(int batchSize, CancellationToken cancellationToken)
    {
        var pending = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .OrderBy(m => m.StoredAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var publishedCount = 0;
        foreach (var message in pending)
        {
            try
            {
                await publisher.PublishAsync(
                    new OutboundMessage(
                        message.MessageId, message.EventType, message.SchemaVersion, message.Producer,
                        message.RoutingKey, message.EnvelopeBytes, message.EnvelopeSha256, null, null),
                    cancellationToken);

                message.MarkProcessed(dateTimeProvider.UtcNow);
                await dbContext.SaveChangesAsync(cancellationToken);
                publishedCount++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to publish outbox message {MessageId}; left unprocessed for redelivery.", message.MessageId);
            }
        }

        return publishedCount;
    }
}
