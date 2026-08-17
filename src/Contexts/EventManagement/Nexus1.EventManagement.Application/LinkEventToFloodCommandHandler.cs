using Nexus1.BuildingBlocks.Application;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Application;

public sealed class LinkEventToFloodCommandHandler(
    IRepository<OperationalEvent, OperationalEventId> eventRepository,
    IRepository<EventFloodLink, EventFloodLinkId> linkRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator)
    : ICommandHandler<LinkEventToFloodCommand, long>
{
    public async Task<Result<long>> Handle(LinkEventToFloodCommand command, CancellationToken cancellationToken)
    {
        if (await eventRepository.GetByIdAsync(new OperationalEventId(command.OperationalEventId), cancellationToken) is null)
        {
            return Result<long>.Failure($"OperationalEvent {command.OperationalEventId} does not exist.");
        }

        EventFloodLink link;
        try
        {
            link = EventFloodLink.Create(
                new EventFloodLinkId(idGenerator.NextLong()), command.OperationalEventId, command.AlarmFloodId,
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
