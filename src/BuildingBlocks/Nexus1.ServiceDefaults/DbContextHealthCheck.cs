using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexus1.ServiceDefaults;

/// <summary>
/// Readiness check for a composed context's database — "can this host
/// actually reach its data" (ADR-007), not liveness ("is the process up").
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
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Cannot connect to {typeof(TContext).Name}'s database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{typeof(TContext).Name} connectivity check threw.", ex);
        }
    }
}
