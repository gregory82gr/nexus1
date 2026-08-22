using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.9.5.2 query 2 exactly: open (IsDeleted = 0, status not CLOSED/CANCELLED) work orders with unit, status and priority, ordered by DueAtUtc.</summary>
internal sealed class EfOpenWorkOrdersFinder(MaintenanceDbContext dbContext) : IOpenWorkOrdersFinder
{
    public async Task<IReadOnlyList<OpenWorkOrderDto>> GetOpenWorkOrdersAsync(CancellationToken cancellationToken)
    {
        var query =
            from wo in dbContext.WorkOrders
            where !wo.IsDeleted
            join u in dbContext.Set<ReactorFleetUnitReference>() on wo.UnitId equals u.UnitId
            join s in dbContext.WorkOrderStatuses on wo.WorkOrderStatusId equals s.Id
            join p in dbContext.WorkPriorities on wo.WorkPriorityId equals p.Id
            where s.Code != "CLOSED" && s.Code != "CANCELLED"
            orderby wo.DueAtUtc
            select new OpenWorkOrderDto(u.Code, wo.WorkOrderCode, wo.Title, s.Code, p.Code, wo.DueAtUtc);

        return await query.ToListAsync(cancellationToken);
    }
}
