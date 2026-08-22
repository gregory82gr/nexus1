using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class GetPolicyEntryCountQueryHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_policy_with_its_entry_count()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetPolicyEntryCountQueryHandler(new EfPolicyEntryCountFinder(dbContext));

        var result = await handler.Handle(new GetPolicyEntryCountQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var policy = Assert.Single(result.Value);
        Assert.Equal(ReinforcementLearningSeedHelper.PolicyCode, policy.PolicyCode);
        Assert.Equal(2, policy.PolicyEntryCount);
    }
}
