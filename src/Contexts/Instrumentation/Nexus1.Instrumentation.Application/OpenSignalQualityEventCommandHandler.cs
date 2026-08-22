using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Domain;

namespace Nexus1.Instrumentation.Application;

public sealed class OpenSignalQualityEventCommandHandler(
    IRepository<Signal, SignalId> signalRepository,
    IRepository<SignalQualityEvent, SignalQualityEventId> eventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IIdGenerator idGenerator)
    : ICommandHandler<OpenSignalQualityEventCommand, long>
{
    public async Task<Result<long>> Handle(OpenSignalQualityEventCommand command, CancellationToken cancellationToken)
    {
        var signalId = new SignalId(command.SignalId);
        if (await signalRepository.GetByIdAsync(signalId, cancellationToken) is null)
        {
            return Result<long>.Failure($"Signal {command.SignalId} does not exist.");
        }

        var qualityEvent = SignalQualityEvent.Create(
            new SignalQualityEventId(idGenerator.NextLong()), signalId, new SignalQualityId(command.SignalQualityId),
            command.StartedAtUtc, dateTimeProvider.UtcNow, command.ReasonCode, command.Notes);

        await eventRepository.AddAsync(qualityEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(qualityEvent.Id.Value);
    }
}
