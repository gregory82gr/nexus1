using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.Metrics;
using Nexus1.Audit.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.Audit.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test. Mirrors RootCauseComponentTestDatabase.</summary>
public abstract class AuditComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=AuditComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected AuditDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(new AuditAppendOnlyInterceptor())
            .Options);

    /// <summary>A fresh, uncaptured instrument set per call — mirrors RootCauseComponentTestDatabase's NewMetrics() (duplicated, not shared, per this project's own convention for test-only doubles).</summary>
    protected static NexusRuntimeMetrics NewMetrics() => new(new TestMeterFactory());

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
