using Microsoft.EntityFrameworkCore;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.2.8 "effective permissions for one user from active roles" verification query exactly.</summary>
internal sealed class EfEffectivePermissionFinder(SecurityDbContext dbContext) : IEffectivePermissionFinder
{
    public async Task<IReadOnlyList<EffectivePermissionDto>> GetEffectivePermissionsAsync(
        ApplicationUserId applicationUserId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var query =
            from ur in dbContext.UserRoles
            where ur.ApplicationUserId == applicationUserId && ur.IsActive
                && (ur.ExpiresAtUtc == null || ur.ExpiresAtUtc > nowUtc)
            join role in dbContext.ApplicationRoles on ur.ApplicationRoleId equals role.Id
            where role.IsActive
            join rp in dbContext.RolePermissions on role.Id equals rp.ApplicationRoleId
            where rp.ExpiresAtUtc == null || rp.ExpiresAtUtc > nowUtc
            join permission in dbContext.Permissions on rp.PermissionId equals permission.Id
            where permission.IsActive
            join category in dbContext.PermissionCategories on permission.PermissionCategoryId equals category.Id
            select new EffectivePermissionDto(permission.Code, permission.Name, category.Code, rp.IsGranted);

        // EF Core's SQL Server provider cannot translate .Distinct() over a
        // projected record type through this many joins — materialize first,
        // then dedupe/order in memory (small per-user result sets, so the
        // client-side cost is negligible).
        var rows = await query.ToListAsync(cancellationToken);
        return rows.Distinct()
            .OrderBy(x => x.CategoryCode, StringComparer.Ordinal)
            .ThenBy(x => x.PermissionCode, StringComparer.Ordinal)
            .ToList();
    }
}
