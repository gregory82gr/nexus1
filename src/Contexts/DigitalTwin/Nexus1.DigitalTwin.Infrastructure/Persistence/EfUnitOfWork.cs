using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(DigitalTwinDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
