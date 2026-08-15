using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.ComponentTests;

public sealed class DetectFloodCommandHandlerTests : AlarmManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
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
    public async Task Enough_recent_alarms_within_the_window_detects_a_flood_and_persists_it()
    {
        await SeedAlarmEventsAsync(NowUtc.AddSeconds(-20), NowUtc.AddSeconds(-10), NowUtc.AddSeconds(-1));

        await using var dbContext = CreateDbContext();
        var handler = new DetectFloodCommandHandler(
            EventFinder(dbContext),
            Repository<AlarmFlood, AlarmFloodId>(dbContext),
            UnitOfWork(dbContext),
            new FixedDateTimeProvider(NowUtc),
            new SequentialIdGenerator());

        var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 3, WindowSeconds: 30), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        await using var verifyContext = CreateDbContext();
        var flood = await verifyContext.AlarmFloods.SingleAsync(f => f.Id == new AlarmFloodId(result.Value!.Value));
        Assert.Equal(new UnitId(1), flood.UnitId);
        Assert.Equal(AlarmFloodStatus.Detected, flood.Status);
    }

    [Fact]
    public async Task Too_few_recent_alarms_does_not_detect_a_flood()
    {
        await SeedAlarmEventsAsync(NowUtc.AddSeconds(-10));

        await using var dbContext = CreateDbContext();
        var handler = new DetectFloodCommandHandler(
            EventFinder(dbContext),
            Repository<AlarmFlood, AlarmFloodId>(dbContext),
            UnitOfWork(dbContext),
            new FixedDateTimeProvider(NowUtc),
            new SequentialIdGenerator());

        var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 3, WindowSeconds: 30), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.AlarmFloods.CountAsync());
    }

    [Fact]
    public async Task Zero_count_threshold_fails()
    {
        await using var dbContext = CreateDbContext();
        var handler = new DetectFloodCommandHandler(
            EventFinder(dbContext),
            Repository<AlarmFlood, AlarmFloodId>(dbContext),
            UnitOfWork(dbContext),
            new FixedDateTimeProvider(NowUtc),
            new SequentialIdGenerator());

        var result = await handler.Handle(new DetectFloodCommand(1, CountThreshold: 0, WindowSeconds: 30), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
