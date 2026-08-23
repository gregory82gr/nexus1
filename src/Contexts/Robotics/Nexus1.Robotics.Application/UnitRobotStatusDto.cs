namespace Nexus1.Robotics.Application;

/// <summary>
/// One robot home-based at the unit, current status, and latest health
/// snapshot if it has one. Latest* fields are nullable — a robot can exist
/// with zero recorded health snapshots yet.
/// </summary>
public sealed record UnitRobotStatusDto(
    string RobotCode,
    string RobotName,
    string RobotStatus,
    decimal? LatestBatteryPercent,
    string? LatestBatteryStatus,
    string? LatestCommunicationStatus,
    DateTime? LatestSnapshotAtUtc);
