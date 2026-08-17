using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(EmergencyPreparednessDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
