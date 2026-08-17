namespace Nexus1.RadiationMonitoring.Application;

public interface ILatestReadingPerMonitorFinder
{
    Task<IReadOnlyList<LatestRadiationReadingDto>> GetLatestReadingsAsync(CancellationToken cancellationToken);
}
