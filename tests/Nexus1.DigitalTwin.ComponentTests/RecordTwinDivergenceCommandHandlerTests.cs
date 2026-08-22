using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class RecordTwinDivergenceCommandHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static RecordTwinDivergenceCommandHandler CreateHandler(DigitalTwinDbContext dbContext) => new(
        new EfRepository<TwinSnapshot, TwinSnapshotId>(dbContext), new EfRepository<TwinDivergence, TwinDivergenceId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    private static async Task<long> SeedSnapshotAsync(DigitalTwinDbContext dbContext, DigitalTwinSeedHelper.SeedResult seed)
    {
        var snapshot = TwinSnapshot.Create(new TwinSnapshotId(1), new TwinRuntimeSessionId(seed.TwinRuntimeSessionId), new SnapshotReasonId(seed.SnapshotReasonId), NowUtc);
        await dbContext.TwinSnapshots.AddAsync(snapshot);
        await dbContext.SaveChangesAsync();
        return snapshot.Id.Value;
    }

    /// <summary>
    /// The sector's own "conscience table" real invariant, proven through a
    /// real round trip through the database (ADR-020, CLAUDE.md's
    /// real-runner-path testing convention): DeltaValue always equals
    /// MeasuredValue - ModeledValue, and there is no way to pass a mismatched
    /// value in.
    /// </summary>
    [Fact]
    public async Task Persisted_delta_value_equals_measured_minus_modeled_after_a_real_round_trip()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var setupContext = CreateDbContext();
        var snapshotId = await SeedSnapshotAsync(setupContext, seed);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordTwinDivergenceCommand(
                snapshotId, seed.SignalId, seed.DivergenceSeverityId, seed.DivergenceStatusOpenId, NowUtc,
                ModeledValue: 100.0, MeasuredValue: 108.25, TwinVariableId: seed.TwinVariableId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.TwinDivergences.FindAsync(new TwinDivergenceId(result.Value));
        Assert.NotNull(stored);
        Assert.Equal(100.0, stored!.ModeledValue);
        Assert.Equal(108.25, stored.MeasuredValue);
        Assert.Equal(8.25, stored.DeltaValue, precision: 10);
        Assert.Equal(stored.MeasuredValue - stored.ModeledValue, stored.DeltaValue, precision: 10);
    }

    [Fact]
    public async Task Fails_when_the_snapshot_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new RecordTwinDivergenceCommand(999, seed.SignalId, seed.DivergenceSeverityId, seed.DivergenceStatusOpenId, NowUtc, 100.0, 100.0),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
