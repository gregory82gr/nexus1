using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class AssemblyPointTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var point = AssemblyPoint.Create(new AssemblyPointId(1), "AP-001", "North Parking Lot", siteId: 100);

        Assert.Equal("AP-001", point.Code);
        Assert.Equal("North Parking Lot", point.Name);
        Assert.Equal(100, point.SiteId);
        Assert.Null(point.PlantId);
        Assert.Null(point.RadiationZoneId);
        Assert.Null(point.MaxOccupancy);
        Assert.False(point.IsIndoor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => AssemblyPoint.Create(new AssemblyPointId(1), code, "North Parking Lot", siteId: 100));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => AssemblyPoint.Create(new AssemblyPointId(1), "AP-001", name, siteId: 100));
    }

    [Fact]
    public void Create_with_real_radiation_zone_fk_and_no_local_validation_sets_it()
    {
        var point = AssemblyPoint.Create(
            new AssemblyPointId(1), "AP-001", "North Parking Lot", siteId: 100, radiationZoneId: 7);

        Assert.Equal(7, point.RadiationZoneId);
    }
}
