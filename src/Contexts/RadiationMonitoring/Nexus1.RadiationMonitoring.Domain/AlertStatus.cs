using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>Lifecycle status of a DoseAlert (ADR-024: OPEN, ACKNOWLEDGED among other codes). Referenced by DoseAlert (NOT NULL).</summary>
public sealed class AlertStatus : Entity<AlertStatusId>, IAggregateRoot
{
    private AlertStatus(AlertStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static AlertStatus Create(
        AlertStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("AlertStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("AlertStatus name must not be empty.", nameof(name));
        }

        return new AlertStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
