using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class RobotTests
{
    private static readonly DateTime CommissionedAtUtc = new(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_null()
    {
        var robot = Robot.Create(new RobotId(1), new RobotModelId(1), new RobotStatusId(1), "RBT-001", "Scout One");

        Assert.Equal("RBT-001", robot.Code);
        Assert.Equal("Scout One", robot.Name);
        Assert.Null(robot.HomeUnitId);
        Assert.Null(robot.SerialNumber);
        Assert.Null(robot.CommissionedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Robot.Create(
            new RobotId(1), new RobotModelId(1), new RobotStatusId(1), code, "Scout One"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Robot.Create(
            new RobotId(1), new RobotModelId(1), new RobotStatusId(1), "RBT-001", name));
    }

    [Fact]
    public void Create_with_passport_only_home_unit_id_sets_it_with_no_enforced_fk()
    {
        var robot = Robot.Create(
            new RobotId(1), new RobotModelId(1), new RobotStatusId(1), "RBT-001", "Scout One",
            homeUnitId: 1, serialNumber: "SN-12345", commissionedAtUtc: CommissionedAtUtc);

        Assert.Equal(1, robot.HomeUnitId);
        Assert.Equal("SN-12345", robot.SerialNumber);
        Assert.Equal(CommissionedAtUtc, robot.CommissionedAtUtc);
    }
}
