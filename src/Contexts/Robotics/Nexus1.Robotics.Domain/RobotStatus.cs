using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>Robot lifecycle status (ADR-023: AVAILABLE among other codes). Referenced by Robot (NOT NULL).</summary>
public sealed class RobotStatus : Entity<RobotStatusId>, IAggregateRoot
{
    private RobotStatus(RobotStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static RobotStatus Create(
        RobotStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RobotStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RobotStatus name must not be empty.", nameof(name));
        }

        return new RobotStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
