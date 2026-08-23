using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Compliance.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComplianceApplication(this IServiceCollection services) => services
        .AddScoped<GetComplianceReviewsBySourceAnalysisIdQueryHandler>();
}
