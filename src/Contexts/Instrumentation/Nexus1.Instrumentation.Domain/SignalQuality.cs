using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>Quality state attached to measurements: good, uncertain, bad, stale, substituted, simulated (atlas C.5.3). "A value without quality is not evidence" (C.5.9).</summary>
public sealed class SignalQuality : Entity<SignalQualityId>, IAggregateRoot
{
    private SignalQuality(SignalQualityId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static SignalQuality Create(
        SignalQualityId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("SignalQuality code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("SignalQuality name must not be empty.", nameof(name));
        }

        return new SignalQuality(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
