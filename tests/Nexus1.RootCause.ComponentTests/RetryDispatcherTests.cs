using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>Mirrors OutboxRelayTests exactly — same resilience contract, real LocalDB, no mocks for persistence.</summary>
public sealed class RetryDispatcherTests : RootCauseComponentTestDatabase
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
            Guid.NewGuid(), AlarmFloodMessageHandler.ConsumerName, messageId, attempt: 1,
            RetryPolicies.AlarmRead.PolicyId, "SimulatedTransientFailure", NowUtc, dueAtUtc: NowUtc,
            producer: "alarm-management", eventType: "nexus1.alarm-management.alarm-flood-detected.v1", schemaVersion: 1,
            originalRoutingKey: "alarm-management.alarm-flood-detected.v1",
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
        Assert.Equal("alarm-management.alarm-flood-detected.v1", publishedMessages[0].RoutingKey);

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

    [Fact]
    public async Task A_ticket_not_yet_due_is_not_dispatched()
    {
        await using (var dbContext = CreateDbContext())
        {
            var notYetDueTicket = new RetryTicket(
                Guid.NewGuid(), AlarmFloodMessageHandler.ConsumerName, Guid.NewGuid(), attempt: 1,
                RetryPolicies.AlarmRead.PolicyId, "SimulatedTransientFailure", NowUtc, dueAtUtc: NowUtc.AddMinutes(5),
                producer: "alarm-management", eventType: "nexus1.alarm-management.alarm-flood-detected.v1", schemaVersion: 1,
                originalRoutingKey: "alarm-management.alarm-flood-detected.v1",
                envelopeBytes: [1, 2, 3], envelopeSha256: new byte[32], createdAtUtc: NowUtc);
            dbContext.RetryTickets.Add(notYetDueTicket);
            await dbContext.SaveChangesAsync();
        }

        var publishedMessages = new List<OutboundMessage>();
        await using var dispatchContext = CreateDbContext();
        var dispatcher = new RetryDispatcher(
            dispatchContext, new RecordingBrokerPublisher(publishedMessages), new FixedDateTimeProvider(NowUtc), NullLogger<RetryDispatcher>.Instance);
        var dispatchedCount = await dispatcher.DispatchOnceAsync(batchSize: 64, CancellationToken.None);

        Assert.Equal(0, dispatchedCount);
        Assert.Empty(publishedMessages);
    }
}
