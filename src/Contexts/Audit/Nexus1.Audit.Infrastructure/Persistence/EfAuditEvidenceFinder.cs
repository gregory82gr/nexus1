using Microsoft.EntityFrameworkCore;
using Nexus1.Audit.Application;

namespace Nexus1.Audit.Infrastructure.Persistence;

internal sealed class EfAuditEvidenceFinder(AuditDbContext dbContext) : IAuditEvidenceFinder
{
    public async Task<IReadOnlyList<AuditEvidenceRecordDto>> GetBySourceAnalysisIdAsync(long sourceAnalysisId, CancellationToken cancellationToken)
    {
        // Convert.ToHexString doesn't translate to SQL, so materialize the
        // entities first and project to the DTO (with the hex conversion)
        // in memory, not inside the EF query itself.
        var records = await dbContext.Evidence
            .Where(e => e.SourceAnalysisId == sourceAnalysisId)
            .OrderBy(e => e.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        return records
            .Select(e => new AuditEvidenceRecordDto(
                e.Id.Value, e.SourceAnalysisId, e.EventType, e.SchemaVersion, e.CorrelationId, e.CausationId,
                Convert.ToHexString(e.EnvelopeSha256), e.OccurredAtUtc, e.RecordedAtUtc))
            .ToList();
    }
}
