using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;
using Nexus1.EventManagement.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

/// <summary>Matches the atlas's own C.8.5.2 query 2, verbatim: timeline entries for an event, ordered by EntryAtUtc, with entry-type code.</summary>
public sealed class GetEventTimelineQueryHandlerTests : EventManagementComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_timeline_entries_ordered_by_entry_time()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await EventManagementSeedHelper.SeedCoreAsync(reactorFleetContext, alarmManagementContext, seedContext, NowUtc);

        long operationalEventId;
        await using (var eventSeedContext = CreateDbContext())
        {
            var operationalEvent = OperationalEvent.Create(
                new OperationalEventId(1), seed.UnitId, new EventTypeId(seed.EventTypeId), new EventStatusId(seed.EventStatusId),
                new EventSeverityId(seed.EventSeverityId), new EventSourceTypeId(seed.EventSourceTypeId),
                EventManagementSeedHelper.EventCode, "Feedwater flow deviation", NowUtc, NowUtc.AddMinutes(5));
            await eventSeedContext.OperationalEvents.AddAsync(operationalEvent);
            await eventSeedContext.SaveChangesAsync();
            operationalEventId = operationalEvent.Id.Value;

            var laterEntry = EventTimelineEntry.Create(
                new EventTimelineEntryId(1), operationalEventId, new EventTimelineEntryTypeId(seed.EventTimelineEntryTypeId),
                NowUtc.AddMinutes(10), "Acknowledged by operator");
            var earlierEntry = EventTimelineEntry.Create(
                new EventTimelineEntryId(2), operationalEventId, new EventTimelineEntryTypeId(seed.EventTimelineEntryTypeId),
                NowUtc, "Detected by alarm flood");
            await eventSeedContext.EventTimelineEntries.AddAsync(laterEntry);
            await eventSeedContext.EventTimelineEntries.AddAsync(earlierEntry);
            await eventSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetEventTimelineQueryHandler(new EfEventTimelineFinder(dbContext));

        var result = await handler.Handle(new GetEventTimelineQuery(operationalEventId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Detected by alarm flood", result.Value[0].Title);
        Assert.Equal("Acknowledged by operator", result.Value[1].Title);
        Assert.Equal("DETECTED", result.Value[0].EntryType);
    }
}
