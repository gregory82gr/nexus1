using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class StaffingScenarioTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var scenario = StaffingScenario.Create(new StaffingScenarioId(1), new SiteId(1), "OUTAGE-1", "Outage Scenario 1", NowUtc);

        Assert.Equal("OUTAGE-1", scenario.ScenarioCode);
        Assert.Null(scenario.CreatedByUserId);
    }

    [Fact]
    public void Create_with_created_by_passport_id_succeeds()
    {
        var scenario = StaffingScenario.Create(
            new StaffingScenarioId(1), new SiteId(1), "OUTAGE-1", "Outage Scenario 1", NowUtc, createdByUserId: 9);

        Assert.Equal(9, scenario.CreatedByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_scenario_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => StaffingScenario.Create(new StaffingScenarioId(1), new SiteId(1), code, "Name", NowUtc));
    }
}
