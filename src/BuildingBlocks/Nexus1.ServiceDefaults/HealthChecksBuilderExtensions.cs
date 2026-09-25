using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexus1.ServiceDefaults;

public static class HealthChecksBuilderExtensions
{
    /// <summary>
    /// Registers the unchanged <see cref="DbContextHealthCheck{TContext}"/> against a KEYED
    /// DbContext registration (ADR-044). A plain <c>AddCheck&lt;DbContextHealthCheck&lt;T&gt;&gt;</c>
    /// resolves the unkeyed registration of <typeparamref name="TContext"/> — so when a context
    /// type is registered twice (e.g. a read-write connection plus a keyed read-only one), it would
    /// silently re-check the wrong connection. The factory runs inside the health-check service's
    /// per-run scope, so the keyed scoped context is resolved fresh for every probe.
    /// </summary>
    public static IHealthChecksBuilder AddKeyedDbContextCheck<TContext>(
        this IHealthChecksBuilder builder, string name, object serviceKey, TimeSpan timeout)
        where TContext : DbContext =>
        builder.Add(new HealthCheckRegistration(
            name,
            serviceProvider => new DbContextHealthCheck<TContext>(serviceProvider.GetRequiredKeyedService<TContext>(serviceKey)),
            failureStatus: HealthStatus.Unhealthy,
            tags: null,
            timeout: timeout));
}
