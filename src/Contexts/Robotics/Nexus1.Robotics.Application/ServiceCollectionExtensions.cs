using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Robotics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoboticsApplication(this IServiceCollection services) => services
        .AddScoped<RegisterRobotCommandHandler>()
        .AddScoped<DispatchMissionCommandHandler>()
        .AddScoped<GetAvailableRobotsByUnitQueryHandler>()
        .AddScoped<GetLatestHealthSnapshotPerRobotQueryHandler>()
        .AddScoped<GetMissionTimelineQueryHandler>()
        .AddScoped<GetBlockingReadinessFailuresQueryHandler>()
        .AddScoped<GetUnitRoboticsOverviewQueryHandler>();
}
