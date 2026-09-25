using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Diagnosis;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// ADR-043: the diagnosis engine under a WRONG causal model. A hand-written, test-only
/// fixture (unit 91 -- never in IncidentRegistry, never provisioned, not an incident) where
/// the physical truth contains a coupling the authored graph does not: an instrument-air
/// header, IA-1, loses pressure and the air-operated valve it feeds, AOV-2, drifts shut;
/// from AOV-2 the cascade runs exactly as modelled (AOV-2 -> LVL-3 -> TRP-4). The graph's
/// maintainers modelled AOV-2's downstream cascade but never its air supply.
///
/// The walker, seeing no edge from IA-1, names AOV-2 origin (it explains 4 of 5 alarms), and
/// AOV-2's own cascade fits its delay windows. The observable evidence against it is timing:
/// IA-1 began BEFORE AOV-2 and nothing explains it -- and causes do not run backwards. The
/// honest behaviour is abstention (the unexplained-precursor rule), not a confident verdict.
///
/// This tests the ENGINE (walker + corroborator gate), not RAG: no corpus is authored for the
/// fixture, and grounding, retrieval and explanation are not exercised. The one full-pipeline
/// assertion abstains at the corroboration gate, before retrieval runs.
/// </summary>
public class ModelMismatchTests : RootCauseComponentTestDatabase
{
    private const int Unit = 91;
    private const int Ia1 = 911, Aov2 = 912, Lvl3 = 913, Trp4 = 914;
    private static readonly DateTime T0 = new(2026, 2, 1, 6, 0, 0, DateTimeKind.Utc);

    private static readonly IncidentContext Fixture = new(
        IncidentId: "TEST-MODEL-MISMATCH",
        UnitId: Unit,
        AlarmedComponentIds: [Ia1, Aov2, Lvl3, Trp4],
        FloodStartUtc: T0,
        QueryText: "unused -- the mismatch case abstains before retrieval",
        CorpusVersion: "none -- no corpus is authored for this fixture");

    // ----- The mismatch: the IA-1 -> AOV-2 coupling is missing from the graph -----

    [Fact]
    public async Task Missing_upstream_coupling_with_an_earlier_unexplained_precursor_fails_corroboration()
    {
        await SeedAsync(graphHasAirSupplyEdge: false, ia1OnsetSeconds: 0);
        await using var db = CreateDbContext();

        var walk = await new EfGraphWalker(db).WalkAsync(Fixture, CancellationToken.None);
        var origin = Assert.Single(walk.Ranked, c => c.Role == "origin");
        var ia1 = Assert.Single(walk.Ranked, c => c.Tag == "IA-1");

        // The walker, working from the incomplete graph, blames AOV-2 -- the evidence of the
        // gap is visible in its own output (IA-1: independent, its own share of the flood).
        Assert.Equal("AOV-2", origin.Tag);
        Assert.Equal(4 / 5d, origin.Weight, 6);
        Assert.Equal("independent", ia1.Role);
        Assert.Equal(1 / 5d, ia1.Weight, 6);

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Fixture, origin.ComponentId, CancellationToken.None);

        Assert.False(corroboration.TimingFits, $"expected the unexplained precursor to fail corroboration; got: {corroboration.Detail}");
        Assert.Equal(
            "unexplained precursor: IA-1 began before origin AOV-2 and is not explained by it -- the causal graph may be incomplete",
            corroboration.Detail);
    }

    [Fact]
    public async Task Full_pipeline_seals_a_named_abstention_for_the_mismatch_before_retrieval_runs()
    {
        await SeedAsync(graphHasAirSupplyEdge: false, ia1OnsetSeconds: 0);
        await using var db = CreateDbContext();
        var retriever = new CountingRetriever(new EfRetriever(db, new NoOpEmbedder()));

        var result = await BuildRunner(db, retriever).RunAsync(Fixture, CancellationToken.None);

        Assert.True(result.Abstained, $"expected an abstention; got verdict {result.Verdict}");
        Assert.Null(result.Verdict);
        Assert.Equal(
            "telemetry corroboration failed: unexplained precursor: IA-1 began before origin AOV-2 and is not explained by it -- the causal graph may be incomplete",
            result.AbstainReason);
        Assert.Empty(result.Citations);

        // Grounding / retrieval / explanation were never exercised: the gate stopped the run first.
        Assert.Equal(0, retriever.Calls);

        // A real, named abstention -- persisted and sealed into the audit chain.
        await using var verify = CreateDbContext();
        var run = await verify.DiagnosisRuns.SingleAsync(r => r.DiagnosisRunId == result.DiagnosisRunId);
        Assert.Null(run.Verdict);
        Assert.Equal(result.AbstainReason, run.AbstainReason);
        Assert.True(await verify.AuditEntries.AnyAsync(a => a.Hash == result.AuditHash), "the abstention was not sealed into the audit chain");
    }

    // ----- Control (a): the complete graph -- the fixture's truth is recoverable -----
    // Never expected to be red: passes on the engine before and after ADR-043.

    [Fact]
    public async Task Control_complete_graph_names_the_true_origin_and_corroborates_cleanly()
    {
        await SeedAsync(graphHasAirSupplyEdge: true, ia1OnsetSeconds: 0);
        await using var db = CreateDbContext();

        var walk = await new EfGraphWalker(db).WalkAsync(Fixture, CancellationToken.None);
        var origin = Assert.Single(walk.Ranked, c => c.Role == "origin");
        Assert.Equal("IA-1", origin.Tag);
        Assert.Equal(1d, origin.Weight, 6);
        Assert.Equal("downstream", Assert.Single(walk.Ranked, c => c.Tag == "AOV-2").Role);

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Fixture, origin.ComponentId, CancellationToken.None);

        Assert.True(corroboration.TimingFits, corroboration.Detail);
        Assert.Equal("3 backbone edge(s) fit their delay windows against the historian", corroboration.Detail);
    }

    // ----- Control (b): a LATER unrelated alarm -- the rule is "precursor", not "independent" -----
    // Never expected to be red: passes on the engine before and after ADR-043.

    [Fact]
    public async Task Control_an_unexplained_alarm_that_starts_after_the_origin_does_not_block_the_verdict()
    {
        await SeedAsync(graphHasAirSupplyEdge: false, ia1OnsetSeconds: 100);
        await using var db = CreateDbContext();

        var walk = await new EfGraphWalker(db).WalkAsync(Fixture, CancellationToken.None);
        var origin = Assert.Single(walk.Ranked, c => c.Role == "origin");
        Assert.Equal("AOV-2", origin.Tag);
        Assert.Equal("independent", Assert.Single(walk.Ranked, c => c.Tag == "IA-1").Role);

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Fixture, origin.ComponentId, CancellationToken.None);

        Assert.True(corroboration.TimingFits, corroboration.Detail);
        Assert.Equal("2 backbone edge(s) fit their delay windows against the historian", corroboration.Detail);
    }

    // ----- Boundary (decision 4): equal or missing onsets never trigger the rule -----

    [Theory]
    [InlineData(20)]   // equal to AOV-2's onset -- simultaneity is ambiguous, not precedence
    [InlineData(null)] // no historian samples for IA-1 -- no onset, no evidence of precedence
    public async Task Boundary_equal_or_missing_precursor_onset_never_triggers_the_rule(int? ia1OnsetSeconds)
    {
        await SeedAsync(graphHasAirSupplyEdge: false, ia1OnsetSeconds);
        await using var db = CreateDbContext();

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Fixture, Aov2, CancellationToken.None);

        Assert.True(corroboration.TimingFits, corroboration.Detail);
    }

    // ----- helpers -----

    /// <summary>
    /// Alarms: IA-1 1, AOV-2 1, LVL-3 2, TRP-4 1 (5 total). Onsets: IA-1 at
    /// <paramref name="ia1OnsetSeconds"/> (none if null), AOV-2 t20, LVL-3 t50, TRP-4 t80.
    /// Backbone AOV-2 -> LVL-3 and LVL-3 -> TRP-4, windows [25,35]s (ranges, deliberately not
    /// the exact-point windows 0418 uses -- ADR-043's jitter observation). The air-supply edge
    /// IA-1 -> AOV-2 [15,25]s exists only in the complete-graph control.
    /// </summary>
    private async Task SeedAsync(bool graphHasAirSupplyEdge, int? ia1OnsetSeconds)
    {
        await using var db = CreateDbContext();

        db.Components.AddRange(
            Node(Ia1, "IA-1", "header", alarms: 1),
            Node(Aov2, "AOV-2", "valve", alarms: 1),
            Node(Lvl3, "LVL-3", "condition", alarms: 2),
            Node(Trp4, "TRP-4", "condition", alarms: 1));

        db.Edges.AddRange(Backbone(Aov2, Lvl3, 25, 35), Backbone(Lvl3, Trp4, 25, 35));
        if (graphHasAirSupplyEdge)
        {
            db.Edges.Add(Backbone(Ia1, Aov2, 15, 25));
        }

        if (ia1OnsetSeconds is { } seconds)
        {
            db.HistorianSamples.Add(Onset(Ia1, seconds));
        }

        db.HistorianSamples.AddRange(Onset(Aov2, 20), Onset(Lvl3, 50), Onset(Trp4, 80));
        await db.SaveChangesAsync();
    }

    private static Component Node(int id, string tag, string kind, int alarms) =>
        new() { ComponentId = id, UnitId = Unit, Tag = tag, Kind = kind, HealthScore = null, Status = "observed", AlarmCount = alarms, IllustrativeWeight = null, IllustrativeRole = null };

    private static Edge Backbone(int from, int to, int min, int max) =>
        new() { FromComponentId = from, ToComponentId = to, Kind = "backbone", DelayMinSeconds = min, DelayMaxSeconds = max, SourceRef = "test-fixture" };

    private static HistorianSample Onset(int channelId, int offsetSeconds) =>
        new() { ChannelId = channelId, TimestampUtc = T0.AddSeconds(offsetSeconds), Value = 1.0, Quality = 0 };

    private FixedIncidentDiagnosisRunner BuildRunner(Infrastructure.Persistence.RootCauseDbContext db, IRetriever retriever) =>
        new(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            retriever,
            new NotReachedExplainer(),
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock,
            NewDiagnosticsMetrics());

    private static readonly IDateTimeProvider Clock = new FixedClock(T0.AddMinutes(5));

    /// <summary>A spy over the real retriever: counts calls so the test can prove retrieval never ran.</summary>
    private sealed class CountingRetriever(IRetriever inner) : IRetriever
    {
        public int Calls { get; private set; }

        public Task<IReadOnlyList<Passage>> RetrieveAsync(string queryText, string tagText, int unitId, CancellationToken cancellationToken)
        {
            Calls++;
            return inner.RetrieveAsync(queryText, tagText, unitId, cancellationToken);
        }
    }

    /// <summary>The explain stage must never be reached in this fixture -- reaching it is a test failure.</summary>
    private sealed class NotReachedExplainer : IExplainer
    {
        public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("the explain stage was reached -- the mismatch case must abstain before retrieval");
    }

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
