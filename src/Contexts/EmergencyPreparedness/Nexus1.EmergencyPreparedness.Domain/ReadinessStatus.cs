using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>Classifies ResourceReadinessCheck outcome (ADR-025). Referenced by ResourceReadinessCheck (NOT NULL).</summary>
public sealed class ReadinessStatus : Entity<ReadinessStatusId>, IAggregateRoot
{
    private ReadinessStatus(ReadinessStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static ReadinessStatus Create(
        ReadinessStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ReadinessStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ReadinessStatus name must not be empty.", nameof(name));
        }

        return new ReadinessStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
