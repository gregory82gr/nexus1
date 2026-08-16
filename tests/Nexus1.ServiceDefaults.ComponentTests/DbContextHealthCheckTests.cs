using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexus1.ServiceDefaults.ComponentTests;

/// <summary>
/// Proves the specific failure mode found in CorePlatform (ADR-018): a
/// database that exists and is reachable, but whose own migration for this
/// context was never applied, must report Unhealthy — not the false
/// Healthy the old CanConnectAsync-only check produced. Real LocalDB, no
/// mocks, matching this project's own component-test discipline.
/// </summary>
public sealed class DbContextHealthCheckTests : IAsyncLifetime
{
    private readonly string _databaseName = $"HealthCheckTests_{Guid.NewGuid():N}";

    private string ConnectionString =>
        $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;";

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }

    private HealthCheckTestDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<HealthCheckTestDbContext>().UseSqlServer(ConnectionString).Options);

    [Fact]
    public async Task Unhealthy_when_the_database_does_not_exist()
    {
        await using var dbContext = CreateDbContext();
        var sut = new DbContextHealthCheck<HealthCheckTestDbContext>(dbContext);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("Cannot connect", result.Description);
    }

    /// <summary>
    /// This is the exact CorePlatform failure mode: the database itself
    /// exists and is reachable (created here via EnsureCreatedAsync,
    /// bypassing migrations entirely, the same effective end state as a
    /// database whose migration for this context was simply never run),
    /// but no migration has ever been recorded against it. The old
    /// CanConnectAsync-only check would report Healthy here — the whole
    /// point of ADR-018's fix is that it must not.
    /// </summary>
    [Fact]
    public async Task Unhealthy_when_the_database_exists_but_this_contexts_migration_was_never_applied()
    {
        await using (var setup = CreateDbContext())
        {
            await setup.Database.EnsureCreatedAsync();
        }

        await using var dbContext = CreateDbContext();
        var sut = new DbContextHealthCheck<HealthCheckTestDbContext>(dbContext);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("missing", result.Description);
        Assert.Contains("migration", result.Description);
    }

    [Fact]
    public async Task Healthy_when_the_database_exists_and_this_contexts_migration_was_applied()
    {
        await using (var setup = CreateDbContext())
        {
            await setup.Database.MigrateAsync();
        }

        await using var dbContext = CreateDbContext();
        var sut = new DbContextHealthCheck<HealthCheckTestDbContext>(dbContext);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}
