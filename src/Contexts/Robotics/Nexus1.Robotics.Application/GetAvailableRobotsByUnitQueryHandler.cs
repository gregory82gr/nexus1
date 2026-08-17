using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed class GetAvailableRobotsByUnitQueryHandler(IAvailableRobotsFinder finder)
    : IQueryHandler<GetAvailableRobotsByUnitQuery, IReadOnlyList<AvailableRobotDto>>
{
    public async Task<Result<IReadOnlyList<AvailableRobotDto>>> Handle(GetAvailableRobotsByUnitQuery query, CancellationToken cancellationToken)
    {
        var robots = await finder.GetAvailableRobotsAsync(cancellationToken);
        return Result<IReadOnlyList<AvailableRobotDto>>.Success(robots);
    }
}
