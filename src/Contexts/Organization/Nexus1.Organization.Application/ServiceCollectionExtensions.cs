using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Organization.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrganizationApplication(this IServiceCollection services) => services
        .AddScoped<GetSitePlantHierarchyQueryHandler>()
        .AddScoped<ResolvePersonOrganizationContextQueryHandler>()
        .AddScoped<AssignPersonToDepartmentCommandHandler>()
        .AddScoped<AssignPersonToTeamCommandHandler>()
        .AddScoped<RecordStaffingScenarioResultCommandHandler>()
        .AddScoped<GetLatestStaffingGapsQueryHandler>();
}
