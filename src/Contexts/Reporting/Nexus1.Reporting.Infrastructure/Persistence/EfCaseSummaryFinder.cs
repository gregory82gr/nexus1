using Microsoft.EntityFrameworkCore;
using Nexus1.Reporting.Application;

namespace Nexus1.Reporting.Infrastructure.Persistence;

internal sealed class EfCaseSummaryFinder(ReportingDbContext dbContext) : ICaseSummaryFinder
{
    public async Task<IReadOnlyList<CaseSummaryDto>> GetCaseSummariesForUnitAsync(int unitId, CancellationToken cancellationToken) =>
        await dbContext.CaseSummaries
            .Where(s => s.UnitId == unitId)
            .OrderByDescending(s => s.OpenedAtUtc)
            .Select(s => new CaseSummaryDto(
                s.Id.Value, s.UnitId, s.AlarmFloodId, s.Status.ToString(), s.Verdict, s.OpenedAtUtc, s.VerdictIssuedAtUtc))
            .ToListAsync(cancellationToken);
}
