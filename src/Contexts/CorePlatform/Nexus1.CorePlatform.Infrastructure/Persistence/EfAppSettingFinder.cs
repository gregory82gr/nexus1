using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfAppSettingFinder(CorePlatformDbContext dbContext) : IAppSettingFinder
{
    public async Task<AppSetting?> FindByKeyAsync(string key, CancellationToken cancellationToken) =>
        await dbContext.AppSettings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
}
