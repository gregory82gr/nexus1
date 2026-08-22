using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(SecurityDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
