using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// RadiationMonitoring has real cross-context SQL FOREIGN KEYs to
/// ReactorFleet.Unit and CorePlatform.EngineeringUnit (ADR-024's own
/// whole-sector FK audit — the sector's only two external dependencies), so
/// this fixture migrates both contexts' own DbContexts first, then
/// RadiationMonitoringDbContext, against the SAME connection string —
/// mirroring DigitalTwinComponentTestDatabase's own three-context precedent
/// (minus Instrumentation, which this sector does not reference).
/// </summary>
public abstract class RadiationMonitoringComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=RadiationMonitoringComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var radiationMonitoringContext = CreateDbContext();
        await radiationMonitoringContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected RadiationMonitoringDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<RadiationMonitoringDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(RadiationMonitoringDbContext dbContext) => new EfUnitOfWork(dbContext);
}
