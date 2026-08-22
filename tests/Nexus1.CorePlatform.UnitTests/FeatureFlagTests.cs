using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class FeatureFlagTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_defaults_to_disabled()
    {
        var flag = FeatureFlag.Create(new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc);

        Assert.False(flag.IsEnabled);
        Assert.Equal("All", flag.EnvironmentName);
    }

    [Fact]
    public void Create_with_expiry_before_enabled_from_throws()
    {
        Assert.Throws<ArgumentException>(() => FeatureFlag.Create(
            new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc,
            enabledFromUtc: NowUtc, expiresAtUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void Enable_sets_IsEnabled_and_EnabledFromUtc()
    {
        var flag = FeatureFlag.Create(new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc);

        flag.Enable(NowUtc);

        Assert.True(flag.IsEnabled);
        Assert.Equal(NowUtc, flag.EnabledFromUtc);
    }

    [Fact]
    public void Disable_sets_IsEnabled_false()
    {
        var flag = FeatureFlag.Create(new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc, isEnabled: true);

        flag.Disable();

        Assert.False(flag.IsEnabled);
    }

    [Fact]
    public void IsActiveAt_is_false_before_enabled_from()
    {
        var flag = FeatureFlag.Create(new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc);
        flag.Enable(NowUtc.AddHours(1));

        Assert.False(flag.IsActiveAt(NowUtc));
        Assert.True(flag.IsActiveAt(NowUtc.AddHours(2)));
    }

    [Fact]
    public void IsActiveAt_is_false_after_expiry()
    {
        var flag = FeatureFlag.Create(
            new FeatureFlagId(1), "new-atlas-ui", "New Atlas UI", NowUtc,
            isEnabled: true, enabledFromUtc: NowUtc, expiresAtUtc: NowUtc.AddDays(1));

        Assert.True(flag.IsActiveAt(NowUtc.AddHours(1)));
        Assert.False(flag.IsActiveAt(NowUtc.AddDays(2)));
    }
}
