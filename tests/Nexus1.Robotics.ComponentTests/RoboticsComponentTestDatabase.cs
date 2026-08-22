using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.ComponentTests;

/// <summary>
/// Real LocalDB, real migrations, no mocks — a fresh database per test.
/// Robotics' only external dependency is ReactorFleet.Unit (ADR-023's own
/// whole-sector FK audit: the sole real cross-context FK in this sector's
/// fifteen-table scope), so this fixture migrates ReactorFleetDbContext
/// first, then RoboticsDbContext, against the SAME connection string —
/// simpler than EventManagement's/Maintenance's own multi-context fixture,
/// no AlarmManagement dependency needed here.
/// </summary>
public abstract class RoboticsComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=RoboticsComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var reactorFleetContext = CreateReactorFleetDbContext();
        await reactorFleetContext.Database.MigrateAsync();

        await using var roboticsContext = CreateDbContext();
        await roboticsContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected RoboticsDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<RoboticsDbContext>().UseSqlServer(ConnectionString).Options);

    protected ReactorFleetDbContext CreateReactorFleetDbContext() =>
        new(new DbContextOptionsBuilder<ReactorFleetDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(RoboticsDbContext dbContext) => new EfUnitOfWork(dbContext);
}
