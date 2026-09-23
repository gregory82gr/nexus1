using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Read the engineered causal graph (topology) for an incident -- the nodes and
/// edges the console draws as Figure 29.1 (ADR-036). Read-only: it returns the
/// seeded Component/Edge data as-is and runs NO engine logic (no walk, no
/// coverage, no verdict -- that is the diagnosis route's job). Only
/// EVT-2026-0418 is valid; any other id is an unknown-incident refusal (-> 404).
/// </summary>
public sealed record GetIncidentGraphQuery(string IncidentId) : IQuery<IncidentGraphResponse>;

/// <summary>The topology a frontend can draw directly: nodes plus edges (edges join to nodes by componentId).</summary>
public sealed record IncidentGraphResponse(
    string IncidentId,
    IReadOnlyList<GraphNodeDto> Nodes,
    IReadOnlyList<GraphEdgeDto> Edges);

public sealed record GraphNodeDto(
    int ComponentId,
    string Tag,
    string Kind,
    string Status,
    int AlarmCount,
    string? IllustrativeRole,
    double? IllustrativeWeight);

public sealed record GraphEdgeDto(
    int FromComponentId,
    int ToComponentId,
    string Kind,
    int DelayMinSeconds,
    int DelayMaxSeconds,
    string? SourceRef);

/// <summary>
/// Reads the seeded component/edge topology for a unit (ADR-036). The port lives
/// in Application; the EF implementation is in Infrastructure and reads through
/// the engine's read/write connection (nexus1_app) -- the same connection the
/// graph walk already reads Component/Edge through, distinct from H7's
/// model/retrieval read-only boundary. It only ever SELECTs.
/// </summary>
public interface IIncidentGraphReader
{
    Task<IncidentGraphResponse> ReadAsync(int unitId, string incidentId, CancellationToken cancellationToken);
}
