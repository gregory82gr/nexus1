using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>Groups permissions by domain (atlas C.2.3): platform, alarm, twin, reporting, audit, security.</summary>
public sealed class PermissionCategory : Entity<PermissionCategoryId>, IAggregateRoot
{
    private PermissionCategory(PermissionCategoryId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static PermissionCategory Create(
        PermissionCategoryId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("PermissionCategory code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("PermissionCategory name must not be empty.", nameof(name));
        }

        return new PermissionCategory(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
