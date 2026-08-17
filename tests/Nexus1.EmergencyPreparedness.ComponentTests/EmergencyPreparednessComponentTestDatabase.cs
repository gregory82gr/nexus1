using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// EmergencyPreparedness has real cross-context SQL FOREIGN KEYs to
/// CorePlatform.EngineeringUnit and RadiationMonitoring.RadiationZone
/// (ADR-025's own whole-sector FK audit). RadiationMonitoring itself has a
/// real FK to ReactorFleet.Unit (ADR-024), so this fixture migrates
/// ReactorFleet, then CorePlatform, then RadiationMonitoring, then
/// EmergencyPreparednessDbContext, against the SAME connection string —
/// mirroring RadiationMonitoring's own three-context precedent extended by
/// one context, and matching Nexus1.ModularRuntime's own Program.cs
/// composition order.
/// </summary>
public abstract class EmergencyPreparednessComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=EmergencyPreparednessComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var radiationMonitoringContext = CreateRadiationMonitoringDbContext();
        await radiationMonitoringContext.Database.MigrateAsync();

        await using var emergencyPreparednessContext = CreateDbContext();
        await emergencyPreparednessContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected EmergencyPreparednessDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<EmergencyPreparednessDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected RadiationMonitoringDbContext CreateRadiationMonitoringDbContext() =>
        new(new DbContextOptionsBuilder<RadiationMonitoringDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(EmergencyPreparednessDbContext dbContext) => new EfUnitOfWork(dbContext);
}
