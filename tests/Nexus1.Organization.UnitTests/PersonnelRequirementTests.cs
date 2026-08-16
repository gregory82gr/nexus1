using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class PersonnelRequirementTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var requirement = PersonnelRequirement.Create(
            new PersonnelRequirementId(1), new SiteId(1), new PositionId(1), minRequiredCount: 2, validFromUtc: NowUtc, createdAtUtc: NowUtc);

        Assert.Equal(2, requirement.MinRequiredCount);
        Assert.Null(requirement.ValidToUtc);
    }

    [Fact]
    public void Create_with_negative_min_required_count_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PersonnelRequirement.Create(
            new PersonnelRequirementId(1), new SiteId(1), new PositionId(1), minRequiredCount: -1, validFromUtc: NowUtc, createdAtUtc: NowUtc));
    }

    [Fact]
    public void Create_with_valid_to_before_valid_from_throws()
    {
        Assert.Throws<ArgumentException>(() => PersonnelRequirement.Create(
            new PersonnelRequirementId(1), new SiteId(1), new PositionId(1), minRequiredCount: 1, validFromUtc: NowUtc, createdAtUtc: NowUtc,
            validToUtc: NowUtc.AddDays(-1)));
    }

    [Fact]
    public void Create_with_valid_to_after_valid_from_succeeds()
    {
        var requirement = PersonnelRequirement.Create(
            new PersonnelRequirementId(1), new SiteId(1), new PositionId(1), minRequiredCount: 1, validFromUtc: NowUtc, createdAtUtc: NowUtc,
            validToUtc: NowUtc.AddYears(1));

        Assert.Equal(NowUtc.AddYears(1), requirement.ValidToUtc);
    }
}
