using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class CountryTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_iso_codes_succeeds()
    {
        var country = Country.Create(new CountryId(1), "GR", "GRC", "Greece", NowUtc);

        Assert.Equal("GR", country.Iso2Code);
        Assert.Equal("GRC", country.Iso3Code);
    }

    [Theory]
    [InlineData("G")]
    [InlineData("GRE")]
    public void Create_with_wrong_length_iso2_throws(string iso2)
    {
        Assert.Throws<ArgumentException>(() => Country.Create(new CountryId(1), iso2, "GRC", "Greece", NowUtc));
    }

    [Theory]
    [InlineData("GR")]
    [InlineData("GREC")]
    public void Create_with_wrong_length_iso3_throws(string iso3)
    {
        Assert.Throws<ArgumentException>(() => Country.Create(new CountryId(1), "GR", iso3, "Greece", NowUtc));
    }
}
