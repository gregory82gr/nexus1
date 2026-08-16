using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Security.Domain;

/// <summary>Atomic permission (atlas C.2.4.5), such as alarm acknowledgement, report export, or security administration.</summary>
public sealed class Permission : Entity<PermissionId>, IAggregateRoot
{
    private Permission(
        PermissionId id, PermissionCategoryId permissionCategoryId, string code, string name, string actionName,
        string? description, string? resourceType, bool isSafetyRelevant, DateTime createdAtUtc)
        : base(id)
    {
        PermissionCategoryId = permissionCategoryId;
        Code = code;
        Name = name;
        ActionName = actionName;
        Description = description;
        ResourceType = resourceType;
        IsSafetyRelevant = isSafetyRelevant;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public PermissionCategoryId PermissionCategoryId { get; }

    public string Code { get; }

    public string Name { get; }

    public string ActionName { get; }

    public string? Description { get; }

    public string? ResourceType { get; }

    public bool IsSafetyRelevant { get; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public static Permission Create(
        PermissionId id, PermissionCategoryId permissionCategoryId, string code, string name, string actionName,
        DateTime createdAtUtc, string? description = null, string? resourceType = null, bool isSafetyRelevant = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Permission code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Permission name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(actionName))
        {
            throw new ArgumentException("Permission action name must not be empty.", nameof(actionName));
        }

        return new Permission(id, permissionCategoryId, code, name, actionName, description, resourceType, isSafetyRelevant, createdAtUtc);
    }

    public void Deactivate() => IsActive = false;
}
