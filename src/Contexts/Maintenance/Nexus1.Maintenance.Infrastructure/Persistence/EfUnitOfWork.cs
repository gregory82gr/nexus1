using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(MaintenanceDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
