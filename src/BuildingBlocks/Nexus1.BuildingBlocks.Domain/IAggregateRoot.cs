namespace Nexus1.BuildingBlocks.Domain;

/// <summary>
/// Marker for aggregate roots — constrains IRepository&lt;TRoot,TId&gt; in
/// Nexus1.BuildingBlocks.Application (ADR-002-amend) to only load consistency
/// boundaries, never child entities directly.
/// </summary>
public interface IAggregateRoot;
