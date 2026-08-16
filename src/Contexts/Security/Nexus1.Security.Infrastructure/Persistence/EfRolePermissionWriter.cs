using Microsoft.EntityFrameworkCore;
using Nexus1.Security.Application;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Infrastructure.Persistence;

internal sealed class EfRolePermissionWriter(SecurityDbContext dbContext) : IRolePermissionWriter
{
    public async Task<bool> ExistsAsync(ApplicationRoleId applicationRoleId, PermissionId permissionId, CancellationToken cancellationToken) =>
        await dbContext.RolePermissions.AnyAsync(
            x => x.ApplicationRoleId == applicationRoleId && x.PermissionId == permissionId, cancellationToken);

    public async Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken) =>
        await dbContext.RolePermissions.AddAsync(rolePermission, cancellationToken);
}
