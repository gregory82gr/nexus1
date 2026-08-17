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
}
