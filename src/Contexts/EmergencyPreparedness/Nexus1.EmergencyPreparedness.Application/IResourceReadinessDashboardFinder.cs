namespace Nexus1.EmergencyPreparedness.Application;

public interface IResourceReadinessDashboardFinder
{
    Task<IReadOnlyList<ResourceReadinessDashboardDto>> GetResourceReadinessDashboardAsync(CancellationToken cancellationToken);
}
