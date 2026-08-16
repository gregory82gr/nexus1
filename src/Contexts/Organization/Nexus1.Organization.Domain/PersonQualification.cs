using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// A person holding a qualification, with issue, expiry, and verification
/// (atlas C.3.4.8: "dated, verifiable rows"). VerifiedByUserId is a plain
/// passport int, not a real FK: Security lives in its own SecurityDb while
/// Organization gets its own OrganizationDb (ADR-017), so a real
/// cross-database FOREIGN KEY is not possible.
/// </summary>
public sealed class PersonQualification : Entity<PersonQualificationId>, IAggregateRoot
{
    private PersonQualification(
        PersonQualificationId id, PersonId personId, QualificationId qualificationId,
        QualificationStatusId qualificationStatusId, DateTime? issuedAtUtc, DateTime? expiresAtUtc,
        DateTime? verifiedAtUtc, int? verifiedByUserId, string? notes, DateTime createdAtUtc)
        : base(id)
    {
        PersonId = personId;
        QualificationId = qualificationId;
        QualificationStatusId = qualificationStatusId;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        VerifiedAtUtc = verifiedAtUtc;
        VerifiedByUserId = verifiedByUserId;
        Notes = notes;
        CreatedAtUtc = createdAtUtc;
    }

    public PersonId PersonId { get; }

    public QualificationId QualificationId { get; }

    public QualificationStatusId QualificationStatusId { get; }

    public DateTime? IssuedAtUtc { get; }

    public DateTime? ExpiresAtUtc { get; }

    public DateTime? VerifiedAtUtc { get; private set; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-017).</summary>
    public int? VerifiedByUserId { get; private set; }

    public string? Notes { get; }

    public DateTime CreatedAtUtc { get; }

    public static PersonQualification Create(
        PersonQualificationId id, PersonId personId, QualificationId qualificationId,
        QualificationStatusId qualificationStatusId, DateTime createdAtUtc, DateTime? issuedAtUtc = null,
        DateTime? expiresAtUtc = null, string? notes = null)
    {
        if (expiresAtUtc is { } expires && issuedAtUtc is { } issued && expires <= issued)
        {
            throw new ArgumentException("ExpiresAtUtc must be later than IssuedAtUtc when both are present.", nameof(expiresAtUtc));
        }

        return new PersonQualification(
            id, personId, qualificationId, qualificationStatusId, issuedAtUtc, expiresAtUtc, null, null, notes, createdAtUtc);
    }

    /// <summary>Records human verification of this qualification row (atlas C.3.4.8's "verification" facet).</summary>
    public void Verify(int verifiedByUserId, DateTime verifiedAtUtc)
    {
        VerifiedByUserId = verifiedByUserId;
        VerifiedAtUtc = verifiedAtUtc;
    }
}
