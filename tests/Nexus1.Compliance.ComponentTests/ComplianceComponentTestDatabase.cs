using Microsoft.EntityFrameworkCore;
using Nexus1.Compliance.Infrastructure.Persistence;

namespace Nexus1.Compliance.ComponentTests;

/// <summary>Real LocalDB, real migrations, no mocks — a fresh database per test. Mirrors AuditComponentTestDatabase.</summary>
public abstract class ComplianceComponentTestDatabase : IAsyncLifetime
{
    protected readonly string ConnectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=ComplianceComponentTests_{Guid.NewGuid():N};Trusted_Connection=True;";

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

    protected ComplianceDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ComplianceDbContext>().UseSqlServer(ConnectionString).Options);
}
