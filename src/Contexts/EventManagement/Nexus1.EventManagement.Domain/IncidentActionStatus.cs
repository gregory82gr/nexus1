using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.EventManagement.Domain;

/// <summary>Action state: proposed, assigned, in progress, completed, verified, cancelled, overdue (atlas C.8.3). Referenced by IncidentAction (NOT NULL); GetOpenIncidentActionsQuery filters on Code NOT IN (COMPLETED, VERIFIED, CANCELLED).</summary>
public sealed class IncidentActionStatus : Entity<IncidentActionStatusId>, IAggregateRoot
{
    private IncidentActionStatus(IncidentActionStatusId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static IncidentActionStatus Create(
        IncidentActionStatusId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("IncidentActionStatus code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("IncidentActionStatus name must not be empty.", nameof(name));
        }

        return new IncidentActionStatus(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
