using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.AlarmManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlarmManagementApplication(this IServiceCollection services) => services
        .AddScoped<DefineAlarmCommandHandler>()
        .AddScoped<EvaluateReadingCommandHandler>()
        .AddScoped<AcknowledgeAlarmCommandHandler>()
        .AddScoped<DetectFloodCommandHandler>()
        .AddScoped<GetActiveAlarmsForUnitQueryHandler>()
        .AddScoped<GetActiveAlarmsQueryHandler>();
}
