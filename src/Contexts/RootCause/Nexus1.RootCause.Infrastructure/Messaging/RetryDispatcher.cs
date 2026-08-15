using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// One dispatch pass, testable in isolation from the hosting loop — mirrors
/// OutboxRelay's shape exactly (ADR-009). Republishes a due ticket's frozen
/// envelope bytes through the normal exchange with its original routing key;
/// the redelivered message re-enters the live queue and is picked up by the
/// same consumer, relying on phase (a)'s inbox dedup rather than a second
/// idempotency mechanism. A publish failure leaves the ticket unpublished
/// (PublishedAtUtc still null) for redelivery on the next pass, same
/// resilience contract as the outbox.
/// </summary>
public sealed class RetryDispatcher(
    RootCauseDbContext dbContext,
    IBrokerPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    ILogger<RetryDispatcher> logger)
{
    public async Task<int> DispatchOnceAsync(int batchSize, CancellationToken cancellationToken)
    {
        var nowUtc = dateTimeProvider.UtcNow;
        var due = await dbContext.RetryTickets
            .Where(t => t.PublishedAtUtc == null && t.DueAtUtc <= nowUtc)
            .OrderBy(t => t.DueAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var dispatchedCount = 0;
        foreach (var ticket in due)
        {
            try
            {
                await publisher.PublishAsync(
                    new OutboundMessage(
                        ticket.MessageId, ticket.EventType, ticket.SchemaVersion, ticket.Producer,
                        ticket.OriginalRoutingKey, ticket.EnvelopeBytes, ticket.EnvelopeSha256, null, null),
                    cancellationToken);

                ticket.MarkPublished(dateTimeProvider.UtcNow);
                await dbContext.SaveChangesAsync(cancellationToken);
                dispatchedCount++;
            }
            catch (Exception ex)
            {
                // Deliberately not rethrown: this ticket stays unpublished for
                // redelivery on the next pass, same contract as OutboxRelay.
                logger.LogWarning(ex, "Failed to dispatch retry ticket {RetryTicketId}; left unpublished for redelivery.", ticket.RetryTicketId);
            }
        }

        return dispatchedCount;
    }
}
