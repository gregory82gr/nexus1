using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class ExtractPolicyCommandHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    private static ExtractPolicyCommandHandler CreateHandler(ReinforcementLearningDbContext dbContext) => new(
        new EfRepository<Policy, PolicyId>(dbContext), UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Extracts_a_new_policy_from_the_seeded_final_q_table()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ExtractPolicyCommand(seed.QTableId, seed.PolicyStatusId, "POL-002", "Extracted Policy v2", NowUtc, 2),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.Policies.FindAsync(new PolicyId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("POL-002", stored!.Code);
        Assert.Equal(2, stored.EntryCount);
    }
}
