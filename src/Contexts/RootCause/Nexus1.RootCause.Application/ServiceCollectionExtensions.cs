using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.RootCause.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRootCauseApplication(this IServiceCollection services) => services
        .AddScoped<OpenAnalysisCommandHandler>()
        .AddScoped<AddHypothesisCommandHandler>()
        .AddScoped<AddEvidenceCommandHandler>()
        .AddScoped<RejectHypothesisCommandHandler>()
        .AddScoped<CloseAnalysisCommandHandler>()
        .AddScoped<GetAnalysisByIdQueryHandler>();
}
