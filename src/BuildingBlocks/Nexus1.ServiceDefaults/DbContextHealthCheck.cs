using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexus1.ServiceDefaults;

/// <summary>
/// Readiness check for a composed context's database — "can this host
/// actually reach its data" (ADR-007), not liveness ("is the process up").
/// Checks both that the database is reachable AND that this context's own
/// migrations have actually been applied to it (ADR-018): a database can be
/// reachable while a specific context's schema inside it is missing — e.g.
/// a context sharing another context's physical database whose own
/// migration was never run. <c>CanConnectAsync</c> alone cannot see that;
/// <c>GetPendingMigrationsAsync</c> reads the target database's own
/// <c>__EFMigrationsHistory</c> table for this context, so a missing
/// schema surfaces as pending migrations, not a false Healthy.
/// </summary>
public sealed class DbContextHealthCheck<TContext>(TContext dbContext) : IHealthCheck
    where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy($"Cannot connect to {typeof(TContext).Name}'s database.");
            }

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            return pendingMigrations.Count == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    $"{typeof(TContext).Name}'s database is reachable but missing {pendingMigrations.Count} " +
                    $"migration(s): {string.Join(", ", pendingMigrations)}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{typeof(TContext).Name} connectivity or migration check threw.", ex);
        }
    }
}
