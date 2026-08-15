using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Contracts.ReactorFleet;

namespace Nexus1.AlarmManagement.ComponentTests;

/// <summary>
/// Exercises the real seam where AlarmManagement consumes ReactorFleet's
/// public contract (ADR-001-amend correction, ADR-004) — the command takes
/// UnitPowerSnapshotRecordedV1 directly, never a ReactorFleet.Domain type.
/// </summary>
public sealed class EvaluateReadingCommandHandlerTests : AlarmManagementComponentTestDatabase
{
    private async Task SeedDefinitionAsync(AlarmDefinition definition)
    {
        await using var seedContext = CreateDbContext();
        await seedContext.AlarmDefinitions.AddAsync(definition);
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task A_reading_above_threshold_raises_an_alarm_event_and_persists_it()
    {
        await SeedDefinitionAsync(AlarmDefinition.Create(
            new AlarmDefinitionId(1), new UnitId(1), "HIGH-POWER", "High Power", AlarmSeverity.Critical, 100m));

        var reading = new UnitPowerSnapshotRecordedV1(1, 150m, new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc));

        await using var dbContext = CreateDbContext();
        var handler = new EvaluateReadingCommandHandler(
            DefinitionFinder(dbContext),
            Repository<AlarmEvent, AlarmEventId>(dbContext),
            UnitOfWork(dbContext),
            new SequentialIdGenerator());

        var result = await handler.Handle(new EvaluateReadingCommand(reading), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);

        await using var verifyContext = CreateDbContext();
        var alarmEvent = await verifyContext.AlarmEvents.SingleAsync();
        Assert.Equal(new UnitId(1), alarmEvent.UnitId);
        Assert.Equal(new AlarmDefinitionId(1), alarmEvent.AlarmDefinitionId);
        Assert.Equal(150m, alarmEvent.SourceValue);
        Assert.Equal(AlarmState.Active, alarmEvent.State);
    }

    [Fact]
    public async Task A_reading_below_threshold_raises_nothing()
    {
        await SeedDefinitionAsync(AlarmDefinition.Create(
            new AlarmDefinitionId(1), new UnitId(1), "HIGH-POWER", "High Power", AlarmSeverity.Critical, 100m));

        var reading = new UnitPowerSnapshotRecordedV1(1, 50m, new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc));

        await using var dbContext = CreateDbContext();
        var handler = new EvaluateReadingCommandHandler(
            DefinitionFinder(dbContext),
            Repository<AlarmEvent, AlarmEventId>(dbContext),
            UnitOfWork(dbContext),
            new SequentialIdGenerator());

        var result = await handler.Handle(new EvaluateReadingCommand(reading), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.AlarmEvents.CountAsync());
    }

    [Fact]
    public async Task A_reading_for_a_unit_with_no_definitions_raises_nothing()
    {
        var reading = new UnitPowerSnapshotRecordedV1(999, 150m, new DateTime(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc));

        await using var dbContext = CreateDbContext();
        var handler = new EvaluateReadingCommandHandler(
            DefinitionFinder(dbContext),
            Repository<AlarmEvent, AlarmEventId>(dbContext),
            UnitOfWork(dbContext),
            new SequentialIdGenerator());

        var result = await handler.Handle(new EvaluateReadingCommand(reading), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
    }
}
