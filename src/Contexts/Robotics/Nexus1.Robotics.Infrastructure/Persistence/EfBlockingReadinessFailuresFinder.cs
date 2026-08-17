using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.12.5.2 query 4, verbatim: MissionReadinessItem rows where IsBlocking = 1 and status is BLOCKED/EXPIRED.</summary>
internal sealed class EfBlockingReadinessFailuresFinder(RoboticsDbContext dbContext) : IBlockingReadinessFailuresFinder
{
    private static readonly string[] BlockingStatusCodes = ["BLOCKED", "EXPIRED"];

    public async Task<IReadOnlyList<ReadinessFailureDto>> GetBlockingFailuresAsync(long missionId, CancellationToken cancellationToken)
    {
        var query =
            from item in dbContext.MissionReadinessItems
            join a in dbContext.MissionReadinessAssessments on item.MissionReadinessAssessmentId equals a.Id
            where a.MissionId == new MissionId(missionId)
            join s in dbContext.ReadinessStatuses on item.ReadinessStatusId equals s.Id
            where item.IsBlocking && BlockingStatusCodes.Contains(s.Code)
            select new ReadinessFailureDto(item.CheckName, s.Code, item.Detail);

        return await query.ToListAsync(cancellationToken);
    }
}
