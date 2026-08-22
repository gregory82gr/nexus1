using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Priority used by the planner: routine, low, normal, high, urgent, outage-critical (atlas C.9.3). Referenced by WorkOrder.</summary>
public sealed class WorkPriority : Entity<WorkPriorityId>, IAggregateRoot
{
    private WorkPriority(WorkPriorityId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static WorkPriority Create(
        WorkPriorityId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("WorkPriority code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("WorkPriority name must not be empty.", nameof(name));
        }

        return new WorkPriority(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
