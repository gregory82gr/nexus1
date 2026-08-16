using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.CorePlatform.Infrastructure.Persistence;

namespace Nexus1.CorePlatform.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class CorePlatformComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=CorePlatformComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected CorePlatformDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<CorePlatformDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(CorePlatformDbContext dbContext) => new EfUnitOfWork(dbContext);
}
