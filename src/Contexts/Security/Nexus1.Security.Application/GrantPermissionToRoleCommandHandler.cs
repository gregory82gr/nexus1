using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

public sealed class GrantPermissionToRoleCommandHandler(
    IRepository<ApplicationRole, ApplicationRoleId> roleRepository,
    IRepository<Permission, PermissionId> permissionRepository,
    IRolePermissionWriter rolePermissionWriter,
    [FromKeyedServices("Security")] IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<GrantPermissionToRoleCommand>
{
    public async Task<Result> Handle(GrantPermissionToRoleCommand command, CancellationToken cancellationToken)
    {
        var roleId = new ApplicationRoleId(command.ApplicationRoleId);
        var permissionId = new PermissionId(command.PermissionId);

        if (await roleRepository.GetByIdAsync(roleId, cancellationToken) is null)
        {
            return Result.Failure($"ApplicationRole {command.ApplicationRoleId} does not exist.");
        }

        if (await permissionRepository.GetByIdAsync(permissionId, cancellationToken) is null)
        {
            return Result.Failure($"Permission {command.PermissionId} does not exist.");
        }

        if (await rolePermissionWriter.ExistsAsync(roleId, permissionId, cancellationToken))
        {
            return Result.Failure($"Role {command.ApplicationRoleId} already has permission {command.PermissionId}.");
        }

        RolePermission rolePermission;
        try
        {
            rolePermission = new RolePermission(
                roleId, permissionId, dateTimeProvider.UtcNow, command.IsGranted,
                command.GrantedByUserId is { } grantedBy ? new ApplicationUserId(grantedBy) : null,
                command.ExpiresAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await rolePermissionWriter.AddAsync(rolePermission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
