using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class GetOpenSignalQualityEventsForUnitQueryHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Returns_only_open_events_with_bad_stale_or_uncertain_quality_ordered_newest_first()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var eventContext = CreateDbContext())
        {
            // Open, BAD quality — should appear.
            var openBad = SignalQualityEvent.Create(
                new SignalQualityEventId(1), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityBadId),
                NowUtc.AddHours(-2), NowUtc, reasonCode: "SENSOR_FAULT");

            // Open, BAD quality, started later — should sort first (StartedAtUtc desc).
            var openBadLater = SignalQualityEvent.Create(
                new SignalQualityEventId(2), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityBadId),
                NowUtc.AddHours(-1), NowUtc, reasonCode: "COMMS_LOSS");

            // Open, GOOD quality — should NOT appear (quality filter).
            var openGood = SignalQualityEvent.Create(
                new SignalQualityEventId(3), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityGoodId),
                NowUtc.AddHours(-3), NowUtc);

            // Closed, BAD quality — should NOT appear (EndedAtUtc filter).
            var closedBad = SignalQualityEvent.Create(
                new SignalQualityEventId(4), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityBadId),
                NowUtc.AddHours(-5), NowUtc);
            closedBad.Close(NowUtc.AddHours(-4));

            await eventContext.SignalQualityEvents.AddRangeAsync(openBad, openBadLater, openGood, closedBad);
            await eventContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetOpenSignalQualityEventsForUnitQueryHandler(new EfOpenSignalQualityEventFinder(dbContext));

        var result = await handler.Handle(
            new GetOpenSignalQualityEventsForUnitQuery(InstrumentationSeedHelper.UnitCode), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("COMMS_LOSS", result.Value[0].ReasonCode);
        Assert.Equal("SENSOR_FAULT", result.Value[1].ReasonCode);
        Assert.All(result.Value, e => Assert.Equal("BAD", e.QualityCode));
    }
}
