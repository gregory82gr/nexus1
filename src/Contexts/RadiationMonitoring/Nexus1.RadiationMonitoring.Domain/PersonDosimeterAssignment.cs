using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RadiationMonitoring.Domain;

/// <summary>
/// Ties personal exposure to an assignment window rather than a loose
/// person id (ADR-024's own design statement). DosimeterId is a real
/// internal FK and NOT NULL. PersonId is passport-only —
/// Organization.Person lives in OrganizationDb, a different physical
/// database — but stays NOT NULL as a plain column, a business requirement
/// without a database-enforced constraint. AssignedByUserId is
/// passport-only — Security.ApplicationUser, a different physical database
/// — and nullable.
///
/// ReturnedAtUtc &gt; AssignedAtUtc (when present) is enforced as a SQL
/// CHECK constraint at the EF configuration layer, not in Domain (ADR-024).
///
/// No audit columns beyond row-insertion bookkeeping — deliberately not
/// modeled, matching the "don't invent an audit trail feature" restraint
/// (ADR-024).
/// </summary>
public sealed class PersonDosimeterAssignment : Entity<PersonDosimeterAssignmentId>, IAggregateRoot
{
    private PersonDosimeterAssignment(
        PersonDosimeterAssignmentId id, int personId, DosimeterId dosimeterId, int? assignedByUserId,
        DateTime assignedAtUtc, DateTime? returnedAtUtc, string? assignmentPurpose)
        : base(id)
    {
        PersonId = personId;
        DosimeterId = dosimeterId;
        AssignedByUserId = assignedByUserId;
        AssignedAtUtc = assignedAtUtc;
        ReturnedAtUtc = returnedAtUtc;
        AssignmentPurpose = assignmentPurpose;
    }

    /// <summary>Passport-only — Organization.Person, a different physical database (ADR-024). NOT NULL business requirement, no enforced FK.</summary>
    public int PersonId { get; }

    public DosimeterId DosimeterId { get; }

    /// <summary>Passport-only — Security.ApplicationUser, a different physical database (ADR-024).</summary>
    public int? AssignedByUserId { get; }

    public DateTime AssignedAtUtc { get; }

    public DateTime? ReturnedAtUtc { get; }

    public string? AssignmentPurpose { get; }

    public static PersonDosimeterAssignment Create(
        PersonDosimeterAssignmentId id, int personId, DosimeterId dosimeterId, DateTime assignedAtUtc,
        int? assignedByUserId = null, DateTime? returnedAtUtc = null, string? assignmentPurpose = null)
    {
        return new PersonDosimeterAssignment(
            id, personId, dosimeterId, assignedByUserId, assignedAtUtc, returnedAtUtc, assignmentPurpose);
    }
}
