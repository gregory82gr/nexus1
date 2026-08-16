using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed catalogue of department kinds (atlas C.3.3): operations, maintenance, engineering, safety, radiation protection, security, administration.</summary>
public sealed class DepartmentType : Entity<DepartmentTypeId>, IAggregateRoot
{
    private DepartmentType(DepartmentTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static DepartmentType Create(
        DepartmentTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("DepartmentType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DepartmentType name must not be empty.", nameof(name));
        }

        return new DepartmentType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
