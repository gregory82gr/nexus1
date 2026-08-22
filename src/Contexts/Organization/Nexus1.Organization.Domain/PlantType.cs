using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed catalogue of plant-level installation kinds (atlas C.3.3): nuclear site, simulator plant, training plant, support plant.</summary>
public sealed class PlantType : Entity<PlantTypeId>, IAggregateRoot
{
    private PlantType(PlantTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static PlantType Create(
        PlantTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("PlantType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("PlantType name must not be empty.", nameof(name));
        }

        return new PlantType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
