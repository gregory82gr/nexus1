using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Application;

public sealed class ReportOperationalEventCommandHandler(
    IRepository<OperationalEvent, OperationalEventId> eventRepository, IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<ReportOperationalEventCommand, long>
{
    public async Task<Result<long>> Handle(ReportOperationalEventCommand command, CancellationToken cancellationToken)
    {
        OperationalEvent operationalEvent;
        try
        {
            operationalEvent = OperationalEvent.Create(
                new OperationalEventId(idGenerator.NextLong()), command.UnitId, new EventTypeId(command.EventTypeId),
                new EventStatusId(command.EventStatusId), new EventSeverityId(command.EventSeverityId),
                new EventSourceTypeId(command.EventSourceTypeId), command.EventCode, command.Title, command.DetectedAtUtc,
                command.ReportedAtUtc, command.PlantId, command.Summary, command.ClosedAtUtc, command.ReportedByUserId,
                command.OwnerUserId, command.OwningPersonId, command.SourceReference, command.IsDrill);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await eventRepository.AddAsync(operationalEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(operationalEvent.Id.Value);
    }
}
