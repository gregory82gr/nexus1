using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class ApplicationRoleTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_defaults_to_active()
    {
        var role = ApplicationRole.Create(new ApplicationRoleId(1), new RoleTypeId(1), "Operator", "OPERATOR", NowUtc);

        Assert.True(role.IsActive);
        Assert.Null(role.ParentRoleId);
    }

    [Fact]
    public void Create_a_child_role_with_a_parent_succeeds()
    {
        var role = ApplicationRole.Create(
            new ApplicationRoleId(2), new RoleTypeId(1), "Shift Operator", "SHIFT_OPERATOR", NowUtc,
            parentRoleId: new ApplicationRoleId(1));

        Assert.Equal(new ApplicationRoleId(1), role.ParentRoleId);
    }

    [Fact]
    public void Create_with_itself_as_parent_throws()
    {
        Assert.Throws<ArgumentException>(() => ApplicationRole.Create(
            new ApplicationRoleId(1), new RoleTypeId(1), "Operator", "OPERATOR", NowUtc,
            parentRoleId: new ApplicationRoleId(1)));
    }

    [Fact]
    public void Deactivate_then_Activate_toggles_IsActive()
    {
        var role = ApplicationRole.Create(new ApplicationRoleId(1), new RoleTypeId(1), "Operator", "OPERATOR", NowUtc);

        role.Deactivate();
        Assert.False(role.IsActive);

        role.Activate();
        Assert.True(role.IsActive);
    }
}
