using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// A physical site: plant campus, headquarters, training centre, or support
/// facility (atlas C.3.4.3). CountryId/RegionId/TimeZoneId are plain
/// passport ints, not real FKs: CorePlatform lives in AlarmManagementDb
/// while Organization gets its own OrganizationDb (ADR-017), so real
/// cross-database FOREIGN KEYs are not possible.
/// </summary>
public sealed class Site : Entity<SiteId>, IAggregateRoot
{
    private Site(
        SiteId id, LegalEntityId legalEntityId, SiteTypeId siteTypeId, int countryId, int? regionId, int timeZoneId,
        string code, string name, string? addressLine1, string? addressLine2, string? city, string? postalCode,
        decimal? latitude, decimal? longitude, bool isOperational, DateTime createdAtUtc)
        : base(id)
    {
        LegalEntityId = legalEntityId;
        SiteTypeId = siteTypeId;
        CountryId = countryId;
        RegionId = regionId;
        TimeZoneId = timeZoneId;
        Code = code;
        Name = name;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        IsOperational = isOperational;
        CreatedAtUtc = createdAtUtc;
    }

    public LegalEntityId LegalEntityId { get; }

    public SiteTypeId SiteTypeId { get; }

    /// <summary>CorePlatform.Country passport id — no enforced FK (ADR-017).</summary>
    public int CountryId { get; }

    /// <summary>CorePlatform.Region passport id — no enforced FK (ADR-017).</summary>
    public int? RegionId { get; }

    /// <summary>CorePlatform.TimeZone passport id — no enforced FK (ADR-017).</summary>
    public int TimeZoneId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? AddressLine1 { get; }

    public string? AddressLine2 { get; }

    public string? City { get; }

    public string? PostalCode { get; }

    public decimal? Latitude { get; }

    public decimal? Longitude { get; }

    public bool IsOperational { get; }

    public DateTime CreatedAtUtc { get; }

    public static Site Create(
        SiteId id, LegalEntityId legalEntityId, SiteTypeId siteTypeId, int countryId, int timeZoneId, string code,
        string name, DateTime createdAtUtc, int? regionId = null, string? addressLine1 = null,
        string? addressLine2 = null, string? city = null, string? postalCode = null, decimal? latitude = null,
        decimal? longitude = null, bool isOperational = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Site code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Site name must not be empty.", nameof(name));
        }

        if (latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), latitude, "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), longitude, "Longitude must be between -180 and 180.");
        }

        return new Site(
            id, legalEntityId, siteTypeId, countryId, regionId, timeZoneId, code, name, addressLine1, addressLine2,
            city, postalCode, latitude, longitude, isOperational, createdAtUtc);
    }
}
