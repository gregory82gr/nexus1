using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// The company, operator, vendor, regulator, or partner that owns sites,
/// employs people, or provides services (atlas C.3.4.3). CountryId is a
/// plain passport int, not a real FK: CorePlatform lives in
/// AlarmManagementDb while Organization gets its own OrganizationDb
/// (ADR-017), so a real cross-database FOREIGN KEY is not possible.
/// </summary>
public sealed class LegalEntity : Entity<LegalEntityId>, IAggregateRoot
{
    private LegalEntity(
        LegalEntityId id, LegalEntityTypeId legalEntityTypeId, LegalEntityId? parentLegalEntityId, int? countryId,
        string code, string name, string? registrationNumber, string? taxIdentifier, string? websiteUrl,
        bool isOperator, bool isVendor, DateTime createdAtUtc)
        : base(id)
    {
        LegalEntityTypeId = legalEntityTypeId;
        ParentLegalEntityId = parentLegalEntityId;
        CountryId = countryId;
        Code = code;
        Name = name;
        RegistrationNumber = registrationNumber;
        TaxIdentifier = taxIdentifier;
        WebsiteUrl = websiteUrl;
        IsOperator = isOperator;
        IsVendor = isVendor;
        CreatedAtUtc = createdAtUtc;
    }

    public LegalEntityTypeId LegalEntityTypeId { get; }

    public LegalEntityId? ParentLegalEntityId { get; }

    /// <summary>CorePlatform.Country passport id — no enforced FK across the OrganizationDb/AlarmManagementDb boundary (ADR-017).</summary>
    public int? CountryId { get; }

    public string Code { get; }

    public string Name { get; }

    public string? RegistrationNumber { get; }

    public string? TaxIdentifier { get; }

    public string? WebsiteUrl { get; }

    public bool IsOperator { get; }

    public bool IsVendor { get; }

    public DateTime CreatedAtUtc { get; }

    public static LegalEntity Create(
        LegalEntityId id, LegalEntityTypeId legalEntityTypeId, string code, string name, DateTime createdAtUtc,
        LegalEntityId? parentLegalEntityId = null, int? countryId = null, string? registrationNumber = null,
        string? taxIdentifier = null, string? websiteUrl = null, bool isOperator = false, bool isVendor = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("LegalEntity code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("LegalEntity name must not be empty.", nameof(name));
        }

        return new LegalEntity(
            id, legalEntityTypeId, parentLegalEntityId, countryId, code, name, registrationNumber, taxIdentifier,
            websiteUrl, isOperator, isVendor, createdAtUtc);
    }
}
