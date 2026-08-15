using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(AlarmManagementDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
