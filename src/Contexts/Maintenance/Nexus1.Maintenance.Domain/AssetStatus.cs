using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Runtime maintenance status: in service, degraded, out of service, under maintenance, retired (atlas C.9.3). Referenced by Asset.</summary>
public sealed class AssetStatus : Entity<AssetStatusId>, IAggregateRoot
{
    private AssetStatus(AssetStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static AssetStatus Create(
        AssetStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AssetStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("AssetStatus name must not be empty.", nameof(name));
        }

        return new AssetStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
