using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class RadiationZoneTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var zone = RadiationZone.Create(
            new RadiationZoneId(1), new RadiationZoneTypeId(1), new RadiationZoneStatusId(1),
            new RadiationAreaClassificationId(1), "RZ-001", "Reactor Building Zone 1");

        Assert.Equal("RZ-001", zone.Code);
        Assert.Equal("Reactor Building Zone 1", zone.Name);
        Assert.Null(zone.UnitId);
        Assert.Null(zone.EquipmentLocationId);
        Assert.True(zone.IsEntryControlled);
        Assert.True(zone.RequiresDosimeter);
        Assert.Null(zone.PostedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => RadiationZone.Create(
            new RadiationZoneId(1), new RadiationZoneTypeId(1), new RadiationZoneStatusId(1),
            new RadiationAreaClassificationId(1), code, "Reactor Building Zone 1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => RadiationZone.Create(
            new RadiationZoneId(1), new RadiationZoneTypeId(1), new RadiationZoneStatusId(1),
            new RadiationAreaClassificationId(1), "RZ-001", name));
    }

    [Fact]
    public void Create_with_passport_only_equipment_location_id_sets_it_with_no_enforced_fk()
    {
        var zone = RadiationZone.Create(
            new RadiationZoneId(1), new RadiationZoneTypeId(1), new RadiationZoneStatusId(1),
            new RadiationAreaClassificationId(1), "RZ-001", "Reactor Building Zone 1", unitId: 1,
            equipmentLocationId: 42);

        Assert.Equal(1, zone.UnitId);
        Assert.Equal(42, zone.EquipmentLocationId);
    }
}
