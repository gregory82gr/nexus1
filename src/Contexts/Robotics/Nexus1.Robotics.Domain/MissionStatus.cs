using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Robotics.Domain;

/// <summary>Mission lifecycle status (ADR-023). Referenced by Mission (NOT NULL).</summary>
public sealed class MissionStatus : Entity<MissionStatusId>, IAggregateRoot
{
    private MissionStatus(MissionStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static MissionStatus Create(
        MissionStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MissionStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("MissionStatus name must not be empty.", nameof(name));
        }

        return new MissionStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
