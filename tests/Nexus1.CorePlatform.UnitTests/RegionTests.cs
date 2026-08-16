using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class RegionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var region = Region.Create(new RegionId(1), new CountryId(1), "GR-C", "Central Greece", NowUtc);

        Assert.Equal(new CountryId(1), region.CountryId);
        Assert.Equal("GR-C", region.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Region.Create(new RegionId(1), new CountryId(1), code, "Central Greece", NowUtc));
    }
}
