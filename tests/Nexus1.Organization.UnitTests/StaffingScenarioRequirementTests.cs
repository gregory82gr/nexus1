using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class StaffingScenarioRequirementTests
{
    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var requirement = StaffingScenarioRequirement.Create(
            new StaffingScenarioRequirementId(1), new StaffingScenarioId(1), new PositionId(1), requiredCount: 3);

        Assert.Equal(3, requirement.RequiredCount);
    }

    [Fact]
    public void Create_with_negative_required_count_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StaffingScenarioRequirement.Create(
            new StaffingScenarioRequirementId(1), new StaffingScenarioId(1), new PositionId(1), requiredCount: -1));
    }

    [Fact]
    public void Create_with_required_qualification_succeeds()
    {
        var requirement = StaffingScenarioRequirement.Create(
            new StaffingScenarioRequirementId(1), new StaffingScenarioId(1), new PositionId(1), requiredCount: 1,
            requiredQualificationId: new QualificationId(4));

        Assert.Equal(new QualificationId(4), requirement.RequiredQualificationId);
    }
}
