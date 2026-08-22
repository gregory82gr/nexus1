using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.12.5.2 query 1, verbatim: Robot JOIN RobotStatus WHERE Code = 'AVAILABLE' AND IsDeleted = 0, LEFT JOIN ReactorFleet.Unit for unit code.</summary>
internal sealed class EfAvailableRobotsFinder(RoboticsDbContext dbContext) : IAvailableRobotsFinder
{
    private const string AvailableStatusCode = "AVAILABLE";

    public async Task<IReadOnlyList<AvailableRobotDto>> GetAvailableRobotsAsync(CancellationToken cancellationToken)
    {
        var query =
            from r in dbContext.Robots
            where !EF.Property<bool>(r, "IsDeleted")
            join s in dbContext.RobotStatuses on r.RobotStatusId equals s.Id
            where s.Code == AvailableStatusCode
            join u in dbContext.Set<ReactorFleetUnitReference>() on r.HomeUnitId equals u.UnitId into units
            from unit in units.DefaultIfEmpty()
            select new AvailableRobotDto(unit != null ? unit.Code : string.Empty, r.Code, r.Name, s.Code);

        return await query.ToListAsync(cancellationToken);
    }
}
