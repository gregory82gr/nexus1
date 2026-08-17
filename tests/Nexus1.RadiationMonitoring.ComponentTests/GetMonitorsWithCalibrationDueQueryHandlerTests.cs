using Nexus1.BuildingBlocks.Application;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>Matches the atlas's own C.13.5.2 query 2, verbatim: monitors whose calibration is due.</summary>
public sealed class GetMonitorsWithCalibrationDueQueryHandlerTests : RadiationMonitoringComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow => utcNow;
    }

    [Fact]
    public async Task Returns_only_monitors_whose_calibration_due_date_has_passed()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await using var corePlatformContext = CreateCorePlatformDbContext();
        await using var seedContext = CreateDbContext();
        var seed = await RadiationMonitoringSeedHelper.SeedCoreAsync(reactorFleetContext, corePlatformContext, seedContext, NowUtc);

        await using (var monitorSeedContext = CreateDbContext())
        {
            var overdueMonitor = RadiationMonitor.Create(
                new RadiationMonitorId(2), new MonitorTypeId(seed.MonitorTypeId),
                new MonitorStatusId(seed.MonitorStatusId), "RM-002", "Area Monitor 2",
                calibrationDueAtUtc: NowUtc.AddDays(-1));
            var notYetDueMonitor = RadiationMonitor.Create(
                new RadiationMonitorId(3), new MonitorTypeId(seed.MonitorTypeId),
                new MonitorStatusId(seed.MonitorStatusId), "RM-003", "Area Monitor 3",
                calibrationDueAtUtc: NowUtc.AddDays(30));

            await monitorSeedContext.RadiationMonitors.AddAsync(overdueMonitor);
            await monitorSeedContext.RadiationMonitors.AddAsync(notYetDueMonitor);
            await monitorSeedContext.SaveChangesAsync();
        }

        await using var dbContext = CreateDbContext();
        var handler = new GetMonitorsWithCalibrationDueQueryHandler(
            new EfMonitorsWithCalibrationDueFinder(dbContext, new FixedDateTimeProvider(NowUtc)));

        var result = await handler.Handle(new GetMonitorsWithCalibrationDueQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var due = Assert.Single(result.Value);
        Assert.Equal("RM-002", due.Code);
        Assert.Equal("AREA_GAMMA", due.MonitorType);
    }
}
