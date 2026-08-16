using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed lifecycle catalogue for employment and engagements (atlas C.3.3): active, inactive, suspended, retired, ended.</summary>
public sealed class EmploymentStatus : Entity<EmploymentStatusId>, IAggregateRoot
{
    private EmploymentStatus(EmploymentStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static EmploymentStatus Create(
        EmploymentStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("EmploymentStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("EmploymentStatus name must not be empty.", nameof(name));
        }

        return new EmploymentStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
