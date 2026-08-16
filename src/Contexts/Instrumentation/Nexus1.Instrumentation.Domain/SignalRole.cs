using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>Role of the signal in the platform: telemetry, control indication, diagnostic, twin input, RCA witness or training signal (atlas C.5.3).</summary>
public sealed class SignalRole : Entity<SignalRoleId>, IAggregateRoot
{
    private SignalRole(SignalRoleId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static SignalRole Create(
        SignalRoleId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("SignalRole code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("SignalRole name must not be empty.", nameof(name));
        }

        return new SignalRole(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
