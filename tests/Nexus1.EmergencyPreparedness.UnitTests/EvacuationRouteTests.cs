using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class EvacuationRouteTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var route = EvacuationRoute.Create(
            new EvacuationRouteId(1), "ER-001", "Turbine Hall Evacuation Route", siteId: 100,
            new AssemblyPointId(1), new RouteStatusId(1), "Turbine Hall Main Floor");

        Assert.Equal("ER-001", route.Code);
        Assert.Equal("Turbine Hall Evacuation Route", route.Name);
        Assert.Equal(100, route.SiteId);
        Assert.Null(route.PlantId);
        Assert.Equal(new AssemblyPointId(1), route.AssemblyPointId);
        Assert.Null(route.EstimatedMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => EvacuationRoute.Create(
            new EvacuationRouteId(1), code, "Turbine Hall Evacuation Route", siteId: 100,
            new AssemblyPointId(1), new RouteStatusId(1), "Turbine Hall Main Floor"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_from_location_throws(string fromLocation)
    {
        Assert.Throws<ArgumentException>(() => EvacuationRoute.Create(
            new EvacuationRouteId(1), "ER-001", "Turbine Hall Evacuation Route", siteId: 100,
            new AssemblyPointId(1), new RouteStatusId(1), fromLocation));
    }

    [Fact]
    public void Create_with_passport_only_plant_id_sets_it_with_no_enforced_fk()
    {
        var route = EvacuationRoute.Create(
            new EvacuationRouteId(1), "ER-001", "Turbine Hall Evacuation Route", siteId: 100,
            new AssemblyPointId(1), new RouteStatusId(1), "Turbine Hall Main Floor", plantId: 42);

        Assert.Equal(42, route.PlantId);
    }
}
