using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.ComponentTests;

public sealed class AcknowledgeAlarmCommandHandlerTests : AlarmManagementComponentTestDatabase
{
    private static readonly DateTime RaisedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private async Task SeedAlarmEventAsync()
    {
        await using var seedContext = CreateDbContext();
        var alarmEvent = AlarmEvent.Raise(
            new AlarmEventId(1), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.Critical,
            RaisedAtUtc, 150m, 100m, "HIGH-POWER breached: 150 >= 100.");
        await seedContext.AlarmEvents.AddAsync(alarmEvent);
        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Acknowledging_an_active_alarm_transitions_it_and_persists_the_change()
    {
        await SeedAlarmEventAsync();
        var userId = Guid.NewGuid();

        await using var dbContext = CreateDbContext();
        var handler = new AcknowledgeAlarmCommandHandler(
            Repository<AlarmEvent, AlarmEventId>(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider());

        var result = await handler.Handle(new AcknowledgeAlarmCommand(1, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var alarmEvent = await verifyContext.AlarmEvents.SingleAsync();
        Assert.Equal(AlarmState.Acknowledged, alarmEvent.State);
        Assert.Equal(new UserId(userId), alarmEvent.AcknowledgedBy);
    }

    [Fact]
    public async Task Acknowledging_an_already_acknowledged_alarm_fails()
    {
        await SeedAlarmEventAsync();
        await using (var firstAckContext = CreateDbContext())
        {
            await new AcknowledgeAlarmCommandHandler(
                Repository<AlarmEvent, AlarmEventId>(firstAckContext), UnitOfWork(firstAckContext), new SystemDateTimeProvider())
                .Handle(new AcknowledgeAlarmCommand(1, Guid.NewGuid()), CancellationToken.None);
        }

        await using var dbContext = CreateDbContext();
        var handler = new AcknowledgeAlarmCommandHandler(
            Repository<AlarmEvent, AlarmEventId>(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider());

        var result = await handler.Handle(new AcknowledgeAlarmCommand(1, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Acknowledging_an_unknown_alarm_fails()
    {
        await using var dbContext = CreateDbContext();
        var handler = new AcknowledgeAlarmCommandHandler(
            Repository<AlarmEvent, AlarmEventId>(dbContext), UnitOfWork(dbContext), new SystemDateTimeProvider());

        var result = await handler.Handle(new AcknowledgeAlarmCommand(999, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("999", result.Error);
    }
}
