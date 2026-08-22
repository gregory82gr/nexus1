using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Organization.Application;

/// <summary>Writes one StaffingScenarioResult plus its StaffingScenarioGap rows in a single operation (atlas C.3.4.9).</summary>
public sealed record RecordStaffingScenarioResultCommand(
    int StaffingScenarioId, DateTime EvaluatedAtUtc, string OverallStatus, IReadOnlyList<StaffingGapRequest> Gaps,
    int? EvaluatedByUserId = null, string? Summary = null)
    : ICommand<int>;
