using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RobotType_Create_with_valid_fields_succeeds()
    {
        var type = RobotType.Create(new RobotTypeId(1), "INSPECTION", "Inspection Robot", NowUtc);
        Assert.Equal("INSPECTION", type.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RobotType_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => RobotType.Create(new RobotTypeId(1), code, "Inspection Robot", NowUtc));
    }

    [Fact]
    public void RobotStatus_Create_with_valid_fields_succeeds()
    {
        var status = RobotStatus.Create(new RobotStatusId(1), "AVAILABLE", "Available", NowUtc);
        Assert.Equal("AVAILABLE", status.Code);
    }

    [Fact]
    public void BatteryStatus_Create_with_valid_fields_succeeds()
    {
        var status = BatteryStatus.Create(new BatteryStatusId(1), "NOMINAL", "Nominal", NowUtc);
        Assert.Equal("NOMINAL", status.Code);
    }

    [Fact]
    public void CommunicationStatus_Create_with_valid_fields_succeeds()
    {
        var status = CommunicationStatus.Create(new CommunicationStatusId(1), "CONNECTED", "Connected", NowUtc);
        Assert.Equal("CONNECTED", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CommunicationStatus_Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => CommunicationStatus.Create(new CommunicationStatusId(1), "CONNECTED", name, NowUtc));
    }

    [Fact]
    public void MissionType_Create_with_valid_fields_succeeds()
    {
        var type = MissionType.Create(new MissionTypeId(1), "INSPECTION", "Inspection", NowUtc);
        Assert.Equal("INSPECTION", type.Code);
    }

    [Fact]
    public void MissionStatus_Create_with_valid_fields_succeeds()
    {
        var status = MissionStatus.Create(new MissionStatusId(1), "REQUESTED", "Requested", NowUtc);
        Assert.Equal("REQUESTED", status.Code);
    }

    [Fact]
    public void MissionPriority_Create_with_valid_fields_succeeds()
    {
        var priority = MissionPriority.Create(new MissionPriorityId(1), "ROUTINE", "Routine", NowUtc);
        Assert.Equal("ROUTINE", priority.Code);
    }

    [Fact]
    public void ReadinessStatus_Create_with_valid_fields_succeeds()
    {
        var status = ReadinessStatus.Create(new ReadinessStatusId(1), "BLOCKED", "Blocked", NowUtc);
        Assert.Equal("BLOCKED", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReadinessStatus_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => ReadinessStatus.Create(new ReadinessStatusId(1), code, "Blocked", NowUtc));
    }
}
