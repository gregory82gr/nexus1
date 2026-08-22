using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// Instrumentation has real cross-context SQL FOREIGN KEYs to ReactorFleet.Unit
/// and CorePlatform.EngineeringUnit (ADR-019), so this fixture migrates all
/// three contexts' own DbContexts against the SAME connection string, in
/// dependency order (ReactorFleet and CorePlatform first, so their tables
/// physically exist before Instrumentation's migration adds FK constraints
/// against them) — mirroring what Nexus1.ModularRuntime's Program.cs does at
/// runtime by composing all three against one AlarmManagementDb connection
/// string. No existing component test fixture in this repository composed
/// more than one context's migrations before this sector.
/// </summary>
public abstract class InstrumentationComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=InstrumentationComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var instrumentationContext = CreateDbContext();
        await instrumentationContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected InstrumentationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<InstrumentationDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(InstrumentationDbContext dbContext) => new EfUnitOfWork(dbContext);
}
