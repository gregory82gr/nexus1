using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class UserRoleTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_without_expiry_is_active_indefinitely()
    {
        var userRole = new UserRole(new ApplicationUserId(1), new ApplicationRoleId(1), NowUtc);

        Assert.True(userRole.IsActiveAt(NowUtc.AddYears(10)));
    }

    [Fact]
    public void Create_with_expiry_before_assigned_at_throws()
    {
        Assert.Throws<ArgumentException>(() => new UserRole(
            new ApplicationUserId(1), new ApplicationRoleId(1), NowUtc, expiresAtUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void IsActiveAt_is_false_after_expiry()
    {
        var userRole = new UserRole(new ApplicationUserId(1), new ApplicationRoleId(1), NowUtc, expiresAtUtc: NowUtc.AddDays(1));

        Assert.True(userRole.IsActiveAt(NowUtc.AddHours(1)));
        Assert.False(userRole.IsActiveAt(NowUtc.AddDays(2)));
    }

    [Fact]
    public void Revoke_makes_IsActiveAt_false_even_before_expiry()
    {
        var userRole = new UserRole(new ApplicationUserId(1), new ApplicationRoleId(1), NowUtc);

        userRole.Revoke();

        Assert.False(userRole.IsActiveAt(NowUtc));
    }
}
