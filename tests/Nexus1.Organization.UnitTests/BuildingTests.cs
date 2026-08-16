using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class BuildingTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var building = Building.Create(new BuildingId(1), new SiteId(1), "BLD-A", "Building A", NowUtc, floorCount: 3, isControlledArea: true);

        Assert.Equal(3, building.FloorCount);
        Assert.True(building.IsControlledArea);
    }

    [Fact]
    public void Create_with_negative_floor_count_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Building.Create(
            new BuildingId(1), new SiteId(1), "BLD-A", "Building A", NowUtc, floorCount: -1));
    }

    [Fact]
    public void Create_with_null_floor_count_succeeds()
    {
        var building = Building.Create(new BuildingId(1), new SiteId(1), "BLD-A", "Building A", NowUtc);
        Assert.Null(building.FloorCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Building.Create(new BuildingId(1), new SiteId(1), code, "Building A", NowUtc));
    }
}
