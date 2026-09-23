using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Returns the incident's causal-graph topology (ADR-036). Hand-rolled dispatch,
/// no MediatR (ADR-002-amend), mirroring GetAnalysisByIdQueryHandler. An unknown
/// incident id is a first-class refusal (Result.Failure -> 404 at the edge); a
/// valid incident always has seeded topology to return.
/// </summary>
public sealed class GetIncidentGraphQueryHandler(IIncidentGraphReader reader)
    : IQueryHandler<GetIncidentGraphQuery, IncidentGraphResponse>
{
    public async Task<Result<IncidentGraphResponse>> Handle(GetIncidentGraphQuery query, CancellationToken cancellationToken)
    {
        if (!string.Equals(query.IncidentId, FixedIncident.IncidentId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<IncidentGraphResponse>.Failure($"unknown incident '{query.IncidentId}'");
        }

        var graph = await reader.ReadAsync(FixedIncident.UnitId, FixedIncident.IncidentId, cancellationToken);
        return Result<IncidentGraphResponse>.Success(graph);
    }
}
