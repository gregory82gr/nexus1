using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Catalogue of skills or authorisations required for roles and staffing scenarios (atlas C.3.4.8).</summary>
public sealed class Qualification : Entity<QualificationId>, IAggregateRoot
{
    private Qualification(
        QualificationId id, string code, string name, string? description, string? issuer, int? validityMonths,
        bool isSafetyCritical, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Issuer = issuer;
        ValidityMonths = validityMonths;
        IsSafetyCritical = isSafetyCritical;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public string? Issuer { get; }

    public int? ValidityMonths { get; }

    public bool IsSafetyCritical { get; }

    public DateTime CreatedAtUtc { get; }

    public static Qualification Create(
        QualificationId id, string code, string name, DateTime createdAtUtc, string? description = null,
        string? issuer = null, int? validityMonths = null, bool isSafetyCritical = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Qualification code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Qualification name must not be empty.", nameof(name));
        }

        if (validityMonths is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(validityMonths), validityMonths, "ValidityMonths must be > 0 when present.");
        }

        return new Qualification(id, code, name, description, issuer, validityMonths, isSafetyCritical, createdAtUtc);
    }
}
