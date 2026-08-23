using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Application;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.12.5.2 query 2, verbatim: one row per robot,
/// most recent RobotHealthSnapshot, with battery/communication status
/// codes. Uses the correlated-subquery pattern (translates to SQL Server
/// OUTER APPLY) rather than GroupBy+OrderByDescending+First followed by a
/// join — that combination does not translate ("ProjectionBindingExpression
/// could not be translated"), caught by the real component test against
/// LocalDB, not by rule/pipeline-layer tests alone.
/// </summary>
internal sealed class EfLatestHealthSnapshotFinder(RoboticsDbContext dbContext) : ILatestHealthSnapshotFinder
{
    public async Task<IReadOnlyList<RobotHealthSnapshotDto>> GetLatestHealthSnapshotsAsync(CancellationToken cancellationToken)
    {
        var query =
            from r in dbContext.Robots
            let latest = dbContext.RobotHealthSnapshots
                .Where(s => s.RobotId == r.Id)
                .OrderByDescending(s => s.SnapshotAtUtc)
                .FirstOrDefault()
            where latest != null
            join b in dbContext.BatteryStatuses on latest!.BatteryStatusId equals b.Id
            join c in dbContext.CommunicationStatuses on latest!.CommunicationStatusId equals c.Id
            select new RobotHealthSnapshotDto(r.Code, latest!.SnapshotAtUtc, latest.BatteryPercent, b.Code, c.Code);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Per-unit, includes robots with zero snapshots (unlike the fleet-wide
    /// query above, which excludes via "where latest != null"). Lookup
    /// codes (battery/communication status) are resolved in a separate
    /// in-memory pass rather than joined inside the ordered correlated
    /// subquery — same translation-safety reasoning as
    /// RadiationMonitoring's per-unit finder (joining after OrderByDescending
    /// inside a subquery is exactly the shape this file's own top comment
    /// already found EF Core failing to translate once).
    /// </summary>
    public async Task<IReadOnlyList<UnitRobotStatusDto>> GetRobotStatusForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var robotRows = await dbContext.Robots
            .Where(r => !EF.Property<bool>(r, "IsDeleted") && r.HomeUnitId == unitId)
            .Join(dbContext.RobotStatuses, r => r.RobotStatusId, rs => rs.Id, (r, rs) => new
            {
                r.Code,
                r.Name,
                RobotStatus = rs.Code,
                LatestBatteryPercent = dbContext.RobotHealthSnapshots
                    .Where(s => s.RobotId == r.Id)
                    .OrderByDescending(s => s.SnapshotAtUtc)
                    .Select(s => s.BatteryPercent)
                    .FirstOrDefault(),
                LatestSnapshotAtUtc = dbContext.RobotHealthSnapshots
                    .Where(s => s.RobotId == r.Id)
                    .OrderByDescending(s => s.SnapshotAtUtc)
                    .Select(s => (DateTime?)s.SnapshotAtUtc)
                    .FirstOrDefault(),
                LatestBatteryStatusId = dbContext.RobotHealthSnapshots
                    .Where(s => s.RobotId == r.Id)
                    .OrderByDescending(s => s.SnapshotAtUtc)
                    .Select(s => (int?)s.BatteryStatusId.Value)
                    .FirstOrDefault(),
                LatestCommunicationStatusId = dbContext.RobotHealthSnapshots
                    .Where(s => s.RobotId == r.Id)
                    .OrderByDescending(s => s.SnapshotAtUtc)
                    .Select(s => (int?)s.CommunicationStatusId.Value)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var batteryStatusCodesById = await dbContext.BatteryStatuses
            .ToDictionaryAsync(b => b.Id.Value, b => b.Code, cancellationToken);
        var communicationStatusCodesById = await dbContext.CommunicationStatuses
            .ToDictionaryAsync(c => c.Id.Value, c => c.Code, cancellationToken);

        return robotRows
            .Select(x => new UnitRobotStatusDto(
                x.Code,
                x.Name,
                x.RobotStatus,
                x.LatestBatteryPercent,
                x.LatestBatteryStatusId is int bId && batteryStatusCodesById.TryGetValue(bId, out var bCode) ? bCode : null,
                x.LatestCommunicationStatusId is int cId && communicationStatusCodesById.TryGetValue(cId, out var cCode) ? cCode : null,
                x.LatestSnapshotAtUtc))
            .ToList();
    }
}
