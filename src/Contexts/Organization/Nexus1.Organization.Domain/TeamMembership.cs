using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// Time-bounded membership of a person in a team or shift crew (atlas
/// C.3.4.6). Mirrors DepartmentAssignment's own real behavior: EndDate must
/// never precede StartDate, enforced both at creation and on close-out.
/// </summary>
public sealed class TeamMembership : Entity<TeamMembershipId>, IAggregateRoot
{
    private TeamMembership(
        TeamMembershipId id, PersonId personId, TeamId teamId, PositionId? positionId, DateOnly startDate,
        DateOnly? endDate, bool isLead, DateTime createdAtUtc)
        : base(id)
    {
        PersonId = personId;
        TeamId = teamId;
        PositionId = positionId;
        StartDate = startDate;
        EndDate = endDate;
        IsLead = isLead;
        CreatedAtUtc = createdAtUtc;
    }

    public PersonId PersonId { get; }

    public TeamId TeamId { get; }

    public PositionId? PositionId { get; }

    public DateOnly StartDate { get; }

    public DateOnly? EndDate { get; private set; }

    public bool IsLead { get; }

    public DateTime CreatedAtUtc { get; }

    public static TeamMembership Create(
        TeamMembershipId id, PersonId personId, TeamId teamId, DateOnly startDate, DateTime createdAtUtc,
        PositionId? positionId = null, DateOnly? endDate = null, bool isLead = false)
    {
        if (endDate is { } end && end < startDate)
        {
            throw new ArgumentException("EndDate must not be earlier than StartDate.", nameof(endDate));
        }

        return new TeamMembership(id, personId, teamId, positionId, startDate, endDate, isLead, createdAtUtc);
    }

    /// <summary>Closes out the membership — re-validates the same date-range invariant enforced at creation.</summary>
    public void End(DateOnly endDate)
    {
        if (endDate < StartDate)
        {
            throw new ArgumentException("EndDate must not be earlier than StartDate.", nameof(endDate));
        }

        EndDate = endDate;
    }
}
