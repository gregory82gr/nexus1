using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(RadiationMonitoringDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
