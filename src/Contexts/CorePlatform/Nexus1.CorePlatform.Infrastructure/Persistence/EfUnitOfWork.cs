using Nexus1.BuildingBlocks.Application;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(CorePlatformDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
