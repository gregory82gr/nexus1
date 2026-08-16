using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class PermissionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var permission = Permission.Create(
            new PermissionId(1), new PermissionCategoryId(1), "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc,
            isSafetyRelevant: true);

        Assert.Equal("alarm.acknowledge", permission.Code);
        Assert.True(permission.IsSafetyRelevant);
        Assert.True(permission.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Permission.Create(
            new PermissionId(1), new PermissionCategoryId(1), code, "Acknowledge Alarm", "Acknowledge", NowUtc));
    }

    [Fact]
    public void Deactivate_sets_IsActive_false()
    {
        var permission = Permission.Create(
            new PermissionId(1), new PermissionCategoryId(1), "alarm.acknowledge", "Acknowledge Alarm", "Acknowledge", NowUtc);

        permission.Deactivate();

        Assert.False(permission.IsActive);
    }
}
