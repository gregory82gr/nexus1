using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>Reference table (atlas C.1.4.6): ISO country catalogue used by organization, security, and compliance.</summary>
public sealed class Country : Entity<CountryId>, IAggregateRoot
{
    private Country(
        CountryId id, string iso2Code, string iso3Code, string? numericCode, string name, string? officialName,
        bool isActive, int displayOrder, DateTime createdAtUtc)
        : base(id)
    {
        Iso2Code = iso2Code;
        Iso3Code = iso3Code;
        NumericCode = numericCode;
        Name = name;
        OfficialName = officialName;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public string Iso2Code { get; }

    public string Iso3Code { get; }

    public string? NumericCode { get; }

    public string Name { get; }

    public string? OfficialName { get; }

    public bool IsActive { get; }

    public int DisplayOrder { get; }

    public DateTime CreatedAtUtc { get; }

    public static Country Create(
        CountryId id, string iso2Code, string iso3Code, string name, DateTime createdAtUtc,
        string? numericCode = null, string? officialName = null, bool isActive = true, int displayOrder = 0)
    {
        if (iso2Code?.Length != 2)
        {
            throw new ArgumentException("Iso2Code must be exactly 2 characters.", nameof(iso2Code));
        }

        if (iso3Code?.Length != 3)
        {
            throw new ArgumentException("Iso3Code must be exactly 3 characters.", nameof(iso3Code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Country name must not be empty.", nameof(name));
        }

        return new Country(id, iso2Code, iso3Code, numericCode, name, officialName, isActive, displayOrder, createdAtUtc);
    }
}
