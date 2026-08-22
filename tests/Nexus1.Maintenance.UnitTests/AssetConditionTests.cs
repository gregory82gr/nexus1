using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

/// <summary>CK_Maintenance_AssetCondition_HealthScore: HealthScorePercent must be in [0, 100] when set, both boundary values accepted, values outside rejected.</summary>
public class AssetConditionTests
{
    private static readonly DateTime AssessedAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_null_health_score_succeeds()
    {
        var condition = AssetCondition.Create(new AssetConditionId(1), new AssetId(1), new ConditionGradeId(1), AssessedAtUtc);
        Assert.Null(condition.HealthScorePercent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(50.5)]
    public void Create_with_health_score_in_range_succeeds(decimal healthScorePercent)
    {
        var condition = AssetCondition.Create(
            new AssetConditionId(1), new AssetId(1), new ConditionGradeId(1), AssessedAtUtc, healthScorePercent: healthScorePercent);

        Assert.Equal(healthScorePercent, condition.HealthScorePercent);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(-50)]
    [InlineData(150)]
    public void Create_with_health_score_out_of_range_throws(decimal healthScorePercent)
    {
        Assert.Throws<ArgumentException>(() => AssetCondition.Create(
            new AssetConditionId(1), new AssetId(1), new ConditionGradeId(1), AssessedAtUtc, healthScorePercent: healthScorePercent));
    }
}
