using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>Lifecycle of an operational event: new, triaged, investigating, actioned, closed, archived (atlas C.8.3). Referenced by OperationalEvent (NOT NULL).</summary>
public sealed class EventStatus : Entity<EventStatusId>, IAggregateRoot
{
    private EventStatus(EventStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static EventStatus Create(
        EventStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EventStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EventStatus name must not be empty.", nameof(name));
        }

        return new EventStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
