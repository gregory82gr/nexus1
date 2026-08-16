using System.Diagnostics;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Messaging;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.AlarmManagement.ComponentTests;

/// <summary>
/// Fast, deterministic proof of the local span graph via in-process
/// ActivityListener capture (ch.51 Verification Assets 51-A/51-E) — mirrors
/// Nexus1.RootCause.ComponentTests.TracingTests exactly (ADR-013 step 5).
/// </summary>
public sealed class TracingTests : AlarmManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed record CapturedSpan(string SourceName, string Name, ActivityKind Kind, IReadOnlyDictionary<string, object?> Tags);

    private static List<CapturedSpan> CaptureSpans(Func<Task> scenario)
    {
        var captured = new List<CapturedSpan>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NexusActivitySources.AlarmManagement,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => captured.Add(new CapturedSpan(
                activity.Source.Name, activity.DisplayName, activity.Kind, activity.TagObjects.ToDictionary(t => t.Key, t => t.Value))),
        };
        ActivitySource.AddActivityListener(listener);

        scenario().GetAwaiter().GetResult();
        return captured;
    }

    private async Task SeedAlarmEventsAsync(params DateTime[] raisedAtTimestamps)
    {
        await using var seedContext = CreateDbContext();
        var id = 1L;
        foreach (var raisedAt in raisedAtTimestamps)
        {
            await seedContext.AlarmEvents.AddAsync(AlarmEvent.Raise(
                new AlarmEventId(id++), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.High,
                raisedAt, 120m, 100m, "HIGH-POWER breached."));
        }

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public void Detecting_a_flood_emits_a_committed_owner_span()
    {
        var captured = CaptureSpans(async () =>
        {
            await SeedAlarmEventsAsync(NowUtc.AddSeconds(-20), NowUtc.AddSeconds(-10), NowUtc.AddSeconds(-1));

            await using var dbContext = CreateDbContext();
            var handler = new DetectFloodCommandHandler(
                EventFinder(dbContext), Repository<AlarmFlood, AlarmFloodId>(dbContext), UnitOfWork(dbContext),
                new FixedDateTimeProvider(NowUtc), new SequentialIdGenerator(), new EfOutboxWriter(dbContext));

            var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 3, WindowSeconds: 30), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.AlarmFloodCommit);
        Assert.Equal(NexusActivitySources.AlarmManagement, span.SourceName);
        Assert.Equal(ActivityKind.Internal, span.Kind);
        Assert.Equal("COMMITTED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void Too_few_alarms_abstains_rather_than_going_unrecorded()
    {
        var captured = CaptureSpans(async () =>
        {
            await SeedAlarmEventsAsync(NowUtc.AddSeconds(-10));

            await using var dbContext = CreateDbContext();
            var handler = new DetectFloodCommandHandler(
                EventFinder(dbContext), Repository<AlarmFlood, AlarmFloodId>(dbContext), UnitOfWork(dbContext),
                new FixedDateTimeProvider(NowUtc), new SequentialIdGenerator(), new EfOutboxWriter(dbContext));

            var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 3, WindowSeconds: 30), CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.AlarmFloodCommit);
        Assert.Equal("ABSTAINED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void Defining_an_alarm_emits_a_committed_owner_span()
    {
        var captured = CaptureSpans(async () =>
        {
            await using var dbContext = CreateDbContext();
            var handler = new DefineAlarmCommandHandler(
                Repository<AlarmDefinition, AlarmDefinitionId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

            var result = await handler.Handle(
                new DefineAlarmCommand(1, "HIGH-POWER", "High Power", AlarmSeverity.High, 100m), CancellationToken.None);
            Assert.True(result.IsSuccess);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.AlarmDefine);
        Assert.Equal("COMMITTED", span.Tags["nexus1.outcome.code"]);
    }

    [Fact]
    public void A_rejected_define_still_emits_a_span_with_a_rejected_outcome()
    {
        var captured = CaptureSpans(async () =>
        {
            await using var dbContext = CreateDbContext();
            var handler = new DefineAlarmCommandHandler(
                Repository<AlarmDefinition, AlarmDefinitionId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

            var result = await handler.Handle(
                new DefineAlarmCommand(1, "", "High Power", AlarmSeverity.High, 100m), CancellationToken.None);
            Assert.True(result.IsFailure);
        });

        var span = Assert.Single(captured, s => s.Name == SpanNames.AlarmDefine);
        Assert.Equal("REJECTED", span.Tags["nexus1.outcome.code"]);
    }
}
