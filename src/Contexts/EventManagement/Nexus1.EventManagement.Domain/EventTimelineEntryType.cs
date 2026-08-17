using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>Classifies timeline entries: detected, alarmed, acknowledged, action, decision, handoff, closure (atlas C.8.3). Referenced by EventTimelineEntry (NOT NULL).</summary>
public sealed class EventTimelineEntryType : Entity<EventTimelineEntryTypeId>, IAggregateRoot
{
    private EventTimelineEntryType(EventTimelineEntryTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static EventTimelineEntryType Create(
        EventTimelineEntryTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EventTimelineEntryType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EventTimelineEntryType name must not be empty.", nameof(name));
        }

        return new EventTimelineEntryType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
