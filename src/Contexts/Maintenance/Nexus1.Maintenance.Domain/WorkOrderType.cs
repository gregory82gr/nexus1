using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Corrective, preventive, predictive, inspection, calibration, modification or emergent work (atlas C.9.3). Referenced by WorkOrder (WorkOrderTypeId is NOT NULL there).</summary>
public sealed class WorkOrderType : Entity<WorkOrderTypeId>, IAggregateRoot
{
    private WorkOrderType(WorkOrderTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static WorkOrderType Create(
        WorkOrderTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("WorkOrderType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("WorkOrderType name must not be empty.", nameof(name));
        }

        return new WorkOrderType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
