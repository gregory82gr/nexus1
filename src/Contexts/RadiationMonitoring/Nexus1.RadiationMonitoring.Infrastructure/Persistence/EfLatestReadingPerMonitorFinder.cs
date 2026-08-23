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

    /// <summary>
    /// Independent correlated scalar subqueries per field (not one combined
    /// projection) — the same shape proven to translate for ReactorFleet's
    /// EfUnitFleetFinder. Deliberately not the `let latest = ... select new
    /// LatestRadiationReadingDto(...)` shape GetLatestReadingsAsync uses above
    /// (which would need `latest != null` to exclude an unsited/unreported
    /// monitor) — here a monitor with zero readings must still appear, with
    /// null fields, not be filtered out. Lookup codes (engineering-unit
    /// symbol, quality code) are resolved in a separate in-memory pass rather
    /// than joined inside an ordered correlated subquery — joining after
    /// OrderByDescending inside a subquery is exactly the kind of shape this
    /// file's own top comment already found EF Core failing to translate
    /// once (Robotics' GroupBy+OrderByDescending+First incident); both lookup
    /// tables are small, so resolving them as two plain dictionary reads
    /// after materializing the monitor rows is the safer, proven-simple
    /// choice, not a performance concession that matters at this scale.
    /// </summary>
    public async Task<IReadOnlyList<UnitRadiationMonitorReadingDto>> GetLatestReadingsForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var monitorRows = await dbContext.RadiationMonitors
            .Where(m => m.UnitId == unitId)
            .Join(dbContext.MonitorStatuses, m => m.MonitorStatusId, ms => ms.Id, (m, ms) => new
            {
                m.Code,
                m.Name,
                MonitorStatus = ms.Code,
                LatestValue = dbContext.RadiationReadings
                    .Where(r => r.RadiationMonitorId == m.Id)
                    .OrderByDescending(r => r.TimestampUtc)
                    .Select(r => (decimal?)r.Value)
                    .FirstOrDefault(),
                LatestTimestampUtc = dbContext.RadiationReadings
                    .Where(r => r.RadiationMonitorId == m.Id)
                    .OrderByDescending(r => r.TimestampUtc)
                    .Select(r => (DateTime?)r.TimestampUtc)
                    .FirstOrDefault(),
                LatestEngineeringUnitId = dbContext.RadiationReadings
                    .Where(r => r.RadiationMonitorId == m.Id)
                    .OrderByDescending(r => r.TimestampUtc)
                    .Select(r => (int?)r.EngineeringUnitId)
                    .FirstOrDefault(),
                LatestMeasurementQualityId = dbContext.RadiationReadings
                    .Where(r => r.RadiationMonitorId == m.Id)
                    .OrderByDescending(r => r.TimestampUtc)
                    .Select(r => (int?)r.MeasurementQualityId.Value)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var engineeringUnitSymbolsById = await dbContext.Set<CorePlatformEngineeringUnitReference>()
            .ToDictionaryAsync(u => u.EngineeringUnitId, u => u.Symbol, cancellationToken);
        var qualityCodesById = await dbContext.MeasurementQualities
            .ToDictionaryAsync(q => q.Id.Value, q => q.Code, cancellationToken);

        return monitorRows
            .Select(x => new UnitRadiationMonitorReadingDto(
                x.Code,
                x.Name,
                x.MonitorStatus,
                x.LatestValue,
                x.LatestEngineeringUnitId is int euId && engineeringUnitSymbolsById.TryGetValue(euId, out var symbol) ? symbol : null,
                x.LatestMeasurementQualityId is int qId && qualityCodesById.TryGetValue(qId, out var code) ? code : null,
                x.LatestTimestampUtc))
            .ToList();
    }
}
