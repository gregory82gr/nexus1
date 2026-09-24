using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.RootCause.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRootCauseApplication(this IServiceCollection services) => services
        .AddScoped<OpenAnalysisCommandHandler>()
        .AddScoped<OpenProvenanceAnalysisCommandHandler>()
        .AddScoped<AddHypothesisCommandHandler>()
        .AddScoped<AddEvidenceCommandHandler>()
        .AddScoped<RejectHypothesisCommandHandler>()
        .AddScoped<CloseAnalysisCommandHandler>()
        .AddScoped<MarkAnalysisInconclusiveCommandHandler>()
        .AddScoped<GetAnalysisByIdQueryHandler>()
        .AddScoped<Diagnosis.RunFixedIncidentDiagnosisCommandHandler>()
        .AddScoped<Diagnosis.GetIncidentGraphQueryHandler>();
}
