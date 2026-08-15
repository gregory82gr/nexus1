using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlarmManagementInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AlarmManagementDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_AlarmManagement")));

        services.AddScoped<IRepository<AlarmDefinition, AlarmDefinitionId>, EfRepository<AlarmDefinition, AlarmDefinitionId>>();
        services.AddScoped<IRepository<AlarmEvent, AlarmEventId>, EfRepository<AlarmEvent, AlarmEventId>>();
        services.AddScoped<IRepository<AlarmFlood, AlarmFloodId>, EfRepository<AlarmFlood, AlarmFloodId>>();
        services.AddScoped<IAlarmDefinitionFinder, EfAlarmDefinitionFinder>();
        services.AddScoped<IAlarmEventFinder, EfAlarmEventFinder>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
