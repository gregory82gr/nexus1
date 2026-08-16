namespace Nexus1.Security.Domain;

/// <summary>
/// Grants or denies a permission through a role (atlas C.2.4.11).
/// Composite-keyed (ApplicationRoleId + PermissionId) — plain class, same
/// reasoning as UserRole.
/// </summary>
public sealed class RolePermission
{
    public RolePermission(
        ApplicationRoleId applicationRoleId, PermissionId permissionId, DateTime grantedAtUtc,
        bool isGranted = true, ApplicationUserId? grantedByUserId = null, DateTime? expiresAtUtc = null)
    {
        if (expiresAtUtc is not null && expiresAtUtc <= grantedAtUtc)
        {
            throw new ArgumentException("ExpiresAtUtc must be later than GrantedAtUtc when present.", nameof(expiresAtUtc));
        }

        ApplicationRoleId = applicationRoleId;
        PermissionId = permissionId;
        IsGranted = isGranted;
        GrantedAtUtc = grantedAtUtc;
        GrantedByUserId = grantedByUserId;
        ExpiresAtUtc = expiresAtUtc;
    }

    public ApplicationRoleId ApplicationRoleId { get; }

    public PermissionId PermissionId { get; }

    public bool IsGranted { get; private set; }

    public DateTime GrantedAtUtc { get; }

    public ApplicationUserId? GrantedByUserId { get; }

    public DateTime? ExpiresAtUtc { get; }

    public bool IsActiveAt(DateTime nowUtc) => IsGranted && (ExpiresAtUtc is null || ExpiresAtUtc > nowUtc);

    public void Revoke() => IsGranted = false;
}
