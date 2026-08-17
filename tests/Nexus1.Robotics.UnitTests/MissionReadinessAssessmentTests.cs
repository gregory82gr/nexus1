using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.UnitTests;

public class MissionReadinessAssessmentTests
{
    private static readonly DateTime AssessedAtUtc = new(2026, 8, 17, 6, 45, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var assessment = MissionReadinessAssessment.Create(
            new MissionReadinessAssessmentId(1), missionId: 1, new ReadinessStatusId(1), AssessedAtUtc);

        Assert.Equal(new MissionId(1), assessment.MissionId);
        Assert.Equal(AssessedAtUtc, assessment.AssessedAtUtc);
        Assert.Null(assessment.AssessedByUserId);
        Assert.Null(assessment.Summary);
    }

    [Fact]
    public void Create_with_passport_only_assessed_by_user_id_sets_it_with_no_enforced_fk()
    {
        var assessment = MissionReadinessAssessment.Create(
            new MissionReadinessAssessmentId(1), 1, new ReadinessStatusId(1), AssessedAtUtc,
            assessedByUserId: 7, summary: "Battery below threshold");

        Assert.Equal(7, assessment.AssessedByUserId);
        Assert.Equal("Battery below threshold", assessment.Summary);
    }
}
