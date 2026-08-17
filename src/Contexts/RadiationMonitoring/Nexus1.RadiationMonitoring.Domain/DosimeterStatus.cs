using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>Lifecycle status of a Dosimeter (ADR-024). Referenced by Dosimeter (NOT NULL).</summary>
public sealed class DosimeterStatus : Entity<DosimeterStatusId>, IAggregateRoot
{
    private DosimeterStatus(DosimeterStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static DosimeterStatus Create(
        DosimeterStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DosimeterStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DosimeterStatus name must not be empty.", nameof(name));
        }

        return new DosimeterStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
