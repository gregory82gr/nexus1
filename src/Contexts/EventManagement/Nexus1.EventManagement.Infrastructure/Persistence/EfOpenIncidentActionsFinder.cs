using Microsoft.EntityFrameworkCore;
using Nexus1.EventManagement.Application;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.8.5.2 query 3, verbatim: incident actions where status NOT IN (COMPLETED, VERIFIED, CANCELLED), with incident number, ordered by DueAtUtc.</summary>
internal sealed class EfOpenIncidentActionsFinder(EventManagementDbContext dbContext) : IOpenIncidentActionsFinder
{
    private static readonly string[] ClosedStatusCodes = ["COMPLETED", "VERIFIED", "CANCELLED"];

    public async Task<IReadOnlyList<OpenIncidentActionDto>> GetOpenIncidentActionsAsync(CancellationToken cancellationToken)
    {
        var query =
            from a in dbContext.IncidentActions
            join i in dbContext.Incidents on a.IncidentId equals i.Id
            join s in dbContext.IncidentActionStatuses on a.IncidentActionStatusId equals s.Id
            where !ClosedStatusCodes.Contains(s.Code)
            orderby a.DueAtUtc
            select new OpenIncidentActionDto(i.IncidentNumber, a.Title, s.Code, a.DueAtUtc);

        return await query.ToListAsync(cancellationToken);
    }
}
