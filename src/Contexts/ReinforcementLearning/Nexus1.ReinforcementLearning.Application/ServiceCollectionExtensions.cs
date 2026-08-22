using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.ReinforcementLearning.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReinforcementLearningApplication(this IServiceCollection services) => services
        .AddScoped<RecordTrainingRunCommandHandler>()
        .AddScoped<ExtractPolicyCommandHandler>()
        .AddScoped<RecordAdvisoryRecommendationCommandHandler>()
        .AddScoped<GetPolicyEntryCountQueryHandler>()
        .AddScoped<GetFinalQTableEntryCountQueryHandler>()
        .AddScoped<GetPolicyGridQueryHandler>()
        .AddScoped<GetClampedRecommendationsQueryHandler>();
}
