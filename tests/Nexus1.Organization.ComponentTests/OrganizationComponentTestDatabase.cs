using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class OrganizationComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=OrganizationComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected OrganizationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<OrganizationDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(OrganizationDbContext dbContext) => new EfUnitOfWork(dbContext);
}
