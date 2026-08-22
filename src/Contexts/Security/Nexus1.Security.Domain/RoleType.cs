using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>Classifies roles (atlas C.2.3): operator, administrator, security, auditor, service account, emergency.</summary>
public sealed class RoleType : Entity<RoleTypeId>, IAggregateRoot
{
    private RoleType(RoleTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static RoleType Create(
        RoleTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RoleType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RoleType name must not be empty.", nameof(name));
        }

        return new RoleType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
