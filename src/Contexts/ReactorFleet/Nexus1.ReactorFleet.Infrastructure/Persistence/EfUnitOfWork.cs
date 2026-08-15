using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(ReactorFleetDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
