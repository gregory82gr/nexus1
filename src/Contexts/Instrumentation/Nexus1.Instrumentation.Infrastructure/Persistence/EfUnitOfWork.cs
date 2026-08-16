using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(InstrumentationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
