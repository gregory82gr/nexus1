using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.UnitTests;

public class DeploymentVersionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var version = DeploymentVersion.Create(
            new DeploymentVersionId(1), "Nexus1.ModularRuntime", DeploymentComponentType.ApiService, "2.1.0", NowUtc);

        Assert.Equal("Nexus1.ModularRuntime", version.ComponentName);
        Assert.False(version.IsCurrent);
    }

    [Fact]
    public void Create_with_git_commit_not_40_characters_throws()
    {
        Assert.Throws<ArgumentException>(() => DeploymentVersion.Create(
            new DeploymentVersionId(1), "Nexus1.ModularRuntime", DeploymentComponentType.ApiService, "2.1.0", NowUtc,
            gitCommit: "abc123"));
    }

    [Fact]
    public void MarkCurrent_then_MarkNotCurrent_toggles_IsCurrent()
    {
        var version = DeploymentVersion.Create(
            new DeploymentVersionId(1), "Nexus1.ModularRuntime", DeploymentComponentType.ApiService, "2.1.0", NowUtc);

        version.MarkCurrent();
        Assert.True(version.IsCurrent);

        version.MarkNotCurrent();
        Assert.False(version.IsCurrent);
    }
}
