using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// Reads the seeded component/edge topology for a unit (ADR-036), through the
/// default read/write RootCauseDbContext (nexus1_app) -- the same connection the
/// graph walk already reads these tables through. It only SELECTs; it never runs
/// the engine or writes.
/// </summary>
public sealed class EfIncidentGraphReader(RootCauseDbContext db) : IIncidentGraphReader
{
    public async Task<IncidentGraphResponse> ReadAsync(int unitId, string incidentId, CancellationToken cancellationToken)
    {
        var nodes = await db.Components
            .Where(c => c.UnitId == unitId)
            .OrderBy(c => c.ComponentId)
            .Select(c => new GraphNodeDto(c.ComponentId, c.Tag, c.Kind, c.Status, c.AlarmCount, c.IllustrativeRole, c.IllustrativeWeight))
            .ToListAsync(cancellationToken);

        var nodeIds = nodes.Select(n => n.ComponentId).ToHashSet();

        var edges = await db.Edges
            .Where(e => nodeIds.Contains(e.FromComponentId) && nodeIds.Contains(e.ToComponentId))
            .OrderBy(e => e.EdgeId)
            .Select(e => new GraphEdgeDto(e.FromComponentId, e.ToComponentId, e.Kind, e.DelayMinSeconds, e.DelayMaxSeconds, e.SourceRef))
            .ToListAsync(cancellationToken);

        return new IncidentGraphResponse(incidentId, nodes, edges);
    }
}
