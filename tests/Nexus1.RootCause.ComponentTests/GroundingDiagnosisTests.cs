using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// The deterministic half of the One-Truth Pipeline (ADR-032), proven against a
/// real LocalDB with the real EF seams and the real EVT-2026-0418 fixture -- no
/// mocks. Coverage computes from the seeded alarm counts (FV-104 explains 11 of
/// 14), timing fits are read from the historian, retrieval grounds on the honest
/// book-worked-example corpus, and every fault injection produces a named
/// abstention with no verdict. The natural-language generation step is deferred;
/// the DraftAnswer here is a fixture standing in for the served model, validated
/// identically.
/// </summary>
public class GroundingDiagnosisTests : RootCauseComponentTestDatabase
{
    private static readonly IncidentContext Incident = IncidentRegistry.Evt20260418;

    // ----- Graph walk -----

    [Fact]
    public async Task Graph_walk_computes_FV104_as_origin_explaining_eleven_of_fourteen()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var result = await new EfGraphWalker(db).WalkAsync(Incident, CancellationToken.None);

        var origin = Assert.Single(result.Ranked, c => c.Role == "origin");
        Assert.Equal("FV-104", origin.Tag);

        // Coverage is the share of the flood; 11 of 14 must fall out of the data.
        Assert.Equal(11, (int)Math.Round(origin.Coverage * 14));

        // The loud bearing outranks nothing above it: it is a proximate symptom,
        // not the origin, and covers fewer alarms than FV-104.
        var bearing = Assert.Single(result.Ranked, c => c.Tag == "PB-2A");
        Assert.Equal("proximate", bearing.Role);
        Assert.True(bearing.Coverage < origin.Coverage);

        // The ruled-out candidate is present, not discarded.
        Assert.Contains(result.Ranked, c => c.Tag == "FT-7" && c.Role == "ruled-out");
    }

    // ----- Telemetry corroboration -----

    [Fact]
    public async Task Telemetry_corroboration_fits_the_delay_windows_for_the_origin_cascade()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Incident, GroundingSeed.Fv104, CancellationToken.None);

        Assert.True(corroboration.TimingFits, corroboration.Detail);
    }

    [Fact]
    public async Task Telemetry_corroboration_fails_when_a_downstream_channel_is_missing()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            // Fault injection: the core-temperature channel goes dark.
            await arrange.HistorianSamples.Where(s => s.ChannelId == GroundingSeed.CoreT).ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        var corroboration = await new EfTelemetryCorroborator(db).CheckAsync(Incident, GroundingSeed.Fv104, CancellationToken.None);

        Assert.False(corroboration.TimingFits);
        Assert.Contains("missing historian data", corroboration.Detail);
    }

    // ----- Retrieval -----

    [Fact]
    public async Task Retrieval_grounds_lexically_on_the_tag_with_an_honest_source_label()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var passages = await new EfRetriever(db, new NoOpEmbedder()).RetrieveAsync("feedwater valve cascade", "FV-104", GroundingSeed.UnitId, CancellationToken.None);

        Assert.NotEmpty(passages);
        Assert.All(passages, p => Assert.Contains("From Flood to Cause", p.SourceLabel));
        // The corpus is the book's worked example, never a fabricated plant document.
        Assert.All(passages, p => Assert.Contains("worked example", p.SourceLabel));
    }

    [Fact]
    public async Task Retrieval_returns_nothing_for_a_tag_absent_from_the_corpus()
    {
        await SeedAsync();
        await using var db = CreateDbContext();

        var passages = await new EfRetriever(db, new NoOpEmbedder()).RetrieveAsync("unrelated", "XV-999", GroundingSeed.UnitId, CancellationToken.None);

        Assert.Empty(passages);
    }

    // ----- Validator (H3, H4) -----

    [Fact]
    public async Task Validator_passes_a_registered_entity_and_a_cited_passage()
    {
        await SeedAsync();
        await using var db = CreateDbContext();
        var passage = new Passage(100, "body", "label", 0);
        var draft = new DraftAnswer("FV-104", ["FV-104"], [new Claim("valve latency", 100)]);

        var validation = await new RegistryAntiHallucinationValidator(db).ValidateAsync(draft, [passage], CancellationToken.None);

        Assert.True(validation.Ok);
    }

    [Fact]
    public async Task Validator_H3_rejects_an_unregistered_entity()
    {
        await SeedAsync();
        await using var db = CreateDbContext();
        var draft = new DraftAnswer("FV-104", ["FV-999"], []);

        var validation = await new RegistryAntiHallucinationValidator(db).ValidateAsync(draft, [], CancellationToken.None);

        Assert.False(validation.Ok);
        Assert.Contains("unknown entity FV-999", validation.Reason);
    }

    [Fact]
    public async Task Validator_H4_rejects_a_claim_citing_a_passage_not_retrieved()
    {
        await SeedAsync();
        await using var db = CreateDbContext();
        var passage = new Passage(100, "body", "label", 0);
        var draft = new DraftAnswer("FV-104", ["FV-104"], [new Claim("valve latency", CitationChunkId: 999999)]);

        var validation = await new RegistryAntiHallucinationValidator(db).ValidateAsync(draft, [passage], CancellationToken.None);

        Assert.False(validation.Ok);
        Assert.Contains("uncited claim", validation.Reason);
    }

    // ----- Audit hash chain (H10) -----

    [Fact]
    public async Task Audit_chain_links_each_entry_to_the_previous_hash_from_genesis()
    {
        await SeedAsync();
        await using var db = CreateDbContext();
        var writer = new Sha256AuditChainWriter(db, Clock);

        var hash1 = await writer.AppendAsync("payload-one", CancellationToken.None);
        var hash2 = await writer.AppendAsync("payload-two", CancellationToken.None);

        Assert.Equal(Sha256AuditChainWriter.ComputeHash(Sha256AuditChainWriter.GenesisHash, "payload-one"), hash1);
        Assert.Equal(Sha256AuditChainWriter.ComputeHash(hash1, "payload-two"), hash2);

        var entries = await db.AuditEntries.OrderBy(a => a.Seq).ToListAsync();
        Assert.Equal(2, entries.Count);
        Assert.Equal(Sha256AuditChainWriter.GenesisHash, entries[0].PrevHash);
        Assert.Equal(hash1, entries[1].PrevHash);
    }

    // ----- Run store -----

    [Fact]
    public async Task Run_store_persists_the_run_and_its_candidates()
    {
        await SeedAsync();
        await using var db = CreateDbContext();
        var run = new Domain.Grounding.DiagnosisRun
        {
            IncidentId = "EVT-2026-0418",
            StartedAtUtc = Clock.UtcNow,
            Verdict = "FV-104",
            CorpusVersion = "test",
        };
        var candidates = new[]
        {
            new Domain.Grounding.Candidate { ComponentId = GroundingSeed.Fv104, Role = "origin", Weight = 0.66, Coverage = 0.7857 },
            new Domain.Grounding.Candidate { ComponentId = GroundingSeed.Ft7, Role = "ruled-out", Weight = 0.02, Coverage = 0 },
        };

        var runId = await new EfDiagnosisRunStore(db).SaveAsync(run, candidates, CancellationToken.None);

        Assert.True(runId > 0);
        await using var verify = CreateDbContext();
        Assert.Equal(1, await verify.DiagnosisRuns.CountAsync(r => r.DiagnosisRunId == runId));
        Assert.Equal(2, await verify.Candidates.CountAsync(c => c.DiagnosisRunId == runId));
    }

    // ----- Full pipeline: the pre-model fault-injection abstentions -----
    // The FV-104 verdict and the two draft-level traps (unregistered entity, uncited claim)
    // moved to the H9 harness as golden-0418-fv104, trap-transposed-entity and
    // trap-invented-source (ADR-041) -- their single home. What stays here are the
    // fault-injection abstentions, which are not Appendix-J trap categories.

    [Fact]
    public async Task Full_pipeline_abstains_when_the_historian_cannot_corroborate()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            await arrange.HistorianSamples.Where(s => s.ChannelId == GroundingSeed.CoreT).ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        var draft = await GoodDraftAsync(db);
        var result = await BuildRunner(db, draft).RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Null(result.Verdict);
        Assert.Contains("telemetry corroboration failed", result.AbstainReason);
    }

    [Fact]
    public async Task Full_pipeline_abstains_when_there_is_nothing_to_ground_on()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            await arrange.CorpusChunks.ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        var draft = new DraftAnswer("FV-104", ["FV-104"], []);
        var result = await BuildRunner(db, draft).RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("no grounding", result.AbstainReason);
    }

    // ----- helpers -----

    private async Task SeedAsync()
    {
        await using var db = CreateDbContext();
        await GroundingSeed.SeedAsync(db);
    }

    // A fixture explainer standing in for the served model -- the real Ollama-
    // backed pipeline is proven separately in OllamaExplainPipelineTests (gated on
    // a live Ollama). These component tests prove the deterministic engine +
    // validation + seal + abstention logic against the real database.
    private FixedIncidentDiagnosisRunner BuildRunner(RootCauseDbContext db, DraftAnswer draft) =>
        new(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            new EfRetriever(db, new NoOpEmbedder()),
            new FixtureExplainer(ExplainOutcome.Answer(draft)),
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock,
            NewDiagnosticsMetrics());

    private sealed class FixtureExplainer(ExplainOutcome outcome) : IExplainer
    {
        public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken) => Task.FromResult(outcome);
    }

    private static async Task<DraftAnswer> GoodDraftAsync(RootCauseDbContext db)
    {
        var chunkId = await FirstChunkIdAsync(db);
        return new DraftAnswer("FV-104", ["FV-104"], [new Claim("FV-104 actuator latency initiated the feedwater cascade", chunkId)]);
    }

    private static async Task<long> FirstChunkIdAsync(RootCauseDbContext db) =>
        await db.CorpusChunks.OrderBy(c => c.ChunkId).Select(c => c.ChunkId).FirstAsync();

    private static readonly IDateTimeProvider Clock = new FixedClock(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc));

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
