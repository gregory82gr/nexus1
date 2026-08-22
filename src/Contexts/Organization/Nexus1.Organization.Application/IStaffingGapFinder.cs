namespace Nexus1.Organization.Application;

/// <summary>Matches the atlas's own C.3.8 query 3: for a scenario, the gaps from its most recent result by EvaluatedAtUtc.</summary>
public interface IStaffingGapFinder
{
    Task<IReadOnlyList<StaffingScenarioGapDto>> GetLatestGapsAsync(int staffingScenarioId, CancellationToken cancellationToken);
}
