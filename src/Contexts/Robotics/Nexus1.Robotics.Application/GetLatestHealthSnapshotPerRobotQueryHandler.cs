using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Application;

public sealed class GetLatestHealthSnapshotPerRobotQueryHandler(ILatestHealthSnapshotFinder finder)
    : IQueryHandler<GetLatestHealthSnapshotPerRobotQuery, IReadOnlyList<RobotHealthSnapshotDto>>
{
    public async Task<Result<IReadOnlyList<RobotHealthSnapshotDto>>> Handle(GetLatestHealthSnapshotPerRobotQuery query, CancellationToken cancellationToken)
    {
        var snapshots = await finder.GetLatestHealthSnapshotsAsync(cancellationToken);
        return Result<IReadOnlyList<RobotHealthSnapshotDto>>.Success(snapshots);
    }
}
