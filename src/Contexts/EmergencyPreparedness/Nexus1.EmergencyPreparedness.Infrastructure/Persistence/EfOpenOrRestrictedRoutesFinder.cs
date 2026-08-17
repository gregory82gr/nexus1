using Microsoft.EntityFrameworkCore;
using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own verification query 3, verbatim: EvacuationRoute
/// JOIN RouteStatus WHERE Code IN ('OPEN','RESTRICTED'), JOIN
/// EvacuationRouteZone, JOIN RadiationMonitoring.RadiationZone (via the
/// RadiationMonitoringRadiationZoneReference shadow entity) for the zone
/// code.
/// </summary>
internal sealed class EfOpenOrRestrictedRoutesFinder(EmergencyPreparednessDbContext dbContext) : IOpenOrRestrictedRoutesFinder
{
    private static readonly string[] OpenOrRestrictedStatusCodes = ["OPEN", "RESTRICTED"];

    public async Task<IReadOnlyList<RouteCrossingZoneDto>> GetOpenOrRestrictedRoutesCrossingZonesAsync(CancellationToken cancellationToken)
    {
        var query =
            from r in dbContext.EvacuationRoutes
            join s in dbContext.RouteStatuses on r.RouteStatusId equals s.Id
            where OpenOrRestrictedStatusCodes.Contains(s.Code)
            join erz in dbContext.EvacuationRouteZones on r.Id equals erz.EvacuationRouteId
            join z in dbContext.Set<RadiationMonitoringRadiationZoneReference>() on erz.RadiationZoneId equals z.RadiationZoneId
            select new RouteCrossingZoneDto(r.Code, s.Code, z.Code, erz.IsAvoidIfAlarmed);

        return await query.ToListAsync(cancellationToken);
    }
}
