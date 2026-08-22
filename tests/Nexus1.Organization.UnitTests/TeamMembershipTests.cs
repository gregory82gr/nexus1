using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class TeamMembershipTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly StartDate = new(2026, 1, 1);

    [Fact]
    public void Create_without_end_date_succeeds()
    {
        var membership = TeamMembership.Create(new TeamMembershipId(1), new PersonId(1), new TeamId(1), StartDate, NowUtc);
        Assert.Null(membership.EndDate);
    }

    [Fact]
    public void Create_with_end_date_before_start_date_throws()
    {
        Assert.Throws<ArgumentException>(() => TeamMembership.Create(
            new TeamMembershipId(1), new PersonId(1), new TeamId(1), StartDate, NowUtc, endDate: StartDate.AddDays(-1)));
    }

    [Fact]
    public void End_with_valid_date_closes_the_membership()
    {
        var membership = TeamMembership.Create(new TeamMembershipId(1), new PersonId(1), new TeamId(1), StartDate, NowUtc);

        membership.End(StartDate.AddMonths(3));

        Assert.Equal(StartDate.AddMonths(3), membership.EndDate);
    }

    [Fact]
    public void End_with_date_before_start_date_throws()
    {
        var membership = TeamMembership.Create(new TeamMembershipId(1), new PersonId(1), new TeamId(1), StartDate, NowUtc);

        Assert.Throws<ArgumentException>(() => membership.End(StartDate.AddDays(-1)));
    }

    [Fact]
    public void Create_with_lead_flag_and_position_succeeds()
    {
        var membership = TeamMembership.Create(
            new TeamMembershipId(1), new PersonId(1), new TeamId(1), StartDate, NowUtc,
            positionId: new PositionId(5), isLead: true);

        Assert.True(membership.IsLead);
        Assert.Equal(new PositionId(5), membership.PositionId);
    }
}
