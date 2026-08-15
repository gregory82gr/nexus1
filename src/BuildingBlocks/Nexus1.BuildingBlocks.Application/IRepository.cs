using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// Constrained to IAggregateRoot so repositories only ever load consistency
/// boundaries, never child entities directly (Blueprint_to_Core, ADR-002-amend).
/// Add/Get only — mutation happens via loaded aggregates' own methods, commit
/// happens via IUnitOfWork, matching the book's split Add/Save repository shape.
/// </summary>
public interface IRepository<TRoot, in TId>
    where TRoot : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    Task<TRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken);

    Task AddAsync(TRoot entity, CancellationToken cancellationToken);
}
