using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class StaffingScenarioResultTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("Pass")]
    [InlineData("Warning")]
    [InlineData("Fail")]
    [InlineData("NotEvaluated")]
    public void Create_with_valid_overall_status_succeeds(string status)
    {
        var result = StaffingScenarioResult.Create(new StaffingScenarioResultId(1), new StaffingScenarioId(1), NowUtc, status);
        Assert.Equal(status, result.OverallStatus);
    }

    [Fact]
    public void Create_with_invalid_overall_status_throws()
    {
        Assert.Throws<ArgumentException>(() => StaffingScenarioResult.Create(
            new StaffingScenarioResultId(1), new StaffingScenarioId(1), NowUtc, "Unknown"));
    }

    [Fact]
    public void Create_with_evaluated_by_passport_id_succeeds()
    {
        var result = StaffingScenarioResult.Create(
            new StaffingScenarioResultId(1), new StaffingScenarioId(1), NowUtc, "Pass", evaluatedByUserId: 3);

        Assert.Equal(3, result.EvaluatedByUserId);
    }
}
