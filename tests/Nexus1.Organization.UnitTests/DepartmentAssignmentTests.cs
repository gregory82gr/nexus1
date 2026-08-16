using Nexus1.Organization.Domain;

namespace Nexus1.Organization.UnitTests;

public class DepartmentAssignmentTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly StartDate = new(2026, 1, 1);

    [Fact]
    public void Create_without_end_date_succeeds()
    {
        var assignment = DepartmentAssignment.Create(
            new DepartmentAssignmentId(1), new PersonId(1), new DepartmentId(1), StartDate, NowUtc);

        Assert.Null(assignment.EndDate);
    }

    [Fact]
    public void Create_with_end_date_on_or_after_start_date_succeeds()
    {
        var assignment = DepartmentAssignment.Create(
            new DepartmentAssignmentId(1), new PersonId(1), new DepartmentId(1), StartDate, NowUtc,
            endDate: StartDate);

        Assert.Equal(StartDate, assignment.EndDate);
    }

    [Fact]
    public void Create_with_end_date_before_start_date_throws()
    {
        Assert.Throws<ArgumentException>(() => DepartmentAssignment.Create(
            new DepartmentAssignmentId(1), new PersonId(1), new DepartmentId(1), StartDate, NowUtc,
            endDate: StartDate.AddDays(-1)));
    }

    [Fact]
    public void End_with_valid_date_closes_the_assignment()
    {
        var assignment = DepartmentAssignment.Create(
            new DepartmentAssignmentId(1), new PersonId(1), new DepartmentId(1), StartDate, NowUtc);

        assignment.End(StartDate.AddMonths(6));

        Assert.Equal(StartDate.AddMonths(6), assignment.EndDate);
    }

    [Fact]
    public void End_with_date_before_start_date_throws()
    {
        var assignment = DepartmentAssignment.Create(
            new DepartmentAssignmentId(1), new PersonId(1), new DepartmentId(1), StartDate, NowUtc);

        Assert.Throws<ArgumentException>(() => assignment.End(StartDate.AddDays(-1)));
    }
}
