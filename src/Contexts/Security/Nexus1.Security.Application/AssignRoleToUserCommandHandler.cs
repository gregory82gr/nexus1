using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

public sealed class AssignRoleToUserCommandHandler(
    IRepository<ApplicationUser, ApplicationUserId> userRepository,
    IRepository<ApplicationRole, ApplicationRoleId> roleRepository,
    IUserRoleWriter userRoleWriter,
    [FromKeyedServices("Security")] IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
    {
        var userId = new ApplicationUserId(command.ApplicationUserId);
        var roleId = new ApplicationRoleId(command.ApplicationRoleId);

        if (await userRepository.GetByIdAsync(userId, cancellationToken) is null)
        {
            return Result.Failure($"ApplicationUser {command.ApplicationUserId} does not exist.");
        }

        if (await roleRepository.GetByIdAsync(roleId, cancellationToken) is null)
        {
            return Result.Failure($"ApplicationRole {command.ApplicationRoleId} does not exist.");
        }

        if (await userRoleWriter.ExistsAsync(userId, roleId, cancellationToken))
        {
            return Result.Failure($"User {command.ApplicationUserId} already has role {command.ApplicationRoleId} assigned.");
        }

        UserRole userRole;
        try
        {
            userRole = new UserRole(
                userId, roleId, dateTimeProvider.UtcNow,
                command.AssignedByUserId is { } assignedBy ? new ApplicationUserId(assignedBy) : null,
                command.ExpiresAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await userRoleWriter.AddAsync(userRole, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
