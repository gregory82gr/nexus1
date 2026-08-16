using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Organization.Domain;

/// <summary>
/// Working team or crew, including shift crews and temporary mission teams
/// (atlas C.3.4.4). IsShiftTeam/IsEmergencyTeam are real flags with no
/// attached workflow in this scope — no shift/emergency workflow exists
/// here to act on them (ADR-017's "flag it, don't wire it" restraint).
/// </summary>
public sealed class Team : Entity<TeamId>, IAggregateRoot
{
    private Team(
        TeamId id, DepartmentId departmentId, TeamTypeId teamTypeId, string code, string name, bool isShiftTeam,
        bool isEmergencyTeam, DateTime createdAtUtc)
        : base(id)
    {
        DepartmentId = departmentId;
        TeamTypeId = teamTypeId;
        Code = code;
        Name = name;
        IsShiftTeam = isShiftTeam;
        IsEmergencyTeam = isEmergencyTeam;
        CreatedAtUtc = createdAtUtc;
    }

    public DepartmentId DepartmentId { get; }

    public TeamTypeId TeamTypeId { get; }

    public string Code { get; }

    public string Name { get; }

    public bool IsShiftTeam { get; }

    public bool IsEmergencyTeam { get; }

    public DateTime CreatedAtUtc { get; }

    public static Team Create(
        TeamId id, DepartmentId departmentId, TeamTypeId teamTypeId, string code, string name, DateTime createdAtUtc,
        bool isShiftTeam = false, bool isEmergencyTeam = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Team code must not be empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Team name must not be empty.", nameof(name));
        }

        return new Team(id, departmentId, teamTypeId, code, name, isShiftTeam, isEmergencyTeam, createdAtUtc);
    }
}
