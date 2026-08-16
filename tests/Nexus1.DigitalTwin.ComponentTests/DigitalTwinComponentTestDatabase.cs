using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.DigitalTwin.Infrastructure.Persistence;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// DigitalTwin has real cross-context SQL FOREIGN KEYs to ReactorFleet.Unit,
/// CorePlatform.EngineeringUnit and Instrumentation.Signal (ADR-020), so this
/// fixture migrates all four contexts' own DbContexts against the SAME
/// connection string, in dependency order (ReactorFleet, CorePlatform and
/// Instrumentation first, so their tables physically exist before
/// DigitalTwin's migration adds FK constraints against them) — mirroring
/// what Nexus1.ModularRuntime's Program.cs does at runtime, and extending
/// Instrumentation's own two-context fixture (ADR-019) by one more context,
/// per Instrumentation.ComponentTests.InstrumentationComponentTestDatabase's
/// own precedent pattern.
/// </summary>
public abstract class DigitalTwinComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=DigitalTwinComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var instrumentationContext = CreateInstrumentationDbContext();
        await instrumentationContext.Database.MigrateAsync();

        await using var digitalTwinContext = CreateDbContext();
        await digitalTwinContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected DigitalTwinDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<DigitalTwinDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected InstrumentationDbContext CreateInstrumentationDbContext() =>
        new(new DbContextOptionsBuilder<InstrumentationDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(DigitalTwinDbContext dbContext) => new EfUnitOfWork(dbContext);
}
