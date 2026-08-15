using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class RootCauseComponentTestDatabase : IAsyncLifetime
{
    private readonly string _connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=RootCauseComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected RootCauseDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<RootCauseDbContext>().UseSqlServer(_connectionString).Options);

    protected static IUnitOfWork UnitOfWork(RootCauseDbContext dbContext) => new EfUnitOfWork(dbContext);

    protected static IRepository<RootCauseAnalysis, RootCauseAnalysisId> Repository(RootCauseDbContext dbContext) =>
        new RootCauseAnalysisRepository(dbContext);
}
