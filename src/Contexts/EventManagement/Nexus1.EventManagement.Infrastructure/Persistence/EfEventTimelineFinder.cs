using Microsoft.EntityFrameworkCore;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.8.5.2 query 2, verbatim: timeline entries for an event, ordered by EntryAtUtc, with entry-type code.</summary>
internal sealed class EfEventTimelineFinder(EventManagementDbContext dbContext) : IEventTimelineFinder
{
    public async Task<IReadOnlyList<EventTimelineEntryDto>> GetTimelineAsync(long operationalEventId, CancellationToken cancellationToken)
    {
        var id = new OperationalEventId(operationalEventId);
        var query =
            from t in dbContext.EventTimelineEntries
            where t.OperationalEventId == id
            join tet in dbContext.EventTimelineEntryTypes on t.EventTimelineEntryTypeId equals tet.Id
            orderby t.EntryAtUtc
            select new EventTimelineEntryDto(t.EntryAtUtc, tet.Code, t.Title, t.Body);

        return await query.ToListAsync(cancellationToken);
    }
}
