using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Maintenance.Domain;

/// <summary>Condition grade from excellent to failed (A..E, FAILED). Used by AssetCondition and DegradationRecord (atlas C.9.3).</summary>
public sealed class ConditionGrade : Entity<ConditionGradeId>, IAggregateRoot
{
    private ConditionGrade(ConditionGradeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static ConditionGrade Create(
        ConditionGradeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ConditionGrade code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ConditionGrade name must not be empty.", nameof(name));
        }

        return new ConditionGrade(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
