using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>Classifies incidents: operational, equipment, safety, radiation, security, cyber, environmental, training (atlas C.8.3). Referenced by Incident (NOT NULL).</summary>
public sealed class IncidentType : Entity<IncidentTypeId>, IAggregateRoot
{
    private IncidentType(IncidentTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static IncidentType Create(
        IncidentTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("IncidentType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("IncidentType name must not be empty.", nameof(name));
        }

        return new IncidentType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
