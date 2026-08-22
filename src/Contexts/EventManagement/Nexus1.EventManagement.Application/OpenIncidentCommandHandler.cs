using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Application;

public sealed class OpenIncidentCommandHandler(
    IRepository<OperationalEvent, OperationalEventId> eventRepository,
    IRepository<Incident, IncidentId> incidentRepository,
    IIncidentExistenceFinder incidentExistenceFinder,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<OpenIncidentCommand, long>
{
    public async Task<Result<long>> Handle(OpenIncidentCommand command, CancellationToken cancellationToken)
    {
        if (await eventRepository.GetByIdAsync(new OperationalEventId(command.OperationalEventId), cancellationToken) is null)
        {
            return Result<long>.Failure($"OperationalEvent {command.OperationalEventId} does not exist.");
        }

        if (await incidentExistenceFinder.ExistsForOperationalEventAsync(command.OperationalEventId, cancellationToken))
        {
            return Result<long>.Failure($"OperationalEvent {command.OperationalEventId} already has an open incident.");
        }

        Incident incident;
        try
        {
            incident = Incident.Create(
                new IncidentId(idGenerator.NextLong()), command.OperationalEventId, new IncidentTypeId(command.IncidentTypeId),
                new IncidentStatusId(command.IncidentStatusId), command.IncidentNumber, command.OpenedAtUtc,
                command.InvestigationSummary, leadInvestigatorUserId: command.LeadInvestigatorUserId);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await incidentRepository.AddAsync(incident, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(incident.Id.Value);
    }
}
