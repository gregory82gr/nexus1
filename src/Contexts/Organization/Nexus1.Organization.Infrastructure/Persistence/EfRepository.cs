using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence;

internal sealed class EfRepository<TRoot, TId>(OrganizationDbContext dbContext) : IRepository<TRoot, TId>
    where TRoot : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    public async Task<TRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken) =>
        await dbContext.Set<TRoot>().FindAsync([id], cancellationToken);

    public async Task AddAsync(TRoot entity, CancellationToken cancellationToken) =>
        await dbContext.Set<TRoot>().AddAsync(entity, cancellationToken);
}
