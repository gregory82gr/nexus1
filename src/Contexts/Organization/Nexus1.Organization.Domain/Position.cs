using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>A job or responsibility position: reactor operator, shift supervisor, radiation technician, maintenance lead (atlas C.3.4.4).</summary>
public sealed class Position : Entity<PositionId>, IAggregateRoot
{
    private Position(
        PositionId id, DepartmentId? departmentId, string code, string title, string? description,
        bool isSafetyCritical, bool requiresShiftWork, DateTime createdAtUtc)
        : base(id)
    {
        DepartmentId = departmentId;
        Code = code;
        Title = title;
        Description = description;
        IsSafetyCritical = isSafetyCritical;
        RequiresShiftWork = requiresShiftWork;
        CreatedAtUtc = createdAtUtc;
    }

    public DepartmentId? DepartmentId { get; }

    public string Code { get; }

    public string Title { get; }

    public string? Description { get; }

    public bool IsSafetyCritical { get; }

    public bool RequiresShiftWork { get; }

    public DateTime CreatedAtUtc { get; }

    public static Position Create(
        PositionId id, string code, string title, DateTime createdAtUtc, DepartmentId? departmentId = null,
        string? description = null, bool isSafetyCritical = false, bool requiresShiftWork = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Position code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Position title must not be empty.", nameof(title));
        }

        return new Position(id, departmentId, code, title, description, isSafetyCritical, requiresShiftWork, createdAtUtc);
    }
}
