using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.DigitalTwin.Infrastructure.Persistence;
using Nexus1.Instrumentation.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// ReinforcementLearning has real cross-context SQL FOREIGN KEYs to
/// ReactorFleet.Unit, DigitalTwin.TwinModel and CorePlatform.EngineeringUnit
/// (ADR-026's own whole-sector FK audit). DigitalTwin itself has real FKs to
/// ReactorFleet.Unit, CorePlatform.EngineeringUnit and Instrumentation.Signal
/// (ADR-020), so this fixture migrates ReactorFleet, then CorePlatform, then
/// Instrumentation, then DigitalTwin, then ReinforcementLearningDbContext,
/// against the SAME connection string — mirroring
/// DigitalTwinComponentTestDatabase's own four-context precedent extended by
/// one more context, and matching Nexus1.ModularRuntime's own Program.cs
/// composition order.
/// </summary>
public abstract class ReinforcementLearningComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=ReinforcementLearningComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var corePlatformContext = CreateCorePlatformDbContext();
        await corePlatformContext.Database.MigrateAsync();

        await using var instrumentationContext = CreateInstrumentationDbContext();
        await instrumentationContext.Database.MigrateAsync();

        await using var digitalTwinContext = CreateDigitalTwinDbContext();
        await digitalTwinContext.Database.MigrateAsync();

        await using var reinforcementLearningContext = CreateDbContext();
        await reinforcementLearningContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected ReinforcementLearningDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ReinforcementLearningDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected CorePlatformDbContext CreateCorePlatformDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected InstrumentationDbContext CreateInstrumentationDbContext() =>
        new(new DbContextOptionsBuilder<InstrumentationDbContext>().UseSqlServer(ConnectionString).Options);

    protected DigitalTwinDbContext CreateDigitalTwinDbContext() =>
        new(new DbContextOptionsBuilder<DigitalTwinDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(ReinforcementLearningDbContext dbContext) => new EfUnitOfWork(dbContext);
}
