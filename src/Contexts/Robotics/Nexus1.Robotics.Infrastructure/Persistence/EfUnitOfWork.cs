using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Robotics.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(RoboticsDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
