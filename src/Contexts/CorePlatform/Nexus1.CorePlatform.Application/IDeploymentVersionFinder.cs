using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

public interface IDeploymentVersionFinder
{
    /// <summary>Matches the atlas's own C.1.8 verification query: current-per-component rows ordered by ComponentType, ComponentName.</summary>
    Task<IReadOnlyList<DeploymentVersion>> GetCurrentAsync(CancellationToken cancellationToken);
}
