using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class AppSettingTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_key_and_value_succeeds()
    {
        var setting = AppSetting.Create(new AppSettingId(1), "flood.threshold", "3", AppSettingValueType.Int, NowUtc);

        Assert.Equal("flood.threshold", setting.Key);
        Assert.Equal("3", setting.Value);
        Assert.Equal(AppSettingValueType.Int, setting.ValueType);
        Assert.False(setting.IsEncrypted);
        Assert.False(setting.IsSystem);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_key_throws(string key)
    {
        Assert.Throws<ArgumentException>(() => AppSetting.Create(new AppSettingId(1), key, "3", AppSettingValueType.Int, NowUtc));
    }

    [Fact]
    public void Create_with_key_over_200_characters_throws()
    {
        var key = new string('k', 201);
        Assert.Throws<ArgumentException>(() => AppSetting.Create(new AppSettingId(1), key, "3", AppSettingValueType.Int, NowUtc));
    }

    [Fact]
    public void UpdateValue_replaces_the_stored_value()
    {
        var setting = AppSetting.Create(new AppSettingId(1), "flood.threshold", "3", AppSettingValueType.Int, NowUtc);

        setting.UpdateValue("5");

        Assert.Equal("5", setting.Value);
    }

    [Fact]
    public void UpdateValue_over_2000_characters_throws()
    {
        var setting = AppSetting.Create(new AppSettingId(1), "flood.threshold", "3", AppSettingValueType.Int, NowUtc);

        Assert.Throws<ArgumentException>(() => setting.UpdateValue(new string('v', 2001)));
    }
}
