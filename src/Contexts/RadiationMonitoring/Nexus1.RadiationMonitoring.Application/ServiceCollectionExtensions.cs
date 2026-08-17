using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.RadiationMonitoring.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRadiationMonitoringApplication(this IServiceCollection services) => services
        .AddScoped<RegisterRadiationZoneCommandHandler>()
        .AddScoped<RecordRadiationReadingCommandHandler>()
        .AddScoped<GetActiveRadiationZonesQueryHandler>()
        .AddScoped<GetMonitorsWithCalibrationDueQueryHandler>()
        .AddScoped<GetLatestReadingPerMonitorQueryHandler>()
        .AddScoped<GetOpenDoseAlertsQueryHandler>();
}
