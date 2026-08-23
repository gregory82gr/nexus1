using Microsoft.EntityFrameworkCore;
using Nexus1.Compliance.Application;

namespace Nexus1.Compliance.Infrastructure.Persistence;

internal sealed class EfComplianceReviewFinder(ComplianceDbContext dbContext) : IComplianceReviewFinder
{
    public async Task<IReadOnlyList<ComplianceReviewDto>> GetBySourceAnalysisIdAsync(long sourceAnalysisId, CancellationToken cancellationToken) =>
        await dbContext.Reviews
            .Where(r => r.SourceAnalysisId == sourceAnalysisId)
            .OrderBy(r => r.OpenedAtUtc)
            .Select(r => new ComplianceReviewDto(
                r.Id.Value, r.SourceAnalysisId, r.Verdict, r.State.ToString(), r.OpenedAtUtc))
            .ToListAsync(cancellationToken);
}
