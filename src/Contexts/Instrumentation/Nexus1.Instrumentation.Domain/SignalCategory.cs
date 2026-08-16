using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>Domain category such as power, pressure, temperature, flow, radiation, vibration, chemistry or electrical (atlas C.5.3).</summary>
public sealed class SignalCategory : Entity<SignalCategoryId>, IAggregateRoot
{
    private SignalCategory(SignalCategoryId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static SignalCategory Create(
        SignalCategoryId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("SignalCategory code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("SignalCategory name must not be empty.", nameof(name));
        }

        return new SignalCategory(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
