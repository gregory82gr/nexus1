using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Draft, requested, approved, planned, in progress, blocked, complete, closed or cancelled (atlas C.9.3). Referenced by WorkOrder.</summary>
public sealed class WorkOrderStatus : Entity<WorkOrderStatusId>, IAggregateRoot
{
    private WorkOrderStatus(WorkOrderStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static WorkOrderStatus Create(
        WorkOrderStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("WorkOrderStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("WorkOrderStatus name must not be empty.", nameof(name));
        }

        return new WorkOrderStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
