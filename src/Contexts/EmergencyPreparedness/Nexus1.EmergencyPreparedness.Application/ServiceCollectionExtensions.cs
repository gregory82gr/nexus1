using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.EmergencyPreparedness.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmergencyPreparednessApplication(this IServiceCollection services) => services
        .AddScoped<ApproveEmergencyPlanCommandHandler>()
        .AddScoped<ScheduleExerciseCommandHandler>()
        .AddScoped<GetSiteActivePlansQueryHandler>()
        .AddScoped<GetExercisesWithCorrectiveObservationsQueryHandler>()
        .AddScoped<GetOpenOrRestrictedRoutesCrossingZonesQueryHandler>()
        .AddScoped<GetResourceReadinessDashboardQueryHandler>();
}
