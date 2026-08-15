using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.Audit.Infrastructure.Messaging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.Audit.ComponentTests;

/// <summary>Mirrors Nexus1.RootCause.ComponentTests.RetryDispatcherTests exactly — real LocalDB, no mocks for persistence.</summary>
public sealed class RetryDispatcherTests : AuditComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class AlwaysThrowsBrokerPublisher : IBrokerPublisher
    {
        public int AttemptCount { get; private set; }

        public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
        {
            AttemptCount++;
            throw new InvalidOperationException("Simulated broker outage.");
        }
    }

    private sealed class RecordingBrokerPublisher(List<OutboundMessage> sink) : IBrokerPublisher
    {
        public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
        {
            sink.Add(message);
            return Task.CompletedTask;
        }
    }

    private async Task SeedDueTicketAsync(Guid messageId)
    {
        await using var dbContext = CreateDbContext();
        var ticket = new RetryTicket(
            Guid.NewGuid(), AuditVerdictMessageHandler.ConsumerName, messageId, attempt: 1,
            RetryPolicies.AuditRootCauseVerdicts.PolicyId, "SimulatedTransientFailure", NowUtc, dueAtUtc: NowUtc,
            producer: "root-cause", eventType: "nexus1.root-cause.root-cause-verdict-issued.v1", schemaVersion: 1,
            originalRoutingKey: "root-cause.root-cause-verdict-issued.v1",
            envelopeBytes: [1, 2, 3], envelopeSha256: new byte[32], createdAtUtc: NowUtc);
        dbContext.RetryTickets.Add(ticket);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task A_due_ticket_is_republished_and_marked_published()
    {
        var messageId = Guid.NewGuid();
        await SeedDueTicketAsync(messageId);

        var publishedMessages = new List<OutboundMessage>();
        var succeedingPublisher = new RecordingBrokerPublisher(publishedMessages);

        await using (var dispatchContext = CreateDbContext())
        {
            var dispatcher = new RetryDispatcher(dispatchContext, succeedingPublisher, new FixedDateTimeProvider(NowUtc), NullLogger<RetryDispatcher>.Instance);
            var dispatchedCount = await dispatcher.DispatchOnceAsync(batchSize: 64, CancellationToken.None);

            Assert.Equal(1, dispatchedCount);
        }

        Assert.Single(publishedMessages);
        Assert.Equal(messageId, publishedMessages[0].MessageId);

        await using var verifyContext = CreateDbContext();
        var ticket = await verifyContext.RetryTickets.SingleAsync();
        Assert.NotNull(ticket.PublishedAtUtc);
    }

    [Fact]
    public async Task A_dispatch_failure_leaves_the_ticket_unpublished_for_redelivery()
    {
        var messageId = Guid.NewGuid();
        await SeedDueTicketAsync(messageId);

        var failingPublisher = new AlwaysThrowsBrokerPublisher();

        await using (var dispatchContext = CreateDbContext())
        {
            var dispatcher = new RetryDispatcher(dispatchContext, failingPublisher, new FixedDateTimeProvider(NowUtc), NullLogger<RetryDispatcher>.Instance);
            var dispatchedCount = await dispatcher.DispatchOnceAsync(batchSize: 64, CancellationToken.None);

            Assert.Equal(0, dispatchedCount);
            Assert.Equal(1, failingPublisher.AttemptCount);
        }

        await using var verifyContext = CreateDbContext();
        var ticket = await verifyContext.RetryTickets.SingleAsync();
        Assert.Null(ticket.PublishedAtUtc);
    }
}
