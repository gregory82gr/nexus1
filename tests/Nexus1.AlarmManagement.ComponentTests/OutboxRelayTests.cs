using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Messaging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;

namespace Nexus1.AlarmManagement.ComponentTests;

/// <summary>Proves the outbox is genuinely transactional — real LocalDB, no mocks for persistence.</summary>
public sealed class OutboxRelayTests : AlarmManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class AlwaysThrowsBrokerPublisher : IBrokerPublisher
    {
        public int AttemptCount { get; private set; }

        public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
        {
            AttemptCount++;
            throw new InvalidOperationException("Simulated broker outage.");
        }
    }

    private async Task DetectAFloodAsync()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DetectFloodCommandHandler(
            EventFinder(dbContext),
            Repository<AlarmFlood, AlarmFloodId>(dbContext),
            UnitOfWork(dbContext),
            new FixedDateTimeProvider(NowUtc),
            new SequentialIdGenerator(),
            new EfOutboxWriter(dbContext));

        // Seed enough recent alarms to trip the flood detector.
        await using (var seedContext = CreateDbContext())
        {
            for (var i = 0; i < 3; i++)
            {
                await seedContext.AlarmEvents.AddAsync(AlarmEvent.Raise(
                    new AlarmEventId(i + 1), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.High,
                    NowUtc.AddSeconds(-i * 5), 120m, 100m, "HIGH-POWER breached."));
            }

            await seedContext.SaveChangesAsync();
        }

        var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 3, WindowSeconds: 30), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    [Fact]
    public async Task Detecting_a_flood_writes_the_AlarmFlood_and_the_outbox_row_in_the_same_transaction()
    {
        await DetectAFloodAsync();

        await using var verifyContext = CreateDbContext();
        Assert.Equal(1, await verifyContext.AlarmFloods.CountAsync());

        var outboxMessage = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal("nexus1.alarm-management.alarm-flood-detected.v1", outboxMessage.EventType);
        Assert.Equal("alarm-management.alarm-flood-detected.v1", outboxMessage.RoutingKey);
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task A_publish_failure_leaves_the_outbox_row_unprocessed_for_redelivery()
    {
        await DetectAFloodAsync();

        var failingPublisher = new AlwaysThrowsBrokerPublisher();

        await using (var relayContext = CreateDbContext())
        {
            var relay = new OutboxRelay(relayContext, failingPublisher, new FixedDateTimeProvider(NowUtc), NullLogger<OutboxRelay>.Instance);
            var publishedCount = await relay.RelayOnceAsync(batchSize: 64, CancellationToken.None);

            Assert.Equal(0, publishedCount);
            Assert.Equal(1, failingPublisher.AttemptCount);
        }

        // Read back with an independent DbContext — the row must still be
        // there, unprocessed, not lost and not silently deleted.
        await using var verifyContext = CreateDbContext();
        var outboxMessage = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task A_successful_publish_marks_the_outbox_row_processed()
    {
        await DetectAFloodAsync();

        var publishedMessages = new List<OutboundMessage>();
        var succeedingPublisher = new RecordingBrokerPublisher(publishedMessages);

        await using (var relayContext = CreateDbContext())
        {
            var relay = new OutboxRelay(relayContext, succeedingPublisher, new FixedDateTimeProvider(NowUtc), NullLogger<OutboxRelay>.Instance);
            var publishedCount = await relay.RelayOnceAsync(batchSize: 64, CancellationToken.None);

            Assert.Equal(1, publishedCount);
        }

        Assert.Single(publishedMessages);

        await using var verifyContext = CreateDbContext();
        var outboxMessage = await verifyContext.OutboxMessages.SingleAsync();
        Assert.NotNull(outboxMessage.ProcessedAtUtc);
    }

    private sealed class RecordingBrokerPublisher(List<OutboundMessage> sink) : IBrokerPublisher
    {
        public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken)
        {
            sink.Add(message);
            return Task.CompletedTask;
        }
    }
}
