using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class CloseSignalQualityEventCommandHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static CloseSignalQualityEventCommandHandler CreateHandler(InstrumentationDbContext dbContext) => new(
        new EfRepository<SignalQualityEvent, SignalQualityEventId>(dbContext), UnitOfWork(dbContext));

    [Fact]
    public async Task Closes_an_open_quality_event()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var openContext = CreateDbContext())
        {
            var qualityEvent = SignalQualityEvent.Create(
                new SignalQualityEventId(1), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityBadId),
                NowUtc.AddHours(-2), NowUtc, reasonCode: "SENSOR_FAULT");
            await openContext.SignalQualityEvents.AddAsync(qualityEvent);
            await openContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CloseSignalQualityEventCommand(1, NowUtc, ReasonCode: "REPAIRED"), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.SignalQualityEvents.FindAsync(new SignalQualityEventId(1));
        Assert.NotNull(stored);
        Assert.Equal(NowUtc, stored!.EndedAtUtc);
        Assert.Equal("REPAIRED", stored.ReasonCode);
    }

    [Fact]
    public async Task Fails_when_the_event_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CloseSignalQualityEventCommand(999, NowUtc), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Fails_when_end_date_is_before_start_date()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var openContext = CreateDbContext())
        {
            var qualityEvent = SignalQualityEvent.Create(
                new SignalQualityEventId(1), new SignalId(seed.SignalId), new SignalQualityId(seed.SignalQualityBadId),
                NowUtc, NowUtc);
            await openContext.SignalQualityEvents.AddAsync(qualityEvent);
            await openContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new CloseSignalQualityEventCommand(1, NowUtc.AddHours(-1)), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
