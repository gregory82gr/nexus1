using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>Mission dispatch priority (ADR-023). Referenced by Mission (NOT NULL).</summary>
public sealed class MissionPriority : Entity<MissionPriorityId>, IAggregateRoot
{
    private MissionPriority(MissionPriorityId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static MissionPriority Create(
        MissionPriorityId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MissionPriority code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("MissionPriority name must not be empty.", nameof(name));
        }

        return new MissionPriority(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
