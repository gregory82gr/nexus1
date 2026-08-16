using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.CorePlatform.Domain;

/// <summary>Reference table (atlas C.1.4.6): sub-national regions, provinces, states, prefectures. Depends on Country.</summary>
public sealed class Region : Entity<RegionId>, IAggregateRoot
{
    private Region(
        RegionId id, CountryId countryId, string code, string name, string? regionType, bool isActive,
        int displayOrder, DateTime createdAtUtc)
        : base(id)
    {
        CountryId = countryId;
        Code = code;
        Name = name;
        RegionType = regionType;
        IsActive = isActive;
        DisplayOrder = displayOrder;
        CreatedAtUtc = createdAtUtc;
    }

    public CountryId CountryId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? RegionType { get; }

    public bool IsActive { get; }

    public int DisplayOrder { get; }

    public DateTime CreatedAtUtc { get; }

    public static Region Create(
        RegionId id, CountryId countryId, string code, string name, DateTime createdAtUtc,
        string? regionType = null, bool isActive = true, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Region code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Region name must not be empty.", nameof(name));
        }

        return new Region(id, countryId, code, name, regionType, isActive, displayOrder, createdAtUtc);
    }
}
