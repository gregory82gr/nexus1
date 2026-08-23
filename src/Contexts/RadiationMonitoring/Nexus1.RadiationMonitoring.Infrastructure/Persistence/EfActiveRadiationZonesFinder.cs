using Microsoft.EntityFrameworkCore;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.13.5.2 query 1, verbatim: RadiationZone WHERE IsDeleted = 0, JOIN RadiationAreaClassification/RadiationZoneStatus, LEFT JOIN ReactorFleet.Unit for unit code.</summary>
internal sealed class EfActiveRadiationZonesFinder(RadiationMonitoringDbContext dbContext) : IActiveRadiationZonesFinder
{
    public async Task<IReadOnlyList<ActiveRadiationZoneDto>> GetActiveRadiationZonesAsync(CancellationToken cancellationToken)
    {
        var query =
            from z in dbContext.RadiationZones
            where !EF.Property<bool>(z, "IsDeleted")
            join c in dbContext.RadiationAreaClassifications on z.RadiationAreaClassificationId equals c.Id
            join s in dbContext.RadiationZoneStatuses on z.RadiationZoneStatusId equals s.Id
            join u in dbContext.Set<ReactorFleetUnitReference>() on z.UnitId equals u.UnitId into units
            from unit in units.DefaultIfEmpty()
            select new ActiveRadiationZoneDto(z.Code, z.Name, unit != null ? unit.Code : null, c.Code, s.Code);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UnitRadiationZoneDto>> GetActiveRadiationZonesForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var query =
            from z in dbContext.RadiationZones
            where !EF.Property<bool>(z, "IsDeleted") && z.UnitId == unitId
            join c in dbContext.RadiationAreaClassifications on z.RadiationAreaClassificationId equals c.Id
            join s in dbContext.RadiationZoneStatuses on z.RadiationZoneStatusId equals s.Id
            select new UnitRadiationZoneDto(z.Code, z.Name, c.Code, s.Code);

        return await query.ToListAsync(cancellationToken);
    }
}
