using Microsoft.EntityFrameworkCore;
using Nexus1.EventManagement.Application;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.8.5.2 query 1: an event by EventCode, with status/severity codes, plus every linked AlarmEventId/AlarmFloodId.</summary>
internal sealed class EfEventWithAlarmsAndFloodFinder(EventManagementDbContext dbContext) : IEventWithAlarmsAndFloodFinder
{
    public async Task<EventWithAlarmsAndFloodDto?> GetByEventCodeAsync(string eventCode, CancellationToken cancellationToken)
    {
        var header =
            await (from e in dbContext.OperationalEvents
                   where e.EventCode == eventCode
                   join es in dbContext.EventStatuses on e.EventStatusId equals es.Id
                   join sev in dbContext.EventSeverities on e.EventSeverityId equals sev.Id
                   select new { e.Id, e.EventCode, e.Title, EventStatus = es.Code, EventSeverity = sev.Code })
                .SingleOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return null;
        }

        var alarmEventIds = await dbContext.EventAlarmLinks
            .Where(l => l.OperationalEventId == header.Id)
            .Select(l => l.AlarmEventId)
            .ToListAsync(cancellationToken);

        var alarmFloodIds = await dbContext.EventFloodLinks
            .Where(l => l.OperationalEventId == header.Id)
            .Select(l => l.AlarmFloodId)
            .ToListAsync(cancellationToken);

        return new EventWithAlarmsAndFloodDto(header.EventCode, header.Title, header.EventStatus, header.EventSeverity, alarmEventIds, alarmFloodIds);
    }
}
