using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>Review state of a divergence: open, acknowledged, explained, corrected, waived, closed (atlas C.6.2).</summary>
public sealed class DivergenceStatus : Entity<DivergenceStatusId>, IAggregateRoot
{
    private DivergenceStatus(DivergenceStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static DivergenceStatus Create(
        DivergenceStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DivergenceStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DivergenceStatus name must not be empty.", nameof(name));
        }

        return new DivergenceStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
