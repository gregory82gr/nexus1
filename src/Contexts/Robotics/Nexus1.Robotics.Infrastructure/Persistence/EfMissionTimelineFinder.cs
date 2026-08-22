using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.12.5.2 query 3, verbatim: mission timeline for one mission, ordered by OccurredAtUtc, with the acting robot's code.</summary>
internal sealed class EfMissionTimelineFinder(RoboticsDbContext dbContext) : IMissionTimelineFinder
{
    public async Task<IReadOnlyList<MissionTimelineEntryDto>> GetTimelineAsync(long missionId, CancellationToken cancellationToken)
    {
        var id = new MissionId(missionId);
        var query =
            from e in dbContext.MissionEvents
            where e.MissionId == id
            join r in dbContext.Robots on e.RobotId equals r.Id into robots
            from robot in robots.DefaultIfEmpty()
            orderby e.OccurredAtUtc
            select new MissionTimelineEntryDto(e.OccurredAtUtc, e.EventCode, e.Title, robot != null ? robot.Code : null);

        return await query.ToListAsync(cancellationToken);
    }
}
