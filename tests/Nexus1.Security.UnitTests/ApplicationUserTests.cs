using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class ApplicationUserTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var user = ApplicationUser.Create(
            new ApplicationUserId(1), new UserStatusId(1), "operator1", "OPERATOR1", "Operator One", NowUtc);

        Assert.Equal("operator1", user.UserName);
        Assert.False(user.IsServiceAccount);
        Assert.False(user.IsLockedOut(NowUtc));
    }

    [Fact]
    public void Create_a_service_account_sets_IsServiceAccount()
    {
        var user = ApplicationUser.Create(
            new ApplicationUserId(1), new UserStatusId(1), "svc-alarm-flood-consumer", "SVC-ALARM-FLOOD-CONSUMER",
            "AlarmManagement flood consumer", NowUtc, isServiceAccount: true);

        Assert.True(user.IsServiceAccount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_username_throws(string userName)
    {
        Assert.Throws<ArgumentException>(() => ApplicationUser.Create(
            new ApplicationUserId(1), new UserStatusId(1), userName, "X", "Display Name", NowUtc));
    }

    [Fact]
    public void Lock_makes_IsLockedOut_true_until_the_lockout_end()
    {
        var user = ApplicationUser.Create(new ApplicationUserId(1), new UserStatusId(1), "operator1", "OPERATOR1", "Operator One", NowUtc);

        user.Lock(NowUtc.AddMinutes(15));

        Assert.True(user.IsLockedOut(NowUtc.AddMinutes(5)));
        Assert.False(user.IsLockedOut(NowUtc.AddMinutes(20)));
    }

    [Fact]
    public void Unlock_clears_the_lockout_and_resets_access_failed_count()
    {
        var user = ApplicationUser.Create(new ApplicationUserId(1), new UserStatusId(1), "operator1", "OPERATOR1", "Operator One", NowUtc);
        user.Lock(NowUtc.AddDays(1));

        user.Unlock();

        Assert.False(user.IsLockedOut(NowUtc));
        Assert.Equal(0, user.AccessFailedCount);
    }
}
