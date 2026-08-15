using Microsoft.EntityFrameworkCore;
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
}
