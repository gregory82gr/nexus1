using Microsoft.EntityFrameworkCore;
using Nexus1.CorePlatform.Application;
using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfLanguageFinder(CorePlatformDbContext dbContext) : ILanguageFinder
{
    public async Task<Language?> FindByCodeAsync(string code, CancellationToken cancellationToken) =>
        await dbContext.Languages.SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
}
