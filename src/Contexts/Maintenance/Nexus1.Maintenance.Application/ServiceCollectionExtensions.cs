using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Maintenance.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaintenanceApplication(this IServiceCollection services) => services
        .AddScoped<GetAssetsByUnitQueryHandler>()
        .AddScoped<GetOpenWorkOrdersByUnitQueryHandler>()
        .AddScoped<GetWorkOrdersWithOriginQueryHandler>()
        .AddScoped<GetLatestConditionPerAssetQueryHandler>()
        .AddScoped<GetActiveDegradationCasesQueryHandler>()
        .AddScoped<GetUnitAssetConditionsQueryHandler>()
        .AddScoped<RecordAssetConditionCommandHandler>()
        .AddScoped<OpenWorkOrderCommandHandler>()
        .AddScoped<RecordDegradationCommandHandler>();
}
