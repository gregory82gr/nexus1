using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class RobotHealthSnapshotTests
{
    private static readonly DateTime SnapshotAtUtc = new(2026, 8, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_defaults_fault_count_zero()
    {
        var snapshot = RobotHealthSnapshot.Create(
            new RobotHealthSnapshotId(1), new RobotId(1), new BatteryStatusId(1), new CommunicationStatusId(1), SnapshotAtUtc);

        Assert.Equal(SnapshotAtUtc, snapshot.SnapshotAtUtc);
        Assert.Equal(0, snapshot.FaultCount);
        Assert.Null(snapshot.BatteryPercent);
        Assert.Null(snapshot.Summary);
    }

    [Fact]
    public void Create_with_full_reading_sets_all_fields()
    {
        var snapshot = RobotHealthSnapshot.Create(
            new RobotHealthSnapshotId(1), new RobotId(1), new BatteryStatusId(1), new CommunicationStatusId(1),
            SnapshotAtUtc, batteryPercent: 87.5m, estimatedRuntimeMin: 120, cpuLoadPercent: 42.0m, faultCount: 2,
            summary: "Minor sensor drift detected");

        Assert.Equal(87.5m, snapshot.BatteryPercent);
        Assert.Equal(120, snapshot.EstimatedRuntimeMin);
        Assert.Equal(42.0m, snapshot.CpuLoadPercent);
        Assert.Equal(2, snapshot.FaultCount);
        Assert.Equal("Minor sensor drift detected", snapshot.Summary);
    }
}
