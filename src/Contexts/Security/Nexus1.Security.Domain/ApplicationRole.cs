using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>
/// Named role, optionally hierarchical via ParentRoleId (atlas C.2.4.4:
/// "allows a broad operator role to parent more specific roles without
/// merging their meanings").
/// </summary>
public sealed class ApplicationRole : Entity<ApplicationRoleId>, IAggregateRoot
{
    private ApplicationRole(
        ApplicationRoleId id, RoleTypeId roleTypeId, string name, string normalizedName, string? description,
        ApplicationRoleId? parentRoleId, bool isBuiltIn, DateTime createdAtUtc)
        : base(id)
    {
        RoleTypeId = roleTypeId;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
        ParentRoleId = parentRoleId;
        IsBuiltIn = isBuiltIn;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public RoleTypeId RoleTypeId { get; }

    public string Name { get; }

    public string NormalizedName { get; }

    public string? Description { get; }

    public ApplicationRoleId? ParentRoleId { get; }

    public bool IsBuiltIn { get; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public static ApplicationRole Create(
        ApplicationRoleId id, RoleTypeId roleTypeId, string name, string normalizedName, DateTime createdAtUtc,
        string? description = null, ApplicationRoleId? parentRoleId = null, bool isBuiltIn = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ApplicationRole name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException("ApplicationRole normalized name must not be empty.", nameof(normalizedName));
        }

        if (parentRoleId == id)
        {
            throw new ArgumentException("A role cannot be its own parent.", nameof(parentRoleId));
        }

        return new ApplicationRole(id, roleTypeId, name, normalizedName, description, parentRoleId, isBuiltIn, createdAtUtc);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
