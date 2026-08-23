using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Compliance.Application;

public sealed class GetComplianceReviewsBySourceAnalysisIdQueryHandler(IComplianceReviewFinder finder)
    : IQueryHandler<GetComplianceReviewsBySourceAnalysisIdQuery, IReadOnlyList<ComplianceReviewDto>>
{
    public async Task<Result<IReadOnlyList<ComplianceReviewDto>>> Handle(GetComplianceReviewsBySourceAnalysisIdQuery query, CancellationToken cancellationToken)
    {
        var reviews = await finder.GetBySourceAnalysisIdAsync(query.SourceAnalysisId, cancellationToken);
        return Result<IReadOnlyList<ComplianceReviewDto>>.Success(reviews);
    }
}
