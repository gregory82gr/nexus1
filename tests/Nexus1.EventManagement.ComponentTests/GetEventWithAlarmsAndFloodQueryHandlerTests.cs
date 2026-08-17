using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

/// <summary>Matches the atlas's own C.8.5.2 query 1: an event by EventCode, with status/severity codes, plus every linked AlarmEventId/AlarmFloodId.</summary>
public sealed class GetEventWithAlarmsAndFloodQueryHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_event_with_its_status_severity_and_linked_alarm_and_flood_ids()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using (var eventSeedContext = CreateDbContext())
        {
            var operationalEvent = OperationalEvent.Create(
                new OperationalEventId(1), seed.UnitId, new EventTypeId(seed.EventTypeId), new EventStatusId(seed.EventStatusId),
                new EventSeverityId(seed.EventSeverityId), new EventSourceTypeId(seed.EventSourceTypeId),
                EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5));
            await eventSeedContext.OperationalEvents.AddAsync(operationalEvent);
            await eventSeedContext.SaveChangesAsync();

            var alarmLink = EventAlarmLink.Create(new EventAlarmLinkId(1), operationalEvent.Id.Value, seed.AlarmEventId);
            var floodLink = EventFloodLink.Create(new EventFloodLinkId(1), operationalEvent.Id.Value, seed.AlarmFloodId);
            await eventSeedContext.EventAlarmLinks.AddAsync(alarmLink);
            await eventSeedContext.EventFloodLinks.AddAsync(floodLink);
            await eventSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetEventWithAlarmsAndFloodQueryHandler(new EfEventWithAlarmsAndFloodFinder(dbContext));

        var result = await handler.Handle(new GetEventWithAlarmsAndFloodQuery(EventManagementSeedHelper.EventCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Feedwater flow deviation", result.Value!.Title);
        Assert.Equal("NEW", result.Value.EventStatus);
        Assert.Equal("MAJOR", result.Value.EventSeverity);
        Assert.Equal(seed.AlarmEventId, Assert.Single(result.Value.AlarmEventIds));
        Assert.Equal(seed.AlarmFloodId, Assert.Single(result.Value.AlarmFloodIds));
    }

    [Fact]
    public async Task Returns_null_when_no_event_matches_the_code()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetEventWithAlarmsAndFloodQueryHandler(new EfEventWithAlarmsAndFloodFinder(dbContext));

        var result = await handler.Handle(new GetEventWithAlarmsAndFloodQuery("EVT-DOES-NOT-EXIST"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }
}
