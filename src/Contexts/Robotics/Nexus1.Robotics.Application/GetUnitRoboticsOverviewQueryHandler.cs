using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed class GetUnitRoboticsOverviewQueryHandler(
    ILatestHealthSnapshotFinder healthFinder, IUnitMissionsFinder missionsFinder)
    : IQueryHandler<GetUnitRoboticsOverviewQuery, UnitRoboticsOverviewDto>
{
    public async Task<Result<UnitRoboticsOverviewDto>> Handle(GetUnitRoboticsOverviewQuery query, CancellationToken cancellationToken)
    {
        var robots = await healthFinder.GetRobotStatusForUnitAsync(query.UnitId, cancellationToken);
        var missions = await missionsFinder.GetMissionsForUnitAsync(query.UnitId, cancellationToken);

        return Result<UnitRoboticsOverviewDto>.Success(new UnitRoboticsOverviewDto(query.UnitId, robots, missions));
    }
}
