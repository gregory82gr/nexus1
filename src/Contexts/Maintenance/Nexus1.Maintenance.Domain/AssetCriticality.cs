using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Business and engineering criticality used for priority, inspection frequency and reporting (atlas C.9.3). Referenced by Asset (AssetCriticalityId is NOT NULL there).</summary>
public sealed class AssetCriticality : Entity<AssetCriticalityId>, IAggregateRoot
{
    private AssetCriticality(AssetCriticalityId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DisplayOrder { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static AssetCriticality Create(
        AssetCriticalityId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AssetCriticality code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("AssetCriticality name must not be empty.", nameof(name));
        }

        return new AssetCriticality(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
