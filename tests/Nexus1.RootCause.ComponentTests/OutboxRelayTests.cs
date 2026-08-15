using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Messaging;
using Nexus1.RootCause.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// Proves RootCause's producer-side outbox is genuinely transactional —
/// real LocalDB, no mocks. Mirrors AlarmManagement's OutboxRelayTests
/// exactly (ADR-010): this same proof was missing for RootCause's own
/// publish side before this step (CloseAnalysisCommandHandler never wired
/// an outbox write until now).
/// </summary>
public sealed class OutboxRelayTests : RootCauseComponentTestDatabase
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

    private async Task<long> CloseAGoldenAnalysisAsync()
    {
        await using var dbContext = CreateDbContext();
        var repository = Repository(dbContext);
        var unitOfWork = UnitOfWork(dbContext);
        var idGenerator = new SequentialIdGenerator();
        var dateTimeProvider = new FixedDateTimeProvider(NowUtc);

        var openHandler = new OpenAnalysisCommandHandler(repository, unitOfWork, dateTimeProvider, idGenerator);
        var openResult = await openHandler.Handle(new OpenAnalysisCommand(1, 500, "system:test"), CancellationToken.None);
        Assert.True(openResult.IsSuccess);
        var analysisId = openResult.Value;

        var hypothesisHandler = new AddHypothesisCommandHandler(repository, unitOfWork, idGenerator);
        var hypothesisResult = await hypothesisHandler.Handle(
            new AddHypothesisCommand(analysisId, "Common power supply instability"), CancellationToken.None);
        Assert.True(hypothesisResult.IsSuccess);

        var evidenceHandler = new AddEvidenceCommandHandler(repository, unitOfWork, dateTimeProvider, idGenerator);
        var evidenceResult = await evidenceHandler.Handle(
            new AddEvidenceCommand(analysisId, hypothesisResult.Value, "Golden evidence."), CancellationToken.None);
        Assert.True(evidenceResult.IsSuccess);

        var closeHandler = new CloseAnalysisCommandHandler(repository, unitOfWork, dateTimeProvider, new EfOutboxWriter(dbContext));
        var closeResult = await closeHandler.Handle(
            new CloseAnalysisCommand(analysisId, "COMMON_POWER_SUPPLY_INSTABILITY", "system:test"), CancellationToken.None);
        Assert.True(closeResult.IsSuccess);

        return analysisId;
    }

    [Fact]
    public async Task Closing_an_analysis_writes_the_verdict_and_the_outbox_row_in_the_same_transaction()
    {
        await CloseAGoldenAnalysisAsync();

        await using var verifyContext = CreateDbContext();
        var analysis = await verifyContext.RootCauseAnalyses.SingleAsync();
        Assert.Equal(AnalysisStatus.Closed, analysis.Status);

        var outboxMessage = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Equal("nexus1.root-cause.root-cause-verdict-issued.v1", outboxMessage.EventType);
        Assert.Equal("root-cause.root-cause-verdict-issued.v1", outboxMessage.RoutingKey);
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task A_publish_failure_leaves_the_outbox_row_unprocessed_for_redelivery()
    {
        await CloseAGoldenAnalysisAsync();

        var failingPublisher = new AlwaysThrowsBrokerPublisher();

        await using (var relayContext = CreateDbContext())
        {
            var relay = new OutboxRelay(relayContext, failingPublisher, new FixedDateTimeProvider(NowUtc), NullLogger<OutboxRelay>.Instance);
            var publishedCount = await relay.RelayOnceAsync(batchSize: 64, CancellationToken.None);

            Assert.Equal(0, publishedCount);
            Assert.Equal(1, failingPublisher.AttemptCount);
        }

        await using var verifyContext = CreateDbContext();
        var outboxMessage = await verifyContext.OutboxMessages.SingleAsync();
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task A_successful_publish_marks_the_outbox_row_processed()
    {
        await CloseAGoldenAnalysisAsync();

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
}
