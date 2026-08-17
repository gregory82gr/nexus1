namespace Nexus1.EventManagement.Application;

public interface IEventWithAlarmsAndFloodFinder
{
    Task<EventWithAlarmsAndFloodDto?> GetByEventCodeAsync(string eventCode, CancellationToken cancellationToken);
}
