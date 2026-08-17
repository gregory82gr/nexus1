namespace Nexus1.Robotics.Application;

/// <summary>Atlas C.12.5.2 query 2, verbatim: latest health snapshot for each robot.</summary>
public sealed record RobotHealthSnapshotDto(
    string RobotCode, DateTime SnapshotAtUtc, decimal? BatteryPercent, string BatteryStatus, string CommunicationStatus);
