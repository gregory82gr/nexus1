using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.AlarmManagement.Application;

public sealed class AcknowledgeAlarmCommandHandler(
    IRepository<AlarmEvent, AlarmEventId> eventRepository,
    [FromKeyedServices("AlarmManagement")] IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AcknowledgeAlarmCommand>
{
    public async Task<Result> Handle(AcknowledgeAlarmCommand command, CancellationToken cancellationToken)
    {
        using var activity = NexusActivitySources.AlarmManagementSource.StartActivity(
            SpanNames.AlarmAcknowledge, ActivityKind.Internal, parentContext: default,
            tags: SafeTags.ForOwnerOperation(messageId: null, "ATTEMPTED"));

        var alarmEvent = await eventRepository.GetByIdAsync(new AlarmEventId(command.AlarmEventId), cancellationToken);
        if (alarmEvent is null)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure($"Alarm event {command.AlarmEventId} does not exist.");
        }

        try
        {
            alarmEvent.Acknowledge(new UserId(command.AcknowledgedByUserId), dateTimeProvider.UtcNow);
        }
        catch (InvalidOperationException ex)
        {
            activity?.SetTag("nexus1.outcome.code", "REJECTED");
            return Result.Failure(ex.Message);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            SafeError.Record(activity, ex);
            throw;
        }

        activity?.SetTag("nexus1.outcome.code", "COMMITTED");
        return Result.Success();
    }
}
