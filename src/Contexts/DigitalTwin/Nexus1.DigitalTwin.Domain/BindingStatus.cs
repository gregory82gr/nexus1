using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>Binding lifecycle: proposed, active, disabled, stale, rejected (atlas C.6.2).</summary>
public sealed class BindingStatus : Entity<BindingStatusId>, IAggregateRoot
{
    private BindingStatus(BindingStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static BindingStatus Create(
        BindingStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("BindingStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("BindingStatus name must not be empty.", nameof(name));
        }

        return new BindingStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
