using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class EmergencyResourceTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var resource = EmergencyResource.Create(
            new EmergencyResourceId(1), "ER-001", "Portable Air Compressor", new ResourceTypeId(1),
            new ResourceStatusId(1), siteId: 100);

        Assert.Equal("ER-001", resource.Code);
        Assert.Equal("Portable Air Compressor", resource.Name);
        Assert.Equal(100, resource.SiteId);
        Assert.Null(resource.PlantId);
        Assert.Null(resource.OwnerTeamId);
        Assert.Null(resource.QuantityOnHand);
        Assert.Null(resource.EngineeringUnitId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => EmergencyResource.Create(
            new EmergencyResourceId(1), code, "Portable Air Compressor", new ResourceTypeId(1),
            new ResourceStatusId(1), siteId: 100));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => EmergencyResource.Create(
            new EmergencyResourceId(1), "ER-001", name, new ResourceTypeId(1),
            new ResourceStatusId(1), siteId: 100));
    }

    [Fact]
    public void Create_with_real_engineering_unit_fk_and_passport_only_owner_team_sets_both()
    {
        var resource = EmergencyResource.Create(
            new EmergencyResourceId(1), "ER-001", "Portable Air Compressor", new ResourceTypeId(1),
            new ResourceStatusId(1), siteId: 100, ownerTeamId: 9, quantityOnHand: 3.5m, engineeringUnitId: 4);

        Assert.Equal(9, resource.OwnerTeamId);
        Assert.Equal(3.5m, resource.QuantityOnHand);
        Assert.Equal(4, resource.EngineeringUnitId);
    }
}
