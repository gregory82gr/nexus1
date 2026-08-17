using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.UnitTests;

public class PersonDosimeterAssignmentTests
{
    private static readonly DateTime AssignedAtUtc = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_null()
    {
        var assignment = PersonDosimeterAssignment.Create(
            new PersonDosimeterAssignmentId(1), personId: 101, new DosimeterId(1), AssignedAtUtc);

        Assert.Equal(101, assignment.PersonId);
        Assert.Equal(AssignedAtUtc, assignment.AssignedAtUtc);
        Assert.Null(assignment.AssignedByUserId);
        Assert.Null(assignment.ReturnedAtUtc);
        Assert.Null(assignment.AssignmentPurpose);
    }

    [Fact]
    public void Create_with_passport_only_person_and_user_ids_sets_both_with_no_enforced_fk()
    {
        var assignment = PersonDosimeterAssignment.Create(
            new PersonDosimeterAssignmentId(1), personId: 101, new DosimeterId(1), AssignedAtUtc,
            assignedByUserId: 7, assignmentPurpose: "Outage work");

        Assert.Equal(101, assignment.PersonId);
        Assert.Equal(7, assignment.AssignedByUserId);
        Assert.Equal("Outage work", assignment.AssignmentPurpose);
    }

    [Fact]
    public void Create_with_returned_after_assigned_sets_returned_at_utc()
    {
        var returnedAtUtc = AssignedAtUtc.AddDays(30);

        var assignment = PersonDosimeterAssignment.Create(
            new PersonDosimeterAssignmentId(1), personId: 101, new DosimeterId(1), AssignedAtUtc,
            returnedAtUtc: returnedAtUtc);

        Assert.Equal(returnedAtUtc, assignment.ReturnedAtUtc);
    }
}
