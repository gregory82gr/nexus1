using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.EventManagement.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventManagementApplication(this IServiceCollection services) => services
        .AddScoped<GetEventWithAlarmsAndFloodQueryHandler>()
        .AddScoped<GetEventTimelineQueryHandler>()
        .AddScoped<GetOpenIncidentActionsQueryHandler>()
        .AddScoped<ReportOperationalEventCommandHandler>()
        .AddScoped<LinkEventToAlarmCommandHandler>()
        .AddScoped<LinkEventToFloodCommandHandler>()
        .AddScoped<OpenIncidentCommandHandler>()
        .AddScoped<RecordIncidentActionCommandHandler>();
}
