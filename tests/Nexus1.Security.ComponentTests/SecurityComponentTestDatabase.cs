using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class SecurityComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=SecurityComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected SecurityDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<SecurityDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(SecurityDbContext dbContext) => new EfUnitOfWork(dbContext);
}
