using System.Linq;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Diagnosis;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// The incident-graph topology read (ADR-036), proven against real LocalDB with the
/// real seeded EVT-2026-0418 fixture. The reader returns exactly the seeded
/// Component/Edge rows (no engine logic); the handler refuses any other incident.
/// </summary>
public class IncidentGraphReaderTests : RootCauseComponentTestDatabase
{
    [Fact]
    public async Task Reads_the_seeded_topology_ten_nodes_nine_edges_with_kinds_and_delays()
    {
        await using (var seed = CreateDbContext())
        {
            await GroundingSeed.SeedAsync(seed);
        }

        await using var db = CreateDbContext();
        var graph = await new EfIncidentGraphReader(db).ReadAsync(GroundingSeed.UnitId, GroundingSeed.IncidentId, CancellationToken.None);

        Assert.Equal(10, graph.Nodes.Count);
        Assert.Equal(9, graph.Edges.Count);
        Assert.Equal(7, graph.Edges.Count(e => e.Kind == "backbone"));
        Assert.Equal(1, graph.Edges.Count(e => e.Kind == "learned"));
        Assert.Equal(1, graph.Edges.Count(e => e.Kind == "rejected"));

        // FV-104 -> SG-1 is a backbone edge with the modelled 36s point window.
        var fv104 = graph.Nodes.Single(n => n.Tag == "FV-104");
        var sg1 = graph.Nodes.Single(n => n.Tag == "SG-1");
        var edge = graph.Edges.Single(e => e.FromComponentId == fv104.ComponentId && e.ToComponentId == sg1.ComponentId);
        Assert.Equal("backbone", edge.Kind);
        Assert.Equal(36, edge.DelayMinSeconds);
        Assert.Equal(36, edge.DelayMaxSeconds);

        // The 4kV bus is the learned contributor into the pump.
        var bus = graph.Nodes.Single(n => n.Tag == "BUS-2A");
        var fwp = graph.Nodes.Single(n => n.Tag == "FWP-2A");
        Assert.Equal("learned", graph.Edges.Single(e => e.FromComponentId == bus.ComponentId && e.ToComponentId == fwp.ComponentId).Kind);

        // The ruled-out FT-7 candidate is kept, its edge rejected with no window.
        var ft7 = graph.Nodes.Single(n => n.Tag == "FT-7");
        Assert.Equal("ruled-out", ft7.IllustrativeRole);
        var rejected = graph.Edges.Single(e => e.Kind == "rejected");
        Assert.Equal(ft7.ComponentId, rejected.FromComponentId);
        Assert.Equal(0, rejected.DelayMaxSeconds);
    }

    [Fact]
    public async Task Handler_returns_the_graph_for_the_fixed_incident()
    {
        await using (var seed = CreateDbContext())
        {
            await GroundingSeed.SeedAsync(seed);
        }

        await using var db = CreateDbContext();
        var handler = new GetIncidentGraphQueryHandler(new EfIncidentGraphReader(db));

        var result = await handler.Handle(new GetIncidentGraphQuery(GroundingSeed.IncidentId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value.Nodes.Count);
        Assert.Equal(9, result.Value.Edges.Count);
    }

    [Fact]
    public async Task Handler_refuses_an_unknown_incident()
    {
        await using var db = CreateDbContext();
        var handler = new GetIncidentGraphQueryHandler(new EfIncidentGraphReader(db));

        var result = await handler.Handle(new GetIncidentGraphQuery("EVT-9999-9999"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("unknown incident", result.Error);
    }
}
