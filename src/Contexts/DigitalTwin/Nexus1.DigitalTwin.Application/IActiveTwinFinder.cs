namespace Nexus1.DigitalTwin.Application;

public interface IActiveTwinFinder
{
    Task<IReadOnlyList<ActiveTwinDto>> GetActiveTwinsAsync(CancellationToken cancellationToken);
}
