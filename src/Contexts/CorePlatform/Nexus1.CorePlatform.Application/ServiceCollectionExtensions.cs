using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.CorePlatform.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorePlatformApplication(this IServiceCollection services) => services
        .AddScoped<UpdateAppSettingValueCommandHandler>()
        .AddScoped<EvaluateFeatureFlagQueryHandler>()
        .AddScoped<GetActiveEngineeringUnitsQueryHandler>()
        .AddScoped<ResolveLocalizedTextQueryHandler>()
        .AddScoped<GetCurrentDeploymentVersionsQueryHandler>();
}
