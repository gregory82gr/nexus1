namespace Nexus1.EventManagement.Application;

public interface IEventTimelineFinder
{
    Task<IReadOnlyList<EventTimelineEntryDto>> GetTimelineAsync(long operationalEventId, CancellationToken cancellationToken);
}
