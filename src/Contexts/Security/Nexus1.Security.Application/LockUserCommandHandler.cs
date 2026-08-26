using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

public sealed class LockUserCommandHandler(IRepository<ApplicationUser, ApplicationUserId> userRepository, [FromKeyedServices("Security")] IUnitOfWork unitOfWork)
    : ICommandHandler<LockUserCommand>
{
    public async Task<Result> Handle(LockUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(new ApplicationUserId(command.ApplicationUserId), cancellationToken);
        if (user is null)
        {
            return Result.Failure($"ApplicationUser {command.ApplicationUserId} does not exist.");
        }

        user.Lock(command.LockoutEndUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
