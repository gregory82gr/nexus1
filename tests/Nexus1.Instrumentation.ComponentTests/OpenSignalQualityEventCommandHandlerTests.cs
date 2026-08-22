using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

public sealed class OpenSignalQualityEventCommandHandlerTests : InstrumentationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static OpenSignalQualityEventCommandHandler CreateHandler(InstrumentationDbContext dbContext) => new(
        new EfRepository<Signal, SignalId>(dbContext), new EfRepository<SignalQualityEvent, SignalQualityEventId>(dbContext),
        UnitOfWork(dbContext), new FixedDateTimeProvider(NowUtc), new SequentialIdGenerator());

    [Fact]
    public async Task Opens_a_quality_event_with_no_end_date()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new OpenSignalQualityEventCommand(seed.SignalId, seed.SignalQualityBadId, NowUtc, ReasonCode: "SENSOR_FAULT"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var stored = await verifyContext.SignalQualityEvents.FindAsync(new SignalQualityEventId(result.Value));
        Assert.NotNull(stored);
        Assert.Null(stored!.EndedAtUtc);
        Assert.Equal("SENSOR_FAULT", stored.ReasonCode);
    }

    [Fact]
    public async Task Fails_when_the_signal_does_not_exist()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await InstrumentationSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new OpenSignalQualityEventCommand(999, seed.SignalQualityBadId, NowUtc), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
