using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>Classifies Dosimeter by device kind (ADR-024). Referenced by Dosimeter (NOT NULL).</summary>
public sealed class DosimeterType : Entity<DosimeterTypeId>, IAggregateRoot
{
    private DosimeterType(DosimeterTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static DosimeterType Create(
        DosimeterTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DosimeterType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DosimeterType name must not be empty.", nameof(name));
        }

        return new DosimeterType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
