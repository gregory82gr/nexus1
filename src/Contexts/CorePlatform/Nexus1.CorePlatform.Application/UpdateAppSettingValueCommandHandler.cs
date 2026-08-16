using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Application;

public sealed class UpdateAppSettingValueCommandHandler(IAppSettingFinder appSettingFinder, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAppSettingValueCommand>
{
    public async Task<Result> Handle(UpdateAppSettingValueCommand command, CancellationToken cancellationToken)
    {
        var setting = await appSettingFinder.FindByKeyAsync(command.Key, cancellationToken);
        if (setting is null)
        {
            return Result.Failure($"AppSetting '{command.Key}' does not exist.");
        }

        try
        {
            setting.UpdateValue(command.NewValue);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
