using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// The second fixed incident, EVT-2026-0419 -- the EMI measurement artefact (ADR-037),
/// proven against a real LocalDB with the real seams and the real seeded fixture, no
/// mocks. Unlike 0418 (a cascade confirmed by movement), 0419's origin is a
/// measurement artefact confirmed by an ABSENCE: the walker names the EMI source via
/// its artefact edges, and the corroborator confirms it because the independent
/// witnesses stayed flat. These tests prove that genuinely-new engine logic, the
/// derived ranking (ADR-042), that the witness silence is load-bearing (fault
/// injection), and that the new unit-scoped corpus isolates the two incidents.
/// </summary>
public class GroundingDiagnosis0419Tests : RootCauseComponentTestDatabase
{
    private static readonly IncidentContext Incident = IncidentRegistry.Evt20260419;

    // ----- Graph walk: artefact-edge coverage names the EMI source as origin -----

    [Fact]
    public async Task Graph_walk_names_the_EMI_source_as_origin_via_artefact_edges_not_the_busy_flow_channel()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var result = await new EfGraphWalker(db).WalkAsync(Incident, CancellationToken.None);

        // The origin is BKR-2A (the EMI source) -- computed from artefact-edge coverage,
        // NOT the loud flow channel FT-9 and NOT 0418's FV-104.
        var origin = Assert.Single(result.Ranked, c => c.Role == "origin");
        Assert.Equal("BKR-2A", origin.Tag);
        Assert.NotEqual("FT-9", origin.Tag);
        Assert.NotEqual("FV-104", origin.Tag);

        // Derived, not the book's illustrative 0.70/0.16/0.10 (ADR-042): the EMI source is
        // attributed the whole flood; the corrupted channels are downstream symptoms it
        // explains; the genuine-excursion hypothesis explains nothing and is ruled out.
        Assert.Equal(1.0, origin.Weight, 3);
        var ft9 = Assert.Single(result.Ranked, c => c.Tag == "FT-9");
        Assert.Equal("downstream", ft9.Role);
        Assert.Equal(0d, ft9.Weight, 3);
        var excursion = Assert.Single(result.Ranked, c => c.Tag == "LOF-1");
        Assert.Equal("ruled-out", excursion.Role);
        Assert.Equal(0d, excursion.Weight, 3);

        // The EMI source explains the alarms on the channels it corrupts (all of them);
        // the busy flow channel explains only its own -- coverage, from the data.
        Assert.True(origin.Coverage > ft9.Coverage, $"expected BKR-2A coverage {origin.Coverage} > FT-9 {ft9.Coverage}");
    }

    // ----- Telemetry corroboration: silence confirms the artefact -----

    [Fact]
    public async Task Telemetry_corroboration_confirms_the_artefact_because_the_independent_witnesses_stay_flat()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Incident, GroundingSeed.Bkr2A, CancellationToken.None);

        Assert.True(corroboration.TimingFits, corroboration.Detail);
        Assert.Contains("stayed flat", corroboration.Detail);
    }

    [Fact]
    public async Task Telemetry_corroboration_fails_when_a_witness_deflected_because_that_means_a_real_excursion()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            // Fault injection: an independent witness (neutron flux) DID move. A real
            // excursion, not an artefact -- the silence was load-bearing, so this must flip.
            arrange.HistorianSamples.Add(new Domain.Grounding.HistorianSample
            {
                ChannelId = GroundingSeed.Nfx1,
                TimestampUtc = GroundingSeed.FloodStartUtc0419,
                Value = 1.0,
                Quality = 0,
            });
            await arrange.SaveChangesAsync();
        }

        await using var db = CreateDbContext();
        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Incident, GroundingSeed.Bkr2A, CancellationToken.None);

        Assert.False(corroboration.TimingFits);
        Assert.Contains("deflected", corroboration.Detail);
        Assert.Contains("real excursion", corroboration.Detail);
    }

    [Fact]
    public async Task Telemetry_corroboration_fails_when_a_coupled_channel_did_not_move()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            // The artefact story needs the coupled channels to move together; delete
            // one channel's deflection and the artefact cannot be corroborated.
            await arrange.HistorianSamples.Where(s => s.ChannelId == GroundingSeed.Pt7).ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Incident, GroundingSeed.Bkr2A, CancellationToken.None);

        Assert.False(corroboration.TimingFits);
        Assert.Contains("no deflection", corroboration.Detail);
    }

    // The full deterministic pipeline verdict (BKR-2A, sealed) moved to the H9 harness as
    // golden-0419-bkr2a (ADR-041) -- its single home, not duplicated here.

    // ----- Corpus scoping: the two incidents do not cross-contaminate -----

    [Fact]
    public async Task Retrieval_is_unit_scoped_so_each_incident_grounds_only_on_its_own_corpus()
    {
        await SeedAsync(); // seeds BOTH incidents
        await using var db = CreateDbContext();

        // 0419 (unit 2) grounds only on the 0419 corpus (Ch.5/Ch.10), never 0418's.
        var artefactPassages = await new EfRetriever(db, new NoOpEmbedder())
            .RetrieveAsync(Incident.QueryText, "BKR-2A", GroundingSeed.UnitId0419, CancellationToken.None);
        Assert.NotEmpty(artefactPassages);
        Assert.All(artefactPassages, p => Assert.Contains("Ch.5", p.SourceLabel));
        Assert.DoesNotContain(artefactPassages, p => p.SourceLabel.Contains("Ch.1-2"));

        // 0418 (unit 1) grounds only on the 0418 corpus, never 0419's artefact corpus.
        var cascadePassages = await new EfRetriever(db, new NoOpEmbedder())
            .RetrieveAsync(IncidentRegistry.Evt20260418.QueryText, "FV-104", GroundingSeed.UnitId, CancellationToken.None);
        Assert.NotEmpty(cascadePassages);
        Assert.DoesNotContain(cascadePassages, p => p.SourceLabel.Contains("Ch.5"));
    }

    // ----- Graph reader (ADR-036) returns the 0419 topology -----

    [Fact]
    public async Task Graph_reader_returns_the_seeded_0419_topology_with_artefact_and_rejected_edges()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var graph = await new EfIncidentGraphReader(db).ReadAsync(GroundingSeed.UnitId0419, GroundingSeed.IncidentId0419, CancellationToken.None);

        Assert.Equal(7, graph.Nodes.Count);
        Assert.Equal(5, graph.Edges.Count);
        Assert.Equal(3, graph.Edges.Count(e => e.Kind == "artefact"));
        Assert.Equal(2, graph.Edges.Count(e => e.Kind == "rejected"));

        // The three artefact edges run from BKR-2A into the coupled channels.
        var bkr = graph.Nodes.Single(n => n.Tag == "BKR-2A");
        Assert.All(graph.Edges.Where(e => e.Kind == "artefact"), e => Assert.Equal(bkr.ComponentId, e.FromComponentId));

        // The rejected edges run to the flat witnesses.
        var witnessIds = new[] { graph.Nodes.Single(n => n.Tag == "RTD-1").ComponentId, graph.Nodes.Single(n => n.Tag == "NFX-1").ComponentId };
        Assert.All(graph.Edges.Where(e => e.Kind == "rejected"), e => Assert.Contains(e.ToComponentId, witnessIds));
    }

    // ----- helpers -----

    private async Task SeedAsync()
    {
        await using var db = CreateDbContext();
        await GroundingSeed.SeedAsync(db);
    }
}
