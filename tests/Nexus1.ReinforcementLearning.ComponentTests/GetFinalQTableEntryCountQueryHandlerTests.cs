using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

public sealed class GetFinalQTableEntryCountQueryHandlerTests : ReinforcementLearningComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_the_seeded_final_q_table_with_its_entry_count()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await using var seedContext = CreateDbContext();
        await ReinforcementLearningSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, digitalTwinContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var handler = new GetFinalQTableEntryCountQueryHandler(new EfFinalQTableEntryCountFinder(dbContext));

        var result = await handler.Handle(new GetFinalQTableEntryCountQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var qTable = Assert.Single(result.Value);
        Assert.Equal(ReinforcementLearningSeedHelper.QTableCode, qTable.QTableCode);
        Assert.Equal(4, qTable.QValueCount);
    }
}
