using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// A human person. The person row is intentionally separate from the
/// security user: a person may or may not have a login account, and a
/// service account may have no person behind it (atlas C.3.4.5, verbatim
/// DDL comment). ApplicationUserId is a plain passport int, not a real FK:
/// Security lives in its own SecurityDb while Organization gets its own
/// OrganizationDb (ADR-017), so a real cross-database FOREIGN KEY is not
/// possible.
/// </summary>
public sealed class Person : Entity<PersonId>, IAggregateRoot
{
    private Person(
        PersonId id, PersonTypeId personTypeId, int? applicationUserId, LegalEntityId? legalEntityId,
        string? personnelNumber, string givenName, string familyName, string displayName, string? workEmail,
        string? workPhone, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        PersonTypeId = personTypeId;
        ApplicationUserId = applicationUserId;
        LegalEntityId = legalEntityId;
        PersonnelNumber = personnelNumber;
        GivenName = givenName;
        FamilyName = familyName;
        DisplayName = displayName;
        WorkEmail = workEmail;
        WorkPhone = workPhone;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public PersonTypeId PersonTypeId { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-017). Optional: only set when the person can log in.</summary>
    public int? ApplicationUserId { get; }

    public LegalEntityId? LegalEntityId { get; }

    public string? PersonnelNumber { get; }

    public string GivenName { get; }

    public string FamilyName { get; }

    public string DisplayName { get; }

    public string? WorkEmail { get; }

    public string? WorkPhone { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static Person Create(
        PersonId id, PersonTypeId personTypeId, string givenName, string familyName, string displayName,
        DateTime createdAtUtc, int? applicationUserId = null, LegalEntityId? legalEntityId = null,
        string? personnelNumber = null, string? workEmail = null, string? workPhone = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(givenName))
        {
            throw new ArgumentException("Person given name must not be empty.", nameof(givenName));
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("Person family name must not be empty.", nameof(familyName));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Person display name must not be empty.", nameof(displayName));
        }

        return new Person(
            id, personTypeId, applicationUserId, legalEntityId, personnelNumber, givenName, familyName, displayName,
            workEmail, workPhone, isActive, createdAtUtc);
    }
}
