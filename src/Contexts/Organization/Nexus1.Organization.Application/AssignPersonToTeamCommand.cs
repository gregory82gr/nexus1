using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>TeamMembership's defining behavior (atlas C.3.4.6).</summary>
public sealed record AssignPersonToTeamCommand(
    int PersonId, int TeamId, DateOnly StartDate, int? PositionId = null, bool IsLead = false)
    : ICommand<int>;
