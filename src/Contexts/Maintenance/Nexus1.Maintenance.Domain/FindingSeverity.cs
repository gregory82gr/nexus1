using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Severity assigned to inspection findings and active degradation records (atlas C.9.3). Referenced by DegradationRecord.</summary>
public sealed class FindingSeverity : Entity<FindingSeverityId>, IAggregateRoot
{
    private FindingSeverity(FindingSeverityId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static FindingSeverity Create(
        FindingSeverityId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("FindingSeverity code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("FindingSeverity name must not be empty.", nameof(name));
        }

        return new FindingSeverity(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
