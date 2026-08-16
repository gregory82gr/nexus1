using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Security.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityApplication(this IServiceCollection services) => services
        .AddScoped<GetEffectivePermissionsForUserQueryHandler>()
        .AddScoped<AssignRoleToUserCommandHandler>()
        .AddScoped<GrantPermissionToRoleCommandHandler>()
        .AddScoped<LockUserCommandHandler>()
        .AddScoped<UnlockUserCommandHandler>()
        .AddScoped<UpdateUserPreferenceCommandHandler>();
}
