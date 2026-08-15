using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(RootCauseDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
