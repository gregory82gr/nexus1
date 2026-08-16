using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed catalogue of legal entity kinds (atlas C.3.3): operator, vendor, regulator, emergency partner, university, service provider.</summary>
public sealed class LegalEntityType : Entity<LegalEntityTypeId>, IAggregateRoot
{
    private LegalEntityType(LegalEntityTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Code { get; }

    public string Name { get; }

    public string? Description { get; }

    public int DisplayOrder { get; }

    public bool IsActive { get; }

    public DateTime CreatedAtUtc { get; }

    public static LegalEntityType Create(
        LegalEntityTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("LegalEntityType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("LegalEntityType name must not be empty.", nameof(name));
        }

        return new LegalEntityType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
