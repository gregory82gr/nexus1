using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfFeatureFlagFinder(CorePlatformDbContext dbContext) : IFeatureFlagFinder
{
    public async Task<FeatureFlag?> FindByCodeAsync(string code, string environmentName, CancellationToken cancellationToken) =>
        await dbContext.FeatureFlags.SingleOrDefaultAsync(
            x => x.Code == code && x.EnvironmentName == environmentName, cancellationToken);
}
