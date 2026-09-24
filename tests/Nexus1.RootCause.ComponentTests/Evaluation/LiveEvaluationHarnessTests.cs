using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Explain;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace Nexus1.RootCause.ComponentTests.Evaluation;

/// <summary>
/// The H9 evaluation harness, live tier (ADR-041): every case with tier "live" runs here
/// under its own name against real LocalDB + the real, loopback-only Ollama (nexus-dslm +
/// nomic-embed-text). Each case SKIPS -- by name, with a stated reason -- when Ollama or a
/// model is absent; it never silently passes and never fails for the model's absence.
///
/// The 3B CPU model is stochastic, so golden live cases expect `verdict_or_safe_abstain`:
/// the run either reaches the engine's verdict (validated, sealed, cited from the right
/// chapter) or safely abstains (H8) -- it can never emit a different or unsourced verdict.
/// The no-grounding case tests the retriever seam (H2) against the real embedder.
/// </summary>
public class LiveEvaluationHarnessTests(ITestOutputHelper output) : RootCauseComponentTestDatabase
{
    private static readonly OllamaOptions Options = new();

    private static readonly EvalClock Clock = new(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc));

    public static TheoryData<string> Cases => EvalCaseCatalog.IdsFor("live");

    [SkippableTheory]
    [MemberData(nameof(Cases))]
    public async Task Case(string id)
    {
        var c = EvalCaseCatalog.Get(id);
        Skip.If(c.Skip is not null, c.Skip);
        await EnsureOllamaAsync();
        output.WriteLine($"[{c.Id}] {c.Category} / {c.Seam} / {c.Target.IncidentId} -- intent (documentation only): {c.Intent}");

        using var http = new HttpClient();
        var embedder = new OllamaEmbedder(http, Options);
        await using (var seed = CreateDbContext())
        {
            await GroundingSeed.SeedAsync(seed);
            await new EmbeddingIngestor(seed, embedder).IngestAsync();
        }

        switch (c.Seam)
        {
            case "pipeline":
                await RunPipelineCaseAsync(c, embedder);
                break;
            case "retriever":
                await RunRetrieverCaseAsync(c, embedder);
                break;
            default:
                throw new InvalidOperationException($"{c.Id}: seam '{c.Seam}' is not runnable on the live tier.");
        }
    }

    private async Task RunPipelineCaseAsync(EvalCase c, OllamaEmbedder embedder)
    {
        Assert.Equal("verdict_or_safe_abstain", c.Expect);
        Assert.True(IncidentRegistry.TryGet(c.Target.IncidentId, out var incident), $"{c.Id}: target not in the registry.");

        await using var db = CreateDbContext();
        var runner = new FixedIncidentDiagnosisRunner(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            new EfRetriever(db, embedder),
            new SemanticKernelExplainer(Options),
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock,
            NewDiagnosticsMetrics());

        var result = await runner.RunAsync(incident, CancellationToken.None);

        // Always holds: the run is sealed, and it never names anything but the engine's origin.
        Assert.NotEmpty(result.AuditHash);
        await using var verify = CreateDbContext();
        Assert.True(await verify.AuditEntries.AnyAsync(a => a.Hash == result.AuditHash), $"{c.Id}: run was not sealed into the audit chain.");
        EvalAssertions.MustNotAssert(c, result);

        if (result.Abstained)
        {
            Assert.Null(result.Verdict);
            output.WriteLine($"[{c.Id}] SAFE ABSTENTION (no false verdict): {result.AbstainReason}");
            return;
        }

        Assert.Equal(c.ExpectVerdict, result.Verdict);
        Assert.All(result.Citations, p => Assert.Contains("From Flood to Cause", p.SourceLabel));
        EvalAssertions.MustCite(c, result.Citations);
        EvalAssertions.MustNotCite(c, result.Citations);
        output.WriteLine($"[{c.Id}] VERDICT {result.Verdict} auditHash={result.AuditHash} citations=[{string.Join(" | ", result.Citations.Select(p => p.SourceLabel))}]");
    }

    private async Task RunRetrieverCaseAsync(EvalCase c, OllamaEmbedder embedder)
    {
        Assert.Equal("abstain", c.Expect);
        Assert.True(IncidentRegistry.TryGet(c.Target.IncidentId, out var incident), $"{c.Id}: target not in the registry.");

        await using var db = CreateDbContext();
        var retriever = new EfRetriever(db, embedder);

        // Empty tagText => the lexical path selects nothing; only the real semantic path
        // (C# cosine over real nomic-embed-text vectors, H2 floor) can return passages.
        var control = await retriever.RetrieveAsync(c.RetrieverControlQuery!, tagText: "", incident.UnitId, CancellationToken.None);
        output.WriteLine($"[{c.Id}] control query \"{c.RetrieverControlQuery}\" -> {control.Count} passage(s)");
        Assert.True(control.Count > 0, $"{c.Id}: the related control query found nothing -- the semantic path is not live, so an empty result below would be vacuous.");

        var passages = await retriever.RetrieveAsync(c.RetrieverQuery!, tagText: "", incident.UnitId, CancellationToken.None);
        output.WriteLine($"[{c.Id}] unrelated query \"{c.RetrieverQuery}\" -> {passages.Count} passage(s) [{string.Join(" | ", passages.Select(p => p.SourceLabel))}]");

        // H2: nothing above the floor = nothing to ground on. The pipeline turns this
        // empty set into "no grounding: corpus retrieval returned no passages".
        Assert.Empty(passages);
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
}
