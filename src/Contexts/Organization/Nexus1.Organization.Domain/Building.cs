using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Building or facility structure used by access, radiation, maintenance, and emergency sectors (atlas C.3.4.3).</summary>
public sealed class Building : Entity<BuildingId>, IAggregateRoot
{
    private Building(
        BuildingId id, SiteId siteId, string code, string name, string? buildingUsage, int? floorCount,
        bool isControlledArea, DateTime createdAtUtc)
        : base(id)
    {
        SiteId = siteId;
        Code = code;
        Name = name;
        BuildingUsage = buildingUsage;
        FloorCount = floorCount;
        IsControlledArea = isControlledArea;
        CreatedAtUtc = createdAtUtc;
    }

    public SiteId SiteId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? BuildingUsage { get; }

    public int? FloorCount { get; }

    public bool IsControlledArea { get; }

    public DateTime CreatedAtUtc { get; }

    public static Building Create(
        BuildingId id, SiteId siteId, string code, string name, DateTime createdAtUtc, string? buildingUsage = null,
        int? floorCount = null, bool isControlledArea = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Building code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Building name must not be empty.", nameof(name));
        }

        if (floorCount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(floorCount), floorCount, "FloorCount must be >= 0.");
        }

        return new Building(id, siteId, code, name, buildingUsage, floorCount, isControlledArea, createdAtUtc);
    }
}
