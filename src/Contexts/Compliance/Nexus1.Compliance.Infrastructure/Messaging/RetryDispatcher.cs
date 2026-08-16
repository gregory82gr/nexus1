using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Compliance.Infrastructure.Persistence;

namespace Nexus1.Compliance.Infrastructure.Messaging;

/// <summary>Mirrors Nexus1.Audit.Infrastructure.Messaging.RetryDispatcher exactly (ADR-009/ADR-011).</summary>
public sealed class RetryDispatcher(
    ComplianceDbContext dbContext,
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
            using var activity = NexusActivitySources.ComplianceSource.StartActivity(
                SpanNames.RetryDispatch, ActivityKind.Internal, parentContext: default,
                tags: SafeTags.ForOwnerOperation(ticket.MessageId, "ATTEMPTED"));

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
                activity?.SetTag("nexus1.outcome.code", "COMMITTED");
            }
            catch (Exception ex)
            {
                SafeError.Record(activity, ex);
                logger.LogWarning(ex, "Failed to dispatch retry ticket {RetryTicketId}; left unpublished for redelivery.", ticket.RetryTicketId);
            }
        }

        return dispatchedCount;
    }
}
