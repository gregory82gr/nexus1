using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Returns the incident's causal-graph topology (ADR-036, ADR-037). Hand-rolled
/// dispatch, no MediatR (ADR-002-amend), mirroring GetAnalysisByIdQueryHandler. The
/// incident id is resolved against <see cref="IncidentRegistry"/> (its unit scopes
/// the read); an unknown id is a first-class refusal (Result.Failure -> 404), so
/// EVT-2026-0420 (unregistered) still 404s; a registered incident always has seeded
/// topology to return.
/// </summary>
public sealed class GetIncidentGraphQueryHandler(IIncidentGraphReader reader)
    : IQueryHandler<GetIncidentGraphQuery, IncidentGraphResponse>
{
    public async Task<Result<IncidentGraphResponse>> Handle(GetIncidentGraphQuery query, CancellationToken cancellationToken)
    {
        if (!IncidentRegistry.TryGet(query.IncidentId, out var incident))
        {
            return Result<IncidentGraphResponse>.Failure($"unknown incident '{query.IncidentId}'");
        }

        var graph = await reader.ReadAsync(incident.UnitId, incident.IncidentId, cancellationToken);
        return Result<IncidentGraphResponse>.Success(graph);
    }
}
