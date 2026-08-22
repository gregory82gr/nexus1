using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class RobotModelTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_defaults_is_autonomous_capable_false()
    {
        var model = RobotModel.Create(
            new RobotModelId(1), new RobotTypeId(1), "INSP-2000", "Acme Robotics", "Inspector 2000");

        Assert.Equal("INSP-2000", model.Code);
        Assert.Equal("Acme Robotics", model.Manufacturer);
        Assert.Equal("Inspector 2000", model.ModelName);
        Assert.False(model.IsAutonomousCapable);
        Assert.Null(model.MaxPayloadKg);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => RobotModel.Create(
            new RobotModelId(1), new RobotTypeId(1), code, "Acme Robotics", "Inspector 2000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_manufacturer_throws(string manufacturer)
    {
        Assert.Throws<ArgumentException>(() => RobotModel.Create(
            new RobotModelId(1), new RobotTypeId(1), "INSP-2000", manufacturer, "Inspector 2000"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_model_name_throws(string modelName)
    {
        Assert.Throws<ArgumentException>(() => RobotModel.Create(
            new RobotModelId(1), new RobotTypeId(1), "INSP-2000", "Acme Robotics", modelName));
    }

    [Fact]
    public void Create_with_full_specification_sets_all_fields()
    {
        var model = RobotModel.Create(
            new RobotModelId(1), new RobotTypeId(1), "INSP-2000", "Acme Robotics", "Inspector 2000",
            description: "Wheeled inspection platform", maxPayloadKg: 25.5m, maxSpeedMps: 1.2m,
            batteryCapacityWh: 480m, nominalRuntimeMin: 240, isAutonomousCapable: true);

        Assert.Equal(25.5m, model.MaxPayloadKg);
        Assert.Equal(1.2m, model.MaxSpeedMps);
        Assert.Equal(480m, model.BatteryCapacityWh);
        Assert.Equal(240, model.NominalRuntimeMin);
        Assert.True(model.IsAutonomousCapable);
    }
}
