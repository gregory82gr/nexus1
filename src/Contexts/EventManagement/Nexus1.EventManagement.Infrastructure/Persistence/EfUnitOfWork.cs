using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(EventManagementDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
