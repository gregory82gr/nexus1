using Microsoft.EntityFrameworkCore;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.13.5.2 query 3, verbatim: one row per monitor,
/// most recent RadiationReading, with engineering-unit symbol and quality
/// code. Uses the correlated-subquery pattern (translates to SQL Server
/// OUTER APPLY) rather than GroupBy+OrderByDescending+First followed by a
/// join — Robotics' own EfLatestHealthSnapshotFinder discovered that
/// combination does not translate ("ProjectionBindingExpression could not
/// be translated"), caught by the real component test against LocalDB, not
/// by rule/pipeline-layer tests alone (ADR-023, carried forward here).
/// </summary>
internal sealed class EfLatestReadingPerMonitorFinder(RadiationMonitoringDbContext dbContext) : ILatestReadingPerMonitorFinder
{
    public async Task<IReadOnlyList<LatestRadiationReadingDto>> GetLatestReadingsAsync(CancellationToken cancellationToken)
    {
        var query =
            from m in dbContext.RadiationMonitors
            let latest = dbContext.RadiationReadings
                .Where(r => r.RadiationMonitorId == m.Id)
                .OrderByDescending(r => r.TimestampUtc)
                .FirstOrDefault()
            where latest != null
            join u in dbContext.Set<CorePlatformEngineeringUnitReference>() on latest!.EngineeringUnitId equals u.EngineeringUnitId
            join q in dbContext.MeasurementQualities on latest!.MeasurementQualityId equals q.Id
            select new LatestRadiationReadingDto(m.Code, latest!.TimestampUtc, latest.Value, u.Symbol, q.Code);

        return await query.ToListAsync(cancellationToken);
    }
}
