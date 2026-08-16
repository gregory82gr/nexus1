using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.Metrics;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test.</summary>
public abstract class RootCauseComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
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
        new(new DbContextOptionsBuilder<RootCauseDbContext>().UseSqlServer(ConnectionString).Options);

    protected static IUnitOfWork UnitOfWork(RootCauseDbContext dbContext) => new EfUnitOfWork(dbContext);

    protected static IRepository<RootCauseAnalysis, RootCauseAnalysisId> Repository(RootCauseDbContext dbContext) =>
        new RootCauseAnalysisRepository(dbContext);

    /// <summary>A fresh, uncaptured instrument set per call — tests that need to assert on recorded measurements build their own MeterListener against this, mirroring Nexus1.BuildingBlocks.Observability.UnitTests' TestMeterFactory (duplicated, not shared, per this project's own convention for test-only doubles).</summary>
    protected static NexusRuntimeMetrics NewMetrics() => new(new TestMeterFactory());

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
