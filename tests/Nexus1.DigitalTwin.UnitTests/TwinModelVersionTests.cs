using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinModelVersionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var version = TwinModelVersion.Create(
            new TwinModelVersionId(1), new TwinModelId(1), new SolverTypeId(1), new ValidationStatusId(1),
            "V1", "1.0.0", NowUtc);

        Assert.Equal("V1", version.VersionCode);
        Assert.Null(version.ApprovedByUserId);
    }

    [Fact]
    public void Create_with_approver_records_the_passport_id()
    {
        var version = TwinModelVersion.Create(
            new TwinModelVersionId(1), new TwinModelId(1), new SolverTypeId(1), new ValidationStatusId(1),
            "V1", "1.0.0", NowUtc, approvedByUserId: 42);

        Assert.Equal(42, version.ApprovedByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_version_code_throws(string versionCode)
    {
        Assert.Throws<ArgumentException>(() => TwinModelVersion.Create(
            new TwinModelVersionId(1), new TwinModelId(1), new SolverTypeId(1), new ValidationStatusId(1),
            versionCode, "1.0.0", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_model_version_throws(string modelVersion)
    {
        Assert.Throws<ArgumentException>(() => TwinModelVersion.Create(
            new TwinModelVersionId(1), new TwinModelId(1), new SolverTypeId(1), new ValidationStatusId(1),
            "V1", modelVersion, NowUtc));
    }
}
