using Microsoft.EntityFrameworkCore;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>Atlas C.9.5.2 query 3, now buildable as the atlas literally specifies (ADR-022's reconnection) — real LEFT JOINs against EventManagement.OperationalEvent/IncidentAction via shadow entities, no longer the raw-passport adaptation ADR-021 originally shipped.</summary>
internal sealed class EfWorkOrdersWithOriginFinder(MaintenanceDbContext dbContext) : IWorkOrdersWithOriginFinder
{
    public async Task<IReadOnlyList<WorkOrderWithOriginDto>> GetWorkOrdersWithOriginAsync(CancellationToken cancellationToken)
    {
        var query =
            from wo in dbContext.WorkOrders
            where wo.OriginOperationalEventId != null || wo.OriginIncidentActionId != null
            join ev in dbContext.Set<EventManagementOperationalEventReference>()
                on wo.OriginOperationalEventId equals ev.OperationalEventId into evGroup
            from ev in evGroup.DefaultIfEmpty()
            join ia in dbContext.Set<EventManagementIncidentActionReference>()
                on wo.OriginIncidentActionId equals ia.IncidentActionId into iaGroup
            from ia in iaGroup.DefaultIfEmpty()
            select new WorkOrderWithOriginDto(wo.WorkOrderCode, ev != null ? ev.EventCode : null, ia != null ? ia.Title : null, wo.Title);

        return await query.ToListAsync(cancellationToken);
    }
}
