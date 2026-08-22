using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>
/// An append-only fact record of a robot's health at a point in time
/// (ADR-023, atlas query 2's own subject). RobotId, BatteryStatusId and
/// CommunicationStatusId are all real internal FKs and NOT NULL. No audit
/// columns at all — append-only, all fields modeled directly in Domain.
/// </summary>
public sealed class RobotHealthSnapshot : Entity<RobotHealthSnapshotId>, IAggregateRoot
{
    private RobotHealthSnapshot(
        RobotHealthSnapshotId id, RobotId robotId, BatteryStatusId batteryStatusId,
        CommunicationStatusId communicationStatusId, DateTime snapshotAtUtc, decimal? batteryPercent,
        int? estimatedRuntimeMin, decimal? cpuLoadPercent, int faultCount, string? summary)
        : base(id)
    {
        RobotId = robotId;
        BatteryStatusId = batteryStatusId;
        CommunicationStatusId = communicationStatusId;
        SnapshotAtUtc = snapshotAtUtc;
        BatteryPercent = batteryPercent;
        EstimatedRuntimeMin = estimatedRuntimeMin;
        CpuLoadPercent = cpuLoadPercent;
        FaultCount = faultCount;
        Summary = summary;
    }

    public RobotId RobotId { get; }

    public BatteryStatusId BatteryStatusId { get; }

    public CommunicationStatusId CommunicationStatusId { get; }

    public DateTime SnapshotAtUtc { get; }

    public decimal? BatteryPercent { get; }

    public int? EstimatedRuntimeMin { get; }

    public decimal? CpuLoadPercent { get; }

    public int FaultCount { get; }

    public string? Summary { get; }

    public static RobotHealthSnapshot Create(
        RobotHealthSnapshotId id, RobotId robotId, BatteryStatusId batteryStatusId,
        CommunicationStatusId communicationStatusId, DateTime snapshotAtUtc, decimal? batteryPercent = null,
        int? estimatedRuntimeMin = null, decimal? cpuLoadPercent = null, int faultCount = 0, string? summary = null)
    {
        return new RobotHealthSnapshot(
            id, robotId, batteryStatusId, communicationStatusId, snapshotAtUtc, batteryPercent,
            estimatedRuntimeMin, cpuLoadPercent, faultCount, summary);
    }
}
