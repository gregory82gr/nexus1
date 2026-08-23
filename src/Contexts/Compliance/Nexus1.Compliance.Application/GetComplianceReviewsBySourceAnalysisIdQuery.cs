using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Compliance.Application;

public sealed record GetComplianceReviewsBySourceAnalysisIdQuery(long SourceAnalysisId) : IQuery<IReadOnlyList<ComplianceReviewDto>>;
