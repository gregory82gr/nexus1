using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>Classifies operational events: incident, near miss, transient, drill, maintenance handoff, security or digital-twin divergence (atlas C.8.3). Referenced by OperationalEvent (NOT NULL).</summary>
public sealed class EventType : Entity<EventTypeId>, IAggregateRoot
{
    private EventType(EventTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static EventType Create(
        EventTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EventType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EventType name must not be empty.", nameof(name));
        }

        return new EventType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
