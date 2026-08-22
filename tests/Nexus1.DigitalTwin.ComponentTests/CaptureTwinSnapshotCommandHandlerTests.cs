using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class CaptureTwinSnapshotCommandHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static CaptureTwinSnapshotCommandHandler CreateHandler(DigitalTwinDbContext dbContext) => new(
        new EfRepository<TwinRuntimeSession, TwinRuntimeSessionId>(dbContext),
        new EfRepository<TwinSnapshot, TwinSnapshotId>(dbContext),
        new EfRepository<TwinSnapshotValue, TwinSnapshotValueId>(dbContext),
        UnitOfWork(dbContext), new SequentialIdGenerator());

    [Fact]
    public async Task Writes_a_snapshot_and_its_values_in_one_operation()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CaptureTwinSnapshotCommand(
                seed.TwinRuntimeSessionId, seed.SnapshotReasonId, NowUtc,
                [new TwinSnapshotValueRequest(seed.TwinVariableId, NumericValue: 101.2)]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var storedSnapshot = await verifyContext.TwinSnapshots.FindAsync(new TwinSnapshotId(result.Value));
        Assert.NotNull(storedSnapshot);

        var storedValues = verifyContext.TwinSnapshotValues.Where(v => v.TwinSnapshotId == storedSnapshot!.Id).ToList();
        var storedValue = Assert.Single(storedValues);
        Assert.Equal(101.2, storedValue.NumericValue);
    }

    [Fact]
    public async Task Fails_when_the_runtime_session_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CaptureTwinSnapshotCommand(
                999, seed.SnapshotReasonId, NowUtc, [new TwinSnapshotValueRequest(seed.TwinVariableId, NumericValue: 1)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    /// <summary>Drives TwinSnapshotValue.Create's CK_DigitalTwin_TwinSnapshotValue_OneValue invariant through the real command-handler path, not just the Domain layer directly.</summary>
    [Fact]
    public async Task Fails_when_a_value_row_has_no_numeric_text_or_json_value()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CaptureTwinSnapshotCommand(
                seed.TwinRuntimeSessionId, seed.SnapshotReasonId, NowUtc, [new TwinSnapshotValueRequest(seed.TwinVariableId)]),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
