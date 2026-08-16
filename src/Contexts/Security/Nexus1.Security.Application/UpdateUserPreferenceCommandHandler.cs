using Nexus1.BuildingBlocks.Application;
using Nexus1.Security.Domain;

namespace Nexus1.Security.Application;

public sealed class UpdateUserPreferenceCommandHandler(
    IRepository<ApplicationUser, ApplicationUserId> userRepository,
    IRepository<UserPreference, ApplicationUserId> preferenceRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateUserPreferenceCommand>
{
    public async Task<Result> Handle(UpdateUserPreferenceCommand command, CancellationToken cancellationToken)
    {
        var userId = new ApplicationUserId(command.ApplicationUserId);

        if (await userRepository.GetByIdAsync(userId, cancellationToken) is null)
        {
            return Result.Failure($"ApplicationUser {command.ApplicationUserId} does not exist.");
        }

        var existing = await preferenceRepository.GetByIdAsync(userId, cancellationToken);

        try
        {
            if (existing is null)
            {
                var preference = UserPreference.Create(
                    userId, command.LanguageId, command.TimeZoneId, dateTimeProvider.UtcNow, command.Theme,
                    command.ReceiveEmailAlerts, command.ReceiveInAppAlerts, command.DateFormat, command.NumberFormat);
                await preferenceRepository.AddAsync(preference, cancellationToken);
            }
            else
            {
                existing.Update(
                    command.LanguageId, command.TimeZoneId, command.Theme, command.ReceiveEmailAlerts,
                    command.ReceiveInAppAlerts, command.DateFormat, command.NumberFormat);
            }
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
