using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// Time-bounded assignment of a person to a department and position (atlas
/// C.3.4.6). Time-bounding avoids overwriting history when a person changes
/// department or position — real behavior, not a passthrough: EndDate must
/// never precede StartDate, enforced both at creation and on close-out.
/// </summary>
public sealed class DepartmentAssignment : Entity<DepartmentAssignmentId>, IAggregateRoot
{
    private DepartmentAssignment(
        DepartmentAssignmentId id, PersonId personId, DepartmentId departmentId, PositionId? positionId,
        DateOnly startDate, DateOnly? endDate, bool isPrimary, DateTime createdAtUtc)
        : base(id)
    {
        PersonId = personId;
        DepartmentId = departmentId;
        PositionId = positionId;
        StartDate = startDate;
        EndDate = endDate;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc;
    }

    public PersonId PersonId { get; }

    public DepartmentId DepartmentId { get; }

    public PositionId? PositionId { get; }

    public DateOnly StartDate { get; }

    public DateOnly? EndDate { get; private set; }

    public bool IsPrimary { get; }

    public DateTime CreatedAtUtc { get; }

    public static DepartmentAssignment Create(
        DepartmentAssignmentId id, PersonId personId, DepartmentId departmentId, DateOnly startDate,
        DateTime createdAtUtc, PositionId? positionId = null, DateOnly? endDate = null, bool isPrimary = false)
    {
        if (endDate is { } end && end < startDate)
        {
            throw new ArgumentException("EndDate must not be earlier than StartDate.", nameof(endDate));
        }

        return new DepartmentAssignment(id, personId, departmentId, positionId, startDate, endDate, isPrimary, createdAtUtc);
    }

    /// <summary>Closes out the assignment — re-validates the same date-range invariant enforced at creation.</summary>
    public void End(DateOnly endDate)
    {
        if (endDate < StartDate)
        {
            throw new ArgumentException("EndDate must not be earlier than StartDate.", nameof(endDate));
        }

        EndDate = endDate;
    }
}
