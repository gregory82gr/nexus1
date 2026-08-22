using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class PlantTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var plant = Plant.Create(
            new PlantId(1), new SiteId(1), new PlantTypeId(1), "PLANT-A", "Plant A", NowUtc,
            operationalStartDate: new DateOnly(2020, 1, 1));

        Assert.Equal("PLANT-A", plant.Code);
        Assert.Equal(new DateOnly(2020, 1, 1), plant.OperationalStartDate);
        Assert.True(plant.IsOperational);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Plant.Create(new PlantId(1), new SiteId(1), new PlantTypeId(1), code, "Plant A", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Plant.Create(new PlantId(1), new SiteId(1), new PlantTypeId(1), "PLANT-A", name, NowUtc));
    }
}
