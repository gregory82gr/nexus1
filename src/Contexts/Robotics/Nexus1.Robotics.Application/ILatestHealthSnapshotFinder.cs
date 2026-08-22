namespace Nexus1.Robotics.Application;

public interface ILatestHealthSnapshotFinder
{
    Task<IReadOnlyList<RobotHealthSnapshotDto>> GetLatestHealthSnapshotsAsync(CancellationToken cancellationToken);
}
