using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(OrganizationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
