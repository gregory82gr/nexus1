using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class RolePermissionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_defaults_to_granted()
    {
        var rolePermission = new RolePermission(new ApplicationRoleId(1), new PermissionId(1), NowUtc);

        Assert.True(rolePermission.IsActiveAt(NowUtc));
    }

    [Fact]
    public void Create_with_expiry_before_granted_at_throws()
    {
        Assert.Throws<ArgumentException>(() => new RolePermission(
            new ApplicationRoleId(1), new PermissionId(1), NowUtc, expiresAtUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void Revoke_makes_IsActiveAt_false()
    {
        var rolePermission = new RolePermission(new ApplicationRoleId(1), new PermissionId(1), NowUtc);

        rolePermission.Revoke();

        Assert.False(rolePermission.IsActiveAt(NowUtc));
    }

    [Fact]
    public void An_explicit_deny_grant_is_never_active()
    {
        var rolePermission = new RolePermission(new ApplicationRoleId(1), new PermissionId(1), NowUtc, isGranted: false);

        Assert.False(rolePermission.IsActiveAt(NowUtc));
    }
}
