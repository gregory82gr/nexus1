using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Corrosion, wear, fatigue, thermal cycling, radiation embrittlement, fouling, vibration or obsolescence (atlas C.9.3). Referenced by DegradationRecord.</summary>
public sealed class DegradationMechanism : Entity<DegradationMechanismId>, IAggregateRoot
{
    private DegradationMechanism(DegradationMechanismId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static DegradationMechanism Create(
        DegradationMechanismId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DegradationMechanism code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DegradationMechanism name must not be empty.", nameof(name));
        }

        return new DegradationMechanism(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
