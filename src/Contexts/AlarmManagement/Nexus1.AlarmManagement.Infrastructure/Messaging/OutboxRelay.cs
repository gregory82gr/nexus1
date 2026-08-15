using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.AlarmManagement.Infrastructure.Messaging;

/// <summary>
/// One relay pass, testable in isolation from the hosting loop. A publish
/// failure leaves the row unprocessed (ProcessedAtUtc still null) for
/// redelivery on the next pass — phase (a)'s reduced stand-in for the
/// book's fuller Retryable/Quarantined state machine (ADR-008).
/// </summary>
public sealed class OutboxRelay(
    AlarmManagementDbContext dbContext,
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
                // Deliberately not rethrown: this row stays unprocessed for
                // redelivery on the next pass. Continue with the rest of the
                // batch rather than let one failure block it.
                logger.LogWarning(ex, "Failed to publish outbox message {MessageId}; left unprocessed for redelivery.", message.MessageId);
            }
        }

        return publishedCount;
    }
}
