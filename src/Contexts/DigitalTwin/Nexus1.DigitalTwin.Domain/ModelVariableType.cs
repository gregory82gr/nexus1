using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>Classifies model variables: state, input, output, derived, diagnostic, safety-margin (atlas C.6.2).</summary>
public sealed class ModelVariableType : Entity<ModelVariableTypeId>, IAggregateRoot
{
    private ModelVariableType(ModelVariableTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static ModelVariableType Create(
        ModelVariableTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("ModelVariableType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("ModelVariableType name must not be empty.", nameof(name));
        }

        return new ModelVariableType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
