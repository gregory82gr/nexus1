using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.EventManagement.Infrastructure.Persistence;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.Maintenance.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.Maintenance.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// Maintenance has real cross-context SQL FOREIGN KEYs to ReactorFleet.Unit,
/// CorePlatform.EngineeringUnit, Instrumentation.Signal, and — as of
/// ADR-022's reconnection — EventManagement.OperationalEvent/IncidentAction,
/// so this fixture migrates all six contexts' own DbContexts against the
/// SAME connection string, in dependency order (ReactorFleet, AlarmManagement,
/// CorePlatform, Instrumentation and EventManagement first, so their tables
/// physically exist before Maintenance's migrations add FK constraints
/// against them). AlarmManagement is included here even though Maintenance
/// itself has no direct FK into it — EventManagement's own migration has
/// real FKs to AlarmManagement.AlarmEvent/AlarmFlood (ADR-022), so this
/// fixture must satisfy EventManagement's own dependency first, the same
/// ordering EventManagementComponentTestDatabase itself already uses —
/// mirroring what Nexus1.ModularRuntime's Program.cs does at runtime.
/// </summary>
public abstract class MaintenanceComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=MaintenanceComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await alarmManagementContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var instrumentationContext = CreateInstrumentationDbContext();
        await instrumentationContext.Database.MigrateAsync();

        await using var eventManagementContext = CreateEventManagementDbContext();
        await eventManagementContext.Database.MigrateAsync();

        await using var maintenanceContext = CreateDbContext();
        await maintenanceContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected MaintenanceDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<MaintenanceDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected AlarmManagementDbContext CreateAlarmManagementDbContext() =>
        new(new DbContextOptionsBuilder<AlarmManagementDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected InstrumentationDbContext CreateInstrumentationDbContext() =>
        new(new DbContextOptionsBuilder<InstrumentationDbContext>().UseSqlServer(ConnectionString).Options);

    protected EventManagementDbContext CreateEventManagementDbContext() =>
        new(new DbContextOptionsBuilder<EventManagementDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(MaintenanceDbContext dbContext) => new EfUnitOfWork(dbContext);
}
