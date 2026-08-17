using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>Classifies robot platforms (ADR-023). Referenced by RobotModel (NOT NULL).</summary>
public sealed class RobotType : Entity<RobotTypeId>, IAggregateRoot
{
    private RobotType(RobotTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static RobotType Create(
        RobotTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RobotType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RobotType name must not be empty.", nameof(name));
        }

        return new RobotType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
