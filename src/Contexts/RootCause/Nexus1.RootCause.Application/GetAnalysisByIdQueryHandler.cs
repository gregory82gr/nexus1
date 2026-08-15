using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain;

namespace Nexus1.RootCause.Application;

public sealed class GetAnalysisByIdQueryHandler(IRepository<RootCauseAnalysis, RootCauseAnalysisId> repository)
    : IQueryHandler<GetAnalysisByIdQuery, AnalysisDto?>
{
    public async Task<Result<AnalysisDto?>> Handle(GetAnalysisByIdQuery query, CancellationToken cancellationToken)
    {
        var analysis = await repository.GetByIdAsync(new RootCauseAnalysisId(query.AnalysisId), cancellationToken);
        if (analysis is null)
        {
            return Result<AnalysisDto?>.Success(null);
        }

        var hypotheses = analysis.Hypotheses
            .Select(h => new HypothesisDto(h.Id.Value, h.HypothesisStatement, h.Status.ToString(), h.Evidence.Count))
            .ToList();

        var dto = new AnalysisDto(
            analysis.Id.Value, analysis.UnitId.Value, analysis.AlarmFloodId.Value,
            analysis.Status.ToString(), analysis.Verdict, hypotheses);

        return Result<AnalysisDto?>.Success(dto);
    }
}
