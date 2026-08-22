using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

public sealed class GetOpenDivergencesQueryHandlerTests : DigitalTwinComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_open_divergences_ordered_by_detected_at_descending_and_excludes_closed()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var instrumentationContext = CreateInstrumentationDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await DigitalTwinSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, instrumentationContext, seedContext, NowUtc);

        await using var setupContext = CreateDbContext();
        var snapshot = TwinSnapshot.Create(
            new TwinSnapshotId(1), new TwinRuntimeSessionId(seed.TwinRuntimeSessionId), new SnapshotReasonId(seed.SnapshotReasonId), NowUtc);
        await setupContext.TwinSnapshots.AddAsync(snapshot);
        await setupContext.SaveChangesAsync();

        var earlierOpen = TwinDivergence.Create(
            new TwinDivergenceId(1), snapshot.Id, seed.SignalId, new DivergenceSeverityId(seed.DivergenceSeverityId),
            new DivergenceStatusId(seed.DivergenceStatusOpenId), NowUtc.AddHours(-2), 100.0, 103.0, new TwinVariableId(seed.TwinVariableId));
        var laterOpen = TwinDivergence.Create(
            new TwinDivergenceId(2), snapshot.Id, seed.SignalId, new DivergenceSeverityId(seed.DivergenceSeverityId),
            new DivergenceStatusId(seed.DivergenceStatusOpenId), NowUtc.AddHours(-1), 100.0, 90.0, new TwinVariableId(seed.TwinVariableId));
        var closed = TwinDivergence.Create(
            new TwinDivergenceId(3), snapshot.Id, seed.SignalId, new DivergenceSeverityId(seed.DivergenceSeverityId),
            new DivergenceStatusId(seed.DivergenceStatusClosedId), NowUtc, 100.0, 150.0, new TwinVariableId(seed.TwinVariableId));

        await setupContext.TwinDivergences.AddRangeAsync(earlierOpen, laterOpen, closed);
        await setupContext.SaveChangesAsync();

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenDivergencesQueryHandler(new EfOpenDivergenceFinder(dbContext));

        var result = await handler.Handle(new GetOpenDivergencesQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, d => Assert.Equal("OPEN", d.Status));
        Assert.DoesNotContain(result.Value, d => Math.Abs(d.MeasuredValue - 150.0) < 0.001);

        // Ordered by DetectedAtUtc descending (atlas C.6.8 query 3).
        Assert.Equal(-10.0, result.Value[0].DeltaValue, precision: 10); // laterOpen: 90 - 100
        Assert.Equal(3.0, result.Value[1].DeltaValue, precision: 10); // earlierOpen: 103 - 100
    }
}
