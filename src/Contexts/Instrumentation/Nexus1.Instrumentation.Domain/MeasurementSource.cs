using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>Origin of a value: historian, simulator, operator entry, import, twin backfill, test harness (atlas C.5.3).</summary>
public sealed class MeasurementSource : Entity<MeasurementSourceId>, IAggregateRoot
{
    private MeasurementSource(MeasurementSourceId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static MeasurementSource Create(
        MeasurementSourceId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("MeasurementSource code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("MeasurementSource name must not be empty.", nameof(name));
        }

        return new MeasurementSource(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
