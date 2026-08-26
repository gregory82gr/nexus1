using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Application;

public sealed class RecordIncidentActionCommandHandler(
    IRepository<Incident, IncidentId> incidentRepository,
    IRepository<IncidentAction, IncidentActionId> actionRepository,
    [FromKeyedServices("EventManagement")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<RecordIncidentActionCommand, long>
{
    public async Task<Result<long>> Handle(RecordIncidentActionCommand command, CancellationToken cancellationToken)
    {
        if (await incidentRepository.GetByIdAsync(new IncidentId(command.IncidentId), cancellationToken) is null)
        {
            return Result<long>.Failure($"Incident {command.IncidentId} does not exist.");
        }

        IncidentAction action;
        try
        {
            action = IncidentAction.Create(
                new IncidentActionId(idGenerator.NextLong()), command.IncidentId, new IncidentActionTypeId(command.IncidentActionTypeId),
                new IncidentActionStatusId(command.IncidentActionStatusId), command.Title, command.Description, command.DueAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await actionRepository.AddAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(action.Id.Value);
    }
}
