using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Audit.Application;

public sealed record GetAuditEvidenceBySourceAnalysisIdQuery(long SourceAnalysisId) : IQuery<IReadOnlyList<AuditEvidenceRecordDto>>;
