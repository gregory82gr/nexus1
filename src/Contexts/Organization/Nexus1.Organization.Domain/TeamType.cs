using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>Typed catalogue of team kinds (atlas C.3.3): shift crew, maintenance crew, emergency team, robotics team, audit team, contractor team.</summary>
public sealed class TeamType : Entity<TeamTypeId>, IAggregateRoot
{
    private TeamType(TeamTypeId id, string code, string name, string? description, int displayOrder, bool isActive, DateTime createdAtUtc)
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

    public static TeamType Create(
        TeamTypeId id, string code, string name, DateTime createdAtUtc,
        string? description = null, int displayOrder = 0, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("TeamType code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TeamType name must not be empty.", nameof(name));
        }

        return new TeamType(id, code, name, description, displayOrder, isActive, createdAtUtc);
    }
}
