using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

/// <summary>RolePermission is composite-keyed (plain class, see RolePermission's own doc comment) — needs its own writer, matching IUserRoleWriter's reasoning.</summary>
public interface IRolePermissionWriter
{
    Task<bool> ExistsAsync(ApplicationRoleId applicationRoleId, PermissionId permissionId, CancellationToken cancellationToken);

    Task AddAsync(RolePermission rolePermission, CancellationToken cancellationToken);
}
