using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Adapted C.9.5.2 query 3 (see WorkOrderWithOriginDto/GetWorkOrdersWithOriginQuery for why): work orders with either origin passport set, no join.</summary>
internal sealed class EfWorkOrdersWithOriginFinder(MaintenanceDbContext dbContext) : IWorkOrdersWithOriginFinder
{
    public async Task<IReadOnlyList<WorkOrderWithOriginDto>> GetWorkOrdersWithOriginAsync(CancellationToken cancellationToken)
    {
        var query =
            from wo in dbContext.WorkOrders
            where wo.OriginOperationalEventId != null || wo.OriginIncidentActionId != null
            select new WorkOrderWithOriginDto(wo.WorkOrderCode, wo.Title, wo.OriginOperationalEventId, wo.OriginIncidentActionId);

        return await query.ToListAsync(cancellationToken);
    }
}
