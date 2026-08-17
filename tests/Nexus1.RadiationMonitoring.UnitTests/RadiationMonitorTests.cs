using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class RadiationMonitorTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_topology_anchors_null()
    {
        var monitor = RadiationMonitor.Create(
            new RadiationMonitorId(1), new MonitorTypeId(1), new MonitorStatusId(1), "RM-001", "Area Monitor 1");

        Assert.Equal("RM-001", monitor.Code);
        Assert.Equal("Area Monitor 1", monitor.Name);
        Assert.Null(monitor.UnitId);
        Assert.Null(monitor.EquipmentId);
        Assert.Null(monitor.RadiationZoneId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => RadiationMonitor.Create(
            new RadiationMonitorId(1), new MonitorTypeId(1), new MonitorStatusId(1), code, "Area Monitor 1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => RadiationMonitor.Create(
            new RadiationMonitorId(1), new MonitorTypeId(1), new MonitorStatusId(1), "RM-001", name));
    }

    [Fact]
    public void Create_with_passport_only_equipment_id_and_a_sited_zone_sets_all_three_anchors()
    {
        var monitor = RadiationMonitor.Create(
            new RadiationMonitorId(1), new MonitorTypeId(1), new MonitorStatusId(1), "RM-001", "Area Monitor 1",
            unitId: 1, equipmentId: 77, radiationZoneId: new RadiationZoneId(5), serialNumber: "SN-1");

        Assert.Equal(1, monitor.UnitId);
        Assert.Equal(77, monitor.EquipmentId);
        Assert.Equal(new RadiationZoneId(5), monitor.RadiationZoneId);
        Assert.Equal("SN-1", monitor.SerialNumber);
    }
}
