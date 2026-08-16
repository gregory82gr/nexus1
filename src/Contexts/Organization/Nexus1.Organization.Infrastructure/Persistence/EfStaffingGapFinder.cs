using Microsoft.EntityFrameworkCore;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.3.8 query 3: for a scenario, the gaps from its most recent result by EvaluatedAtUtc.</summary>
internal sealed class EfStaffingGapFinder(OrganizationDbContext dbContext) : IStaffingGapFinder
{
    public async Task<IReadOnlyList<StaffingScenarioGapDto>> GetLatestGapsAsync(int staffingScenarioId, CancellationToken cancellationToken)
    {
        var scenarioId = new StaffingScenarioId(staffingScenarioId);

        var latestResult = await dbContext.StaffingScenarioResults
            .Where(r => r.StaffingScenarioId == scenarioId)
            .OrderByDescending(r => r.EvaluatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestResult is null)
        {
            return [];
        }

        var latestResultId = latestResult.Id;

        return await dbContext.StaffingScenarioGaps
            .Where(g => g.StaffingScenarioResultId == latestResultId)
            .OrderBy(g => g.PositionId)
            .Select(g => new StaffingScenarioGapDto(g.PositionId.Value, g.RequiredCount, g.AvailableCount, g.GapCount, g.Notes))
            .ToListAsync(cancellationToken);
    }
}
