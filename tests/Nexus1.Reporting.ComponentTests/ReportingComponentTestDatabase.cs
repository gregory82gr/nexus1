using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.Metrics;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.Reporting.Infrastructure.Persistence;

namespace Nexus1.Reporting.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test. Mirrors AuditComponentTestDatabase/ComplianceComponentTestDatabase.</summary>
public abstract class ReportingComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=ReportingComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected ReportingDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ReportingDbContext>().UseSqlServer(ConnectionString).Options);

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
