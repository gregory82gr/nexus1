using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Application;

namespace Nexus1.Robotics.Infrastructure.Persistence;

internal sealed class EfUnitMissionsFinder(RoboticsDbContext dbContext) : IUnitMissionsFinder
{
    public async Task<IReadOnlyList<UnitMissionDto>> GetMissionsForUnitAsync(int unitId, CancellationToken cancellationToken)
    {
        var query =
            from m in dbContext.Missions
            where !EF.Property<bool>(m, "IsDeleted") && m.UnitId == unitId
            join mt in dbContext.MissionTypes on m.MissionTypeId equals mt.Id
            join ms in dbContext.MissionStatuses on m.MissionStatusId equals ms.Id
            join mp in dbContext.MissionPriorities on m.MissionPriorityId equals mp.Id
            orderby m.RequestedAtUtc descending
            select new UnitMissionDto(
                m.Code, m.Title, mt.Code, ms.Code, mp.Code,
                m.RequestedAtUtc, m.PlannedStartUtc, m.PlannedEndUtc, m.ActualStartUtc, m.ActualEndUtc);

        return await query.ToListAsync(cancellationToken);
    }
}
