using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>DepartmentAssignment's defining behavior (atlas C.3.4.6).</summary>
public sealed record AssignPersonToDepartmentCommand(
    int PersonId, int DepartmentId, DateOnly StartDate, int? PositionId = null, bool IsPrimary = false)
    : ICommand<int>;
