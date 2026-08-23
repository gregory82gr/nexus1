using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Audit.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditApplication(this IServiceCollection services) => services
        .AddScoped<GetAuditEvidenceBySourceAnalysisIdQueryHandler>();
}
