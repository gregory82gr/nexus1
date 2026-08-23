using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Audit.Application;

public sealed class GetAuditEvidenceBySourceAnalysisIdQueryHandler(IAuditEvidenceFinder finder)
    : IQueryHandler<GetAuditEvidenceBySourceAnalysisIdQuery, IReadOnlyList<AuditEvidenceRecordDto>>
{
    public async Task<Result<IReadOnlyList<AuditEvidenceRecordDto>>> Handle(GetAuditEvidenceBySourceAnalysisIdQuery query, CancellationToken cancellationToken)
    {
        var records = await finder.GetBySourceAnalysisIdAsync(query.SourceAnalysisId, cancellationToken);
        return Result<IReadOnlyList<AuditEvidenceRecordDto>>.Success(records);
    }
}
