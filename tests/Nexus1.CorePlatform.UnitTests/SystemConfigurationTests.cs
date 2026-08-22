using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class SystemConfigurationTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_json_succeeds()
    {
        var config = SystemConfiguration.Create(
            new SystemConfigurationId(1), "AlarmManagement", "flood-detector", "{\"threshold\":3}", 1, NowUtc, NowUtc);

        Assert.True(config.IsActive);
        Assert.Equal(1, config.SchemaVersion);
    }

    [Fact]
    public void Create_with_malformed_json_throws()
    {
        Assert.Throws<ArgumentException>(() => SystemConfiguration.Create(
            new SystemConfigurationId(1), "AlarmManagement", "flood-detector", "{not json", 1, NowUtc, NowUtc));
    }

    [Fact]
    public void Create_with_zero_or_negative_schema_version_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SystemConfiguration.Create(
            new SystemConfigurationId(1), "AlarmManagement", "flood-detector", "{}", 0, NowUtc, NowUtc));
    }

    [Fact]
    public void Create_with_effective_to_before_effective_from_throws()
    {
        Assert.Throws<ArgumentException>(() => SystemConfiguration.Create(
            new SystemConfigurationId(1), "AlarmManagement", "flood-detector", "{}", 1, NowUtc, NowUtc,
            effectiveToUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void Deactivate_sets_IsActive_false()
    {
        var config = SystemConfiguration.Create(
            new SystemConfigurationId(1), "AlarmManagement", "flood-detector", "{}", 1, NowUtc, NowUtc);

        config.Deactivate();

        Assert.False(config.IsActive);
    }
}
