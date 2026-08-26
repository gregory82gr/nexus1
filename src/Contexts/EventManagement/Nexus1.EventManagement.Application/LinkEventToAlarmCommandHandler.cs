using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Application;

public sealed class LinkEventToAlarmCommandHandler(
    IRepository<OperationalEvent, OperationalEventId> eventRepository,
    IRepository<EventAlarmLink, EventAlarmLinkId> linkRepository,
    [FromKeyedServices("EventManagement")] IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<LinkEventToAlarmCommand, long>
{
    public async Task<Result<long>> Handle(LinkEventToAlarmCommand command, CancellationToken cancellationToken)
    {
        if (await eventRepository.GetByIdAsync(new OperationalEventId(command.OperationalEventId), cancellationToken) is null)
        {
            return Result<long>.Failure($"OperationalEvent {command.OperationalEventId} does not exist.");
        }

        EventAlarmLink link;
        try
        {
            link = EventAlarmLink.Create(
                new EventAlarmLinkId(idGenerator.NextLong()), command.OperationalEventId, command.AlarmEventId,
                command.LinkRole, command.Note);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await linkRepository.AddAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(link.Id.Value);
    }
}
