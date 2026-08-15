using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class AcknowledgeAlarmCommandHandler(
    IRepository<AlarmEvent, AlarmEventId> eventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AcknowledgeAlarmCommand>
{
    public async Task<Result> Handle(AcknowledgeAlarmCommand command, CancellationToken cancellationToken)
    {
        var alarmEvent = await eventRepository.GetByIdAsync(new AlarmEventId(command.AlarmEventId), cancellationToken);
        if (alarmEvent is null)
        {
            return Result.Failure($"Alarm event {command.AlarmEventId} does not exist.");
        }

        try
        {
            alarmEvent.Acknowledge(new UserId(command.AcknowledgedByUserId), dateTimeProvider.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
