using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfDeploymentVersionFinder(CorePlatformDbContext dbContext) : IDeploymentVersionFinder
{
    public async Task<IReadOnlyList<DeploymentVersion>> GetCurrentAsync(CancellationToken cancellationToken) =>
        await dbContext.DeploymentVersions
            .Where(x => x.IsCurrent)
            .OrderBy(x => x.ComponentType)
            .ThenBy(x => x.ComponentName)
            .ToListAsync(cancellationToken);
}
