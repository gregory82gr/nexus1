using Microsoft.EntityFrameworkCore;
using Nexus1.Audit.Infrastructure.Persistence;

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
}
