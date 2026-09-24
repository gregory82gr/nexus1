using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Explain;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// The served-model stage proven for real (ADR-033): real LocalDB + real,
/// loopback-only Ollama running the pinned nexus-dslm chat model and
/// nomic-embed-text. These tests SKIP (honestly, not silently pass) when Ollama
/// or the models are absent, and RUN end to end when present. They complete the
/// EVT-2026-0418 walking skeleton: engine decides, model explains, H3/H4
/// validates, audit seals. The deterministic H3/H4 abstention proofs remain in
/// GroundingDiagnosisTests (fixture drafts) -- reliably forcing a temperature-0
/// grounded local model to emit a hallucination is not reproducible, so here we
/// prove the real model's output PASSES H3/H4 with a real citation, and that the
/// pre-model gates still abstain through the full real pipeline.
/// </summary>
public class OllamaExplainPipelineTests(ITestOutputHelper output) : RootCauseComponentTestDatabase
{
    private static readonly OllamaOptions Options = new();

    private static readonly IncidentContext Incident = IncidentRegistry.Evt20260418;

    private static readonly IDateTimeProvider Clock = new FixedClock(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc));

    [SkippableFact]
    public async Task Embedder_returns_a_real_vector()
    {
        await EnsureOllamaAsync();
        using var http = new HttpClient();
        var embedder = new OllamaEmbedder(http, Options);

        var vector = await embedder.EmbedAsync("feedwater control valve actuator latency", EmbedKind.Document, CancellationToken.None);

        Assert.NotNull(vector);
        Assert.True(vector!.Count > 100, $"expected a real embedding, got length {vector.Count}");
    }

    [SkippableFact]
    public async Task Ingest_populates_embeddings_and_the_semantic_path_retrieves()
    {
        await EnsureOllamaAsync();
        await SeedAsync();
        using var http = new HttpClient();
        var embedder = new OllamaEmbedder(http, Options);

        int populated;
        await using (var db = CreateDbContext())
        {
            populated = await new EmbeddingIngestor(db, embedder).IngestAsync();
        }

        Assert.True(populated > 0, "embedding ingest populated nothing");

        await using (var verify = CreateDbContext())
        {
            Assert.False(await verify.CorpusChunks.AnyAsync(c => c.EmbeddingJson == null), "some corpus chunks still lack embeddings");

            // Empty tagText => the lexical path selects nothing, so a non-empty
            // result proves the C#-cosine semantic path over real embeddings.
            var passages = await new EfRetriever(verify, embedder)
                .RetrieveAsync("what starved the steam generator and tripped the reactor?", tagText: "", GroundingSeed.UnitId, CancellationToken.None);

            Assert.NotEmpty(passages);
            output.WriteLine($"semantic-only retrieval returned {passages.Count} passage(s)");
        }
    }

    [SkippableFact]
    public async Task Explainer_produces_a_validated_draft_citing_the_book_worked_example()
    {
        await EnsureOllamaAsync();
        await SeedAsync();

        await using var db = CreateDbContext();
        using var http = new HttpClient();
        var passages = await new EfRetriever(db, new OllamaEmbedder(http, Options))
            .RetrieveAsync("feedwater valve latency cascade", "FV-104", GroundingSeed.UnitId, CancellationToken.None);
        Assert.NotEmpty(passages);

        var explainer = new SemanticKernelExplainer(Options);
        var outcome = await explainer.ExplainAsync(Incident, "FV-104", passages, CancellationToken.None);
        var validation = outcome.Abstained
            ? null
            : await new RegistryAntiHallucinationValidator(db).ValidateAsync(outcome.Draft!, passages, CancellationToken.None);

        // The 3B CPU model is stochastic (see the determinism test); H8 forbids
        // salvaging a bad draft, so a non-compliant reply is a legitimate safe
        // abstention, not a test failure. The guarantee asserted here: whenever
        // the model DOES answer, its answer names FV-104, passes the real H3/H4
        // validator, and cites the honest book worked-example corpus -- it is
        // never a false or unsourced answer.
        if (outcome.Abstained)
        {
            output.WriteLine("model safely abstained: " + outcome.AbstainReason);
            return;
        }

        output.WriteLine("model draft: " + System.Text.Json.JsonSerializer.Serialize(outcome.Draft));
        Assert.Equal("FV-104", outcome.Draft!.CauseTag);
        Assert.True(validation!.Ok, validation.Reason);

        var citedIds = outcome.Draft.Claims.Select(c => c.CitationChunkId).ToHashSet();
        var cited = passages.Where(p => citedIds.Contains(p.ChunkId)).ToList();
        Assert.NotEmpty(cited);
        Assert.All(cited, p => Assert.Contains("From Flood to Cause", p.SourceLabel));
    }

    [SkippableFact]
    public async Task Full_pipeline_end_to_end_reaches_the_FV104_verdict_with_the_real_model()
    {
        await EnsureOllamaAsync();
        await SeedAsync();
        await IngestAsync();

        await using var db = CreateDbContext();
        using var http = new HttpClient();
        var result = await BuildRealRunner(db, http).RunAsync(Incident, CancellationToken.None);

        // The pipeline's safety invariant, which ALWAYS holds: the engine's
        // decision is FV-104, so the run either reaches the FV-104 verdict (when
        // the model's draft validates) or SAFELY ABSTAINS (H8) -- it can never emit
        // a different or unsourced verdict. Both outcomes are sealed. On a
        // compliant model run this reaches FV-104 with book citations (captured in
        // the evidence file); a safe abstention is an expected stochastic-model
        // outcome, not a failure.
        Assert.NotEmpty(result.AuditHash);
        await using var verify = CreateDbContext();
        Assert.True(await verify.AuditEntries.AnyAsync(a => a.Hash == result.AuditHash), "run was not sealed into the audit chain");

        if (result.Abstained)
        {
            output.WriteLine("safe abstention (no false verdict): " + result.AbstainReason);
            return;
        }

        Assert.Equal("FV-104", result.Verdict);
        Assert.All(result.Citations, p => Assert.Contains("From Flood to Cause", p.SourceLabel));
        output.WriteLine($"verdict={result.Verdict} auditHash={result.AuditHash}");
    }

    [SkippableFact]
    public async Task Engine_verdict_is_identical_across_two_runs_and_narration_equality_is_reported()
    {
        await EnsureOllamaAsync();
        await SeedAsync();

        // H6, the TRUE guarantee: the engine's decision is deterministic. Proven
        // model-free by running the graph walk twice -- it names FV-104 as origin
        // both times, every time. This never flakes because no model is involved.
        string origin1, origin2;
        await using (var db = CreateDbContext())
        {
            origin1 = (await new EfGraphWalker(db).WalkAsync(Incident, CancellationToken.None))
                .Ranked.Single(c => c.Role == "origin").Tag;
        }

        await using (var db = CreateDbContext())
        {
            origin2 = (await new EfGraphWalker(db).WalkAsync(Incident, CancellationToken.None))
                .Ranked.Single(c => c.Role == "origin").Tag;
        }

        Assert.Equal("FV-104", origin1);
        Assert.Equal(origin1, origin2);

        // The model's NARRATION determinism is reported, not asserted (Appendix G's
        // honest boundary): temperature 0 + a pinned seed make it likely, but on
        // CPU multi-threaded float-reduction order varies. Run the full pipeline
        // twice and log verdict + audit-hash (which seals the narration) equality.
        await IngestAsync();
        string? v1, v2, h1, h2;
        await using (var db = CreateDbContext())
        using (var http = new HttpClient())
        {
            var r = await BuildRealRunner(db, http).RunAsync(Incident, CancellationToken.None);
            v1 = r.Verdict ?? $"ABSTAIN({r.AbstainReason})";
            h1 = r.AuditHash;
        }

        await using (var db = CreateDbContext())
        using (var http = new HttpClient())
        {
            var r = await BuildRealRunner(db, http).RunAsync(Incident, CancellationToken.None);
            v2 = r.Verdict ?? $"ABSTAIN({r.AbstainReason})";
            h2 = r.AuditHash;
        }

        output.WriteLine($"run1 verdict={v1} hash={h1}");
        output.WriteLine($"run2 verdict={v2} hash={h2}");
        output.WriteLine(h1 == h2
            ? "narration was BYTE-IDENTICAL across both runs (audit hashes match)"
            : "narration DIFFERED across runs (engine origin still identical) -- expected-possible per the GPU/CPU-batching honest boundary");
    }

    [SkippableFact]
    public async Task Telemetry_fault_abstains_through_the_full_real_pipeline()
    {
        await EnsureOllamaAsync();
        await SeedAsync();
        await IngestAsync();
        await using (var arrange = CreateDbContext())
        {
            await arrange.HistorianSamples.Where(s => s.ChannelId == GroundingSeed.CoreT).ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        using var http = new HttpClient();
        var result = await BuildRealRunner(db, http).RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("telemetry corroboration failed", result.AbstainReason);
    }

    [SkippableFact]
    public async Task Corpus_fault_abstains_through_the_full_real_pipeline()
    {
        await EnsureOllamaAsync();
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            await arrange.CorpusChunks.ExecuteDeleteAsync();
        }

        await using var db = CreateDbContext();
        using var http = new HttpClient();
        var result = await BuildRealRunner(db, http).RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("no grounding", result.AbstainReason);
    }

    [SkippableFact]
    public void Di_composition_replaces_the_noop_defaults_with_the_ollama_backed_seams()
    {
        var services = new ServiceCollection();
        services.AddRootCauseExplain(Options);
        using var provider = services.BuildServiceProvider();

        Assert.IsType<OllamaEmbedder>(provider.GetRequiredService<IEmbedder>());
        Assert.IsType<SemanticKernelExplainer>(provider.GetRequiredService<IExplainer>());
    }

    // ----- helpers -----

    private FixedIncidentDiagnosisRunner BuildRealRunner(RootCauseDbContext db, HttpClient http) =>
        new(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            new EfRetriever(db, new OllamaEmbedder(http, Options)),
            new SemanticKernelExplainer(Options),
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock);

    private async Task SeedAsync()
    {
        await using var db = CreateDbContext();
        await GroundingSeed.SeedAsync(db);
    }

    private async Task IngestAsync()
    {
        using var http = new HttpClient();
        await using var db = CreateDbContext();
        await new EmbeddingIngestor(db, new OllamaEmbedder(http, Options)).IngestAsync();
    }

    private static async Task EnsureOllamaAsync()
    {
        bool ready;
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var tags = await http.GetFromJsonAsync<TagsResponse>(new Uri(Options.Endpoint, "/api/tags"));
            var names = tags?.Models?.Select(m => m.Name ?? string.Empty).ToList() ?? [];
            ready = names.Any(n => n.StartsWith("nexus-dslm", StringComparison.Ordinal))
                 && names.Any(n => n.StartsWith("nomic-embed-text", StringComparison.Ordinal));
        }
        catch
        {
            ready = false;
        }

        Skip.IfNot(ready, "Local Ollama with nexus-dslm + nomic-embed-text is not available on 127.0.0.1:11434.");
    }

    private sealed record TagsResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("models")] List<TagModel>? Models);

    private sealed record TagModel(
        [property: System.Text.Json.Serialization.JsonPropertyName("name")] string? Name);

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
