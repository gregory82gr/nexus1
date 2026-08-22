using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.EventManagement.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// EventManagement has real cross-context SQL FOREIGN KEYs to ReactorFleet.Unit,
/// AlarmManagement.AlarmEvent and AlarmManagement.AlarmFlood (ADR-022), so this
/// fixture migrates all three contexts' own DbContexts against the SAME
/// connection string, in dependency order (ReactorFleet and AlarmManagement
/// first, so their tables physically exist before EventManagement's migration
/// adds FK constraints against them) — mirroring what Nexus1.ModularRuntime's
/// Program.cs does at runtime, and copying
/// Nexus1.Maintenance.ComponentTests.MaintenanceComponentTestDatabase's own
/// precedent pattern exactly (ADR-021/ADR-022).
/// </summary>
public abstract class EventManagementComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=EventManagementComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var alarmManagementContext = CreateAlarmManagementDbContext();
        await alarmManagementContext.Database.MigrateAsync();

        await using var eventManagementContext = CreateDbContext();
        await eventManagementContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected EventManagementDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<EventManagementDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected AlarmManagementDbContext CreateAlarmManagementDbContext() =>
        new(new DbContextOptionsBuilder<AlarmManagementDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(EventManagementDbContext dbContext) => new EfUnitOfWork(dbContext);
}
