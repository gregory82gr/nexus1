using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class UserPreferenceTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_default_theme_succeeds()
    {
        var preference = UserPreference.Create(new ApplicationUserId(1), languageId: 1, timeZoneId: 1, NowUtc);

        Assert.Equal("System", preference.Theme);
        Assert.True(preference.ReceiveEmailAlerts);
    }

    [Fact]
    public void Create_with_invalid_theme_throws()
    {
        Assert.Throws<ArgumentException>(() => UserPreference.Create(
            new ApplicationUserId(1), languageId: 1, timeZoneId: 1, NowUtc, theme: "Purple"));
    }

    [Fact]
    public void Update_replaces_the_stored_preferences()
    {
        var preference = UserPreference.Create(new ApplicationUserId(1), languageId: 1, timeZoneId: 1, NowUtc);

        preference.Update(languageId: 2, timeZoneId: 3, theme: "Dark", receiveEmailAlerts: false, receiveInAppAlerts: true, dateFormat: "yyyy-MM-dd", numberFormat: null);

        Assert.Equal(2, preference.LanguageId);
        Assert.Equal(3, preference.TimeZoneId);
        Assert.Equal("Dark", preference.Theme);
        Assert.False(preference.ReceiveEmailAlerts);
    }

    [Fact]
    public void Update_with_invalid_theme_throws()
    {
        var preference = UserPreference.Create(new ApplicationUserId(1), languageId: 1, timeZoneId: 1, NowUtc);

        Assert.Throws<ArgumentException>(() => preference.Update(1, 1, "Purple", true, true, null, null));
    }
}
