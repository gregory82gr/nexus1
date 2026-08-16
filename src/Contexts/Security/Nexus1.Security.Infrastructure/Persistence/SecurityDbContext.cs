using Microsoft.EntityFrameworkCore;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence;

/// <summary>Own physical database (SecurityDb, ADR-016) — unlike CorePlatform/ReactorFleet, Security's ApplicationUser carries credential-adjacent columns, a data-sensitivity reason to isolate independent of deployment topology.</summary>
public sealed class SecurityDbContext(DbContextOptions<SecurityDbContext> options) : DbContext(options)
{
    public DbSet<UserStatus> UserStatuses => Set<UserStatus>();

    public DbSet<RoleType> RoleTypes => Set<RoleType>();

    public DbSet<PermissionCategory> PermissionCategories => Set<PermissionCategory>();

    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();

    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecurityDbContext).Assembly);
    }
}
