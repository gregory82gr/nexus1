using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;
using Nexus1.Security.Infrastructure.Persistence;

namespace Nexus1.Security.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at SecurityDb, Security's own physical database (ADR-016) — not shared with any other context.</summary>
    public static IServiceCollection AddSecurityInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SecurityDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Security")));

        services.AddScoped<IRepository<UserStatus, UserStatusId>, EfRepository<UserStatus, UserStatusId>>();
        services.AddScoped<IRepository<RoleType, RoleTypeId>, EfRepository<RoleType, RoleTypeId>>();
        services.AddScoped<IRepository<PermissionCategory, PermissionCategoryId>, EfRepository<PermissionCategory, PermissionCategoryId>>();
        services.AddScoped<IRepository<ApplicationUser, ApplicationUserId>, EfRepository<ApplicationUser, ApplicationUserId>>();
        services.AddScoped<IRepository<ApplicationRole, ApplicationRoleId>, EfRepository<ApplicationRole, ApplicationRoleId>>();
        services.AddScoped<IRepository<Permission, PermissionId>, EfRepository<Permission, PermissionId>>();
        services.AddScoped<IRepository<UserPreference, ApplicationUserId>, EfRepository<UserPreference, ApplicationUserId>>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IUserRoleWriter, EfUserRoleWriter>();
        services.AddScoped<IRolePermissionWriter, EfRolePermissionWriter>();
        services.AddScoped<IEffectivePermissionFinder, EfEffectivePermissionFinder>();

        return services;
    }
}
