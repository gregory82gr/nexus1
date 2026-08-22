using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class AdvisorySessionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_defaults()
    {
        var session = AdvisorySession.Create(new AdvisorySessionId(1), new PolicyDeploymentId(1), unitId: 10, NowUtc);

        Assert.Equal(10, session.UnitId);
        Assert.Equal(NowUtc, session.StartedAtUtc);
        Assert.Null(session.EndedAtUtc);
        Assert.Null(session.StartedByUserId);
    }

    [Fact]
    public void Create_with_passport_only_started_by_user_id_sets_it_with_no_enforced_fk()
    {
        var session = AdvisorySession.Create(
            new AdvisorySessionId(1), new PolicyDeploymentId(1), unitId: 10, NowUtc, startedByUserId: 602);

        Assert.Equal(602, session.StartedByUserId);
    }
}
