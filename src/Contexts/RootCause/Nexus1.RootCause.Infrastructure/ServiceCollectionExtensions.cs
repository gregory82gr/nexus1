using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRootCauseInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RootCauseDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RootCause")));

        services.AddScoped<IRepository<RootCauseAnalysis, RootCauseAnalysisId>, RootCauseAnalysisRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Assumes AddNexusMessaging(...) was already called (Host composition
        // root registers messaging once per host, not once per context).
        services.AddSingleton<AlarmFloodMessageHandler>();
        services.AddHostedService<AlarmFloodConsumerBackgroundService>();

        services.AddScoped<RetryDispatcher>();
        services.AddHostedService<RetryDispatcherBackgroundService>();

        return services;
    }
}
