using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EmergencyPreparedness.Domain;

/// <summary>Classifies EmergencyResource by kind (ADR-025). Referenced by EmergencyResource (NOT NULL).</summary>
public sealed class ResourceType : Entity<ResourceTypeId>, IAggregateRoot
{
    private ResourceType(ResourceTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static ResourceType Create(
        ResourceTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ResourceType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ResourceType name must not be empty.", nameof(name));
        }

        return new ResourceType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
