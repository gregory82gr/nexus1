namespace Nexus1.Robotics.Application;

public interface ILatestHealthSnapshotFinder
{
    Task<IReadOnlyList<RobotHealthSnapshotDto>> GetLatestHealthSnapshotsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Per-unit (robots home-based there, via Robot.HomeUnitId), unlike
    /// GetLatestHealthSnapshotsAsync (fleet-wide, no unit filter) — added
    /// for the BFF's Fleet Overview screen. Includes robot status/code/name
    /// alongside latest health, broader than RobotHealthSnapshotDto alone.
    /// Includes a robot with zero snapshots yet (null health fields) rather
    /// than excluding it, same reasoning as RadiationMonitoring's per-unit
    /// monitor-reading finder.
    /// </summary>
    Task<IReadOnlyList<UnitRobotStatusDto>> GetRobotStatusForUnitAsync(int unitId, CancellationToken cancellationToken);
}
