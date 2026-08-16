using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

/// <summary>
/// The sector's most distinctive real behavior (ADR-017): GapCount is
/// computed by the domain factory itself, never accepted as a
/// caller-supplied value, mirroring the atlas's own SQL computed column
/// exactly (RequiredCount &gt; AvailableCount ? RequiredCount - AvailableCount : 0).
/// </summary>
public class StaffingScenarioGapTests
{
    [Fact]
    public void Create_when_required_exceeds_available_computes_positive_gap()
    {
        var gap = StaffingScenarioGap.Create(
            new StaffingScenarioGapId(1), new StaffingScenarioResultId(1), new PositionId(1),
            requiredCount: 5, availableCount: 3);

        Assert.Equal(2, gap.GapCount);
    }

    [Fact]
    public void Create_when_available_meets_or_exceeds_required_computes_zero_gap()
    {
        var gapExact = StaffingScenarioGap.Create(
            new StaffingScenarioGapId(1), new StaffingScenarioResultId(1), new PositionId(1),
            requiredCount: 4, availableCount: 4);

        var gapSurplus = StaffingScenarioGap.Create(
            new StaffingScenarioGapId(2), new StaffingScenarioResultId(1), new PositionId(1),
            requiredCount: 4, availableCount: 6);

        Assert.Equal(0, gapExact.GapCount);
        Assert.Equal(0, gapSurplus.GapCount);
    }

    [Fact]
    public void Create_with_negative_required_count_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StaffingScenarioGap.Create(
            new StaffingScenarioGapId(1), new StaffingScenarioResultId(1), new PositionId(1),
            requiredCount: -1, availableCount: 0));
    }

    [Fact]
    public void Create_with_negative_available_count_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StaffingScenarioGap.Create(
            new StaffingScenarioGapId(1), new StaffingScenarioResultId(1), new PositionId(1),
            requiredCount: 0, availableCount: -1));
    }
}
