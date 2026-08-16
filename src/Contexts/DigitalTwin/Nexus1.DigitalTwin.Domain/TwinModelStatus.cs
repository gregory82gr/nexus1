using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>Lifecycle of a twin model: draft, validating, active, retired, superseded, failed (atlas C.6.2).</summary>
public sealed class TwinModelStatus : Entity<TwinModelStatusId>, IAggregateRoot
{
    private TwinModelStatus(TwinModelStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static TwinModelStatus Create(
        TwinModelStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("TwinModelStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TwinModelStatus name must not be empty.", nameof(name));
        }

        return new TwinModelStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
