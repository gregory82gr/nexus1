using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfLocalizationFinder(CorePlatformDbContext dbContext) : ILocalizationFinder
{
    public async Task<Localization?> FindAsync(string resourceKey, LanguageId languageId, CancellationToken cancellationToken) =>
        await dbContext.Localizations.SingleOrDefaultAsync(
            x => x.ResourceKey == resourceKey && x.LanguageId == languageId, cancellationToken);
}
