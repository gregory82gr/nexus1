namespace Nexus1.BuildingBlocks.Domain;

/// <summary>
/// Base type for entities identified by <typeparamref name="TId"/>, inferred per
/// CLAUDE.md §1: From_Domain_to_Twin calls AddDomainEvent(...) inside aggregate
/// methods but never declares the base type that supplies it (ADR-003).
/// </summary>
public abstract class Entity<TId>
    where TId : notnull
{
    private readonly List<object> _domainEvents = [];

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; }

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(object domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other
            && other.GetType() == GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
