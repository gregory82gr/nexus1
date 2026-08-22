using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Application;

public sealed class CloseSignalQualityEventCommandHandler(
    IRepository<SignalQualityEvent, SignalQualityEventId> eventRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CloseSignalQualityEventCommand>
{
    public async Task<Result> Handle(CloseSignalQualityEventCommand command, CancellationToken cancellationToken)
    {
        var qualityEvent = await eventRepository.GetByIdAsync(new SignalQualityEventId(command.SignalQualityEventId), cancellationToken);
        if (qualityEvent is null)
        {
            return Result.Failure($"SignalQualityEvent {command.SignalQualityEventId} does not exist.");
        }

        try
        {
            qualityEvent.Close(command.EndedAtUtc, command.ReasonCode);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
