using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.ComponentTests;

public sealed class GetActiveAlarmsForUnitQueryHandlerTests : AlarmManagementComponentTestDatabase
{
    private static readonly DateTime RaisedAtUtc = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_active_alarms_for_the_requested_unit()
    {
        await using (var seedContext = CreateDbContext())
        {
            var activeForUnit1 = AlarmEvent.Raise(
                new AlarmEventId(1), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.High,
                RaisedAtUtc, 120m, 100m, "Active alarm for unit 1.");

            var acknowledgedForUnit1 = AlarmEvent.Raise(
                new AlarmEventId(2), new AlarmDefinitionId(1), new UnitId(1), AlarmSeverity.High,
                RaisedAtUtc, 130m, 100m, "Acknowledged alarm for unit 1.");
            acknowledgedForUnit1.Acknowledge(new UserId(Guid.NewGuid()), RaisedAtUtc.AddMinutes(1));

            var activeForUnit2 = AlarmEvent.Raise(
                new AlarmEventId(3), new AlarmDefinitionId(2), new UnitId(2), AlarmSeverity.High,
                RaisedAtUtc, 120m, 100m, "Active alarm for unit 2.");

            await seedContext.AlarmEvents.AddRangeAsync(activeForUnit1, acknowledgedForUnit1, activeForUnit2);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetActiveAlarmsForUnitQueryHandler(EventFinder(dbContext));

        var result = await handler.Handle(new GetActiveAlarmsForUnitQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var alarm = Assert.Single(result.Value);
        Assert.Equal(1, alarm.AlarmEventId);
        Assert.Equal("Active alarm for unit 1.", alarm.Message);
    }
}
