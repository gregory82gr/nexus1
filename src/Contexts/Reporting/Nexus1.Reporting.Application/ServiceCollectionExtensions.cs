using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Reporting.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportingApplication(this IServiceCollection services) => services
        .AddScoped<GetCaseSummariesForUnitQueryHandler>();
}
