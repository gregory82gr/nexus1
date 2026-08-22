using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>How the event was created: alarm flood, manual report, drill, import, digital twin, maintenance or security (atlas C.8.3). Referenced by OperationalEvent (NOT NULL) - easy to miss from the abbreviated table-list summary, confirmed against the real DDL (ADR-022).</summary>
public sealed class EventSourceType : Entity<EventSourceTypeId>, IAggregateRoot
{
    private EventSourceType(EventSourceTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DisplayOrder { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static EventSourceType Create(
        EventSourceTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EventSourceType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EventSourceType name must not be empty.", nameof(name));
        }

        return new EventSourceType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
