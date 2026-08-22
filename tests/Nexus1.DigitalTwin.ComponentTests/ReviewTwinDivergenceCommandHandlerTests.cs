using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class ReviewTwinDivergenceCommandHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static ReviewTwinDivergenceCommandHandler CreateHandler(DigitalTwinDbContext dbContext) => new(
        new EfRepository<TwinDivergence, TwinDivergenceId>(dbContext), new EfRepository<TwinDivergenceReview, TwinDivergenceReviewId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    private static async Task<long> SeedDivergenceAsync(DigitalTwinDbContext dbContext, DigitalTwinSeedHelper.SeedResult seed)
    {
        var snapshot = TwinSnapshot.Create(new TwinSnapshotId(1), new TwinRuntimeSessionId(seed.TwinRuntimeSessionId), new SnapshotReasonId(seed.SnapshotReasonId), NowUtc);
        await dbContext.TwinSnapshots.AddAsync(snapshot);
        await dbContext.SaveChangesAsync();

        var divergence = TwinDivergence.Create(
            new TwinDivergenceId(1), snapshot.Id, seed.SignalId, new DivergenceSeverityId(seed.DivergenceSeverityId),
            new DivergenceStatusId(seed.DivergenceStatusOpenId), NowUtc, 100.0, 105.0, new TwinVariableId(seed.TwinVariableId));
        await dbContext.TwinDivergences.AddAsync(divergence);
        await dbContext.SaveChangesAsync();

        return divergence.Id.Value;
    }

    [Fact]
    public async Task Records_a_review_disposition_for_an_existing_divergence()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var setupContext = CreateDbContext();
        var divergenceId = await SeedDivergenceAsync(setupContext, seed);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ReviewTwinDivergenceCommand(
                divergenceId, seed.DivergenceStatusClosedId, NowUtc, ReviewedByUserId: 7,
                ReviewNote: "Sensor drift confirmed.", CorrectiveAction: "Recalibrate."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.TwinDivergenceReviews.FindAsync(new TwinDivergenceReviewId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal("Sensor drift confirmed.", stored!.ReviewNote);
        Assert.Equal(7, stored.ReviewedByUserId);
    }

    [Fact]
    public async Task Fails_when_the_divergence_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ReviewTwinDivergenceCommand(999, seed.DivergenceStatusClosedId, NowUtc), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
