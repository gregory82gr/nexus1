using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>Radiological area classification for a RadiationZone (ADR-024). Referenced by RadiationZone (NOT NULL).</summary>
public sealed class RadiationAreaClassification : Entity<RadiationAreaClassificationId>, IAggregateRoot
{
    private RadiationAreaClassification(RadiationAreaClassificationId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static RadiationAreaClassification Create(
        RadiationAreaClassificationId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RadiationAreaClassification code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("RadiationAreaClassification name must not be empty.", nameof(name));
        }

        return new RadiationAreaClassification(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
