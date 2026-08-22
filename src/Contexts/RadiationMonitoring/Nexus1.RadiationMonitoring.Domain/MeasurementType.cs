using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>Classifies RadiationReading by measurement kind (ADR-024). Referenced by RadiationReading (NOT NULL).</summary>
public sealed class MeasurementType : Entity<MeasurementTypeId>, IAggregateRoot
{
    private MeasurementType(MeasurementTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static MeasurementType Create(
        MeasurementTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MeasurementType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("MeasurementType name must not be empty.", nameof(name));
        }

        return new MeasurementType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
