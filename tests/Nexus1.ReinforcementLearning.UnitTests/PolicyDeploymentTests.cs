using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class PolicyDeploymentTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_defaults()
    {
        var deployment = PolicyDeployment.Create(new PolicyDeploymentId(1), new PolicyId(1), new AdvisoryModeId(1), unitId: 10, NowUtc);

        Assert.Equal(10, deployment.UnitId);
        Assert.True(deployment.IsActive);
        Assert.Null(deployment.RetiredAtUtc);
        Assert.Null(deployment.DeployedByUserId);
    }

    [Fact]
    public void Create_with_passport_only_deployed_by_user_id_sets_it_with_no_enforced_fk()
    {
        var deployment = PolicyDeployment.Create(
            new PolicyDeploymentId(1), new PolicyId(1), new AdvisoryModeId(1), unitId: 10, NowUtc, deployedByUserId: 601);

        Assert.Equal(601, deployment.DeployedByUserId);
    }
}
