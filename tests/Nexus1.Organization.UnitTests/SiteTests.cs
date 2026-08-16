using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class SiteTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var site = Site.Create(
            new SiteId(1), new LegalEntityId(1), new SiteTypeId(1), countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc,
            latitude: 45.5m, longitude: -73.6m);

        Assert.Equal("SITE-A", site.Code);
        Assert.Equal(45.5m, site.Latitude);
        Assert.True(site.IsOperational);
    }

    [Theory]
    [InlineData(-90.000001)]
    [InlineData(90.000001)]
    public void Create_with_out_of_range_latitude_throws(double latitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Site.Create(
            new SiteId(1), new LegalEntityId(1), new SiteTypeId(1), countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc,
            latitude: (decimal)latitude));
    }

    [Theory]
    [InlineData(-180.000001)]
    [InlineData(180.000001)]
    public void Create_with_out_of_range_longitude_throws(double longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Site.Create(
            new SiteId(1), new LegalEntityId(1), new SiteTypeId(1), countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc,
            longitude: (decimal)longitude));
    }

    [Theory]
    [InlineData(-90)]
    [InlineData(90)]
    [InlineData(0)]
    public void Create_with_boundary_latitude_succeeds(double latitude)
    {
        var site = Site.Create(
            new SiteId(1), new LegalEntityId(1), new SiteTypeId(1), countryId: 1, timeZoneId: 1, "SITE-A", "Site A", NowUtc,
            latitude: (decimal)latitude);

        Assert.Equal((decimal)latitude, site.Latitude);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Site.Create(
            new SiteId(1), new LegalEntityId(1), new SiteTypeId(1), countryId: 1, timeZoneId: 1, code, "Site A", NowUtc));
    }
}
