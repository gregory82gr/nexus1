using Nexus1.BuildingBlocks.Application;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(ReinforcementLearningDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
