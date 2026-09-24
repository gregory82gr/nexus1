using System.Text.Json;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.ComponentTests.Evaluation;

/// <summary>
/// One H9 evaluation case (From Flood to Cause, Appendix J; ADR-041), loaded from
/// Evaluation/Cases/*.json. Follows the book's grammar with one deliberate substitution:
/// there is NO free-form question interface in this system (the diagnosis route takes an
/// incident id), so the book's `question` becomes `target.incidentId` + `intent`, where
/// `intent` is documentation only and is never sent anywhere.
/// </summary>
public sealed class EvalCase
{
    public string Id { get; init; } = "";

    /// <summary>golden | trap-transposed-entity | trap-invented-source | trap-leading | trap-no-grounding.</summary>
    public string Category { get; init; } = "";

    /// <summary>deterministic (fixture explainer, always runs) | live (real Ollama, skippable).</summary>
    public string Tier { get; init; } = "";

    /// <summary>pipeline (full One-Truth run) | record (the human case record) | retriever (the H2 gate).</summary>
    public string Seam { get; init; } = "";

    public EvalTarget Target { get; init; } = new();

    /// <summary>The book-style natural-language phrasing. DOCUMENTATION ONLY — never executed.</summary>
    public string Intent { get; init; } = "";

    /// <summary>Trap cases: the draft injected at the IExplainer seam. Null = the golden "good draft".</summary>
    public InjectedDraftSpec? InjectedDraft { get; init; }

    /// <summary>verdict | abstain | refused | verdict_or_safe_abstain | inconclusive.</summary>
    public string Expect { get; init; } = "";

    public string? ExpectVerdict { get; init; }

    public string? ExpectAbstainReasonContains { get; init; }

    public List<string> ExpectReasonContains { get; init; } = [];

    /// <summary>The ranked candidates, in engine order (golden pipeline cases).</summary>
    public List<ExpectedCandidate> ExpectCandidates { get; init; } = [];

    /// <summary>The origin's share of the flood, "numerator/denominator" (e.g. "11/14").</summary>
    public string? ExpectOriginCoverage { get; init; }

    /// <summary>Tags the result must never name as the verdict/origin.</summary>
    public List<string> MustNotAssert { get; init; } = [];

    /// <summary>SourceLabel substrings that must appear among the citations (ChunkIds are unstable across databases).</summary>
    public List<string> MustCite { get; init; } = [];

    /// <summary>SourceLabel substrings that must never appear among the citations.</summary>
    public List<string> MustNotCite { get; init; } = [];

    /// <summary>Retriever seam only: the query embedded on the semantic path (expected to find nothing above the H2 floor).</summary>
    public string? RetrieverQuery { get; init; }

    /// <summary>Retriever seam only: a related query that MUST find grounding — proves the semantic path is live, so an empty result is not vacuous.</summary>
    public string? RetrieverControlQuery { get; init; }

    /// <summary>A named reason; the case is reported Skipped by name, never silently absent.</summary>
    public string? Skip { get; init; }

    public override string ToString() => Id;
}

public sealed class EvalTarget
{
    public string IncidentId { get; init; } = "";
}

public sealed class InjectedDraftSpec
{
    public string CauseTag { get; init; } = "";

    public List<string> Entities { get; init; } = [];

    public List<ClaimSpec> Claims { get; init; } = [];
}

public sealed class ClaimSpec
{
    public string Text { get; init; } = "";

    /// <summary>"retrieved" (the first retrieved passage) | "unretrieved:&lt;SourceLabel substring&gt;" (a REAL document that exists but was not retrieved).</summary>
    public string Cite { get; init; } = "";
}

public sealed class ExpectedCandidate
{
    public string Tag { get; init; } = "";

    public string Role { get; init; } = "";

    public double Weight { get; init; }
}

/// <summary>The known case vocabulary and the loader.</summary>
public static class EvalCaseCatalog
{
    public static readonly string[] Categories = ["golden", "trap-transposed-entity", "trap-invented-source", "trap-leading", "trap-no-grounding"];

    public static readonly string[] Tiers = ["deterministic", "live"];

    public static readonly string[] Seams = ["pipeline", "record", "retriever"];

    public static readonly string[] Expectations = ["verdict", "abstain", "refused", "verdict_or_safe_abstain", "inconclusive"];

    /// <summary>The only incidents the harness may target: the three already-built fixed incidents.</summary>
    public static readonly string[] Targets = ["EVT-2026-0418", "EVT-2026-0419", Evt20260420QaEscape.IncidentId];

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly Lazy<IReadOnlyList<(string FileName, EvalCase Case)>> Loaded = new(Load);

    public static string CasesDirectory => Path.Combine(AppContext.BaseDirectory, "Evaluation", "Cases");

    public static IReadOnlyList<EvalCase> All => Loaded.Value.Select(x => x.Case).ToList();

    public static IReadOnlyList<(string FileName, EvalCase Case)> AllWithFiles => Loaded.Value;

    public static EvalCase Get(string id) => All.Single(c => c.Id == id);

    /// <summary>Case ids for one tier, as xUnit theory data — each case then runs under its own name.</summary>
    public static TheoryData<string> IdsFor(string tier)
    {
        var data = new TheoryData<string>();
        foreach (var c in All.Where(c => c.Tier == tier).OrderBy(c => c.Id))
        {
            data.Add(c.Id);
        }

        return data;
    }

    private static IReadOnlyList<(string FileName, EvalCase Case)> Load()
    {
        if (!Directory.Exists(CasesDirectory))
        {
            throw new InvalidOperationException($"H9 case directory not found at '{CasesDirectory}' -- are the case files copied to the output?");
        }

        return Directory.GetFiles(CasesDirectory, "*.json")
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => (Path.GetFileNameWithoutExtension(f),
                JsonSerializer.Deserialize<EvalCase>(File.ReadAllText(f), Json)
                    ?? throw new InvalidOperationException($"H9 case file '{f}' deserialized to null.")))
            .ToList();
    }
}

/// <summary>
/// The explainer for the deterministic tier. With no injected draft it returns the golden
/// "good draft" — naming the origin the engine decided and citing what was actually
/// retrieved — so the case tests the engine's decision, the grounding gates, validation and
/// the seal. With an injected draft (a trap) it returns exactly that draft, resolving each
/// citation spec against the real corpus. It never decides anything itself.
/// </summary>
public sealed class CaseExplainer(InjectedDraftSpec? spec, IReadOnlyList<CorpusRef> corpus) : IExplainer
{
    public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken)
    {
        if (spec is null)
        {
            var good = new DraftAnswer(originTag, [originTag], [new Claim($"{originTag} is the origin the engine decided.", passages[0].ChunkId)]);
            return Task.FromResult(ExplainOutcome.Answer(good));
        }

        var claims = spec.Claims.Select(c => new Claim(c.Text, ResolveCitation(c.Cite, passages))).ToList();
        return Task.FromResult(ExplainOutcome.Answer(new DraftAnswer(spec.CauseTag, spec.Entities, claims)));
    }

    private long ResolveCitation(string cite, IReadOnlyList<Passage> passages)
    {
        if (cite == "retrieved")
        {
            return passages[0].ChunkId;
        }

        const string unretrieved = "unretrieved:";
        if (cite.StartsWith(unretrieved, StringComparison.Ordinal))
        {
            var label = cite[unretrieved.Length..];
            var retrievedIds = passages.Select(p => p.ChunkId).ToHashSet();
            return corpus.First(c => c.SourceLabel.Contains(label, StringComparison.Ordinal) && !retrievedIds.Contains(c.ChunkId)).ChunkId;
        }

        throw new InvalidOperationException($"Unknown citation spec '{cite}'.");
    }
}

public sealed record CorpusRef(long ChunkId, string SourceLabel);

/// <summary>Assertions shared by both runners.</summary>
public static class EvalAssertions
{
    public static void MustNotAssert(EvalCase c, DiagnosisResult result)
    {
        foreach (var tag in c.MustNotAssert)
        {
            Assert.False(string.Equals(result.Verdict, tag, StringComparison.OrdinalIgnoreCase), $"{c.Id}: the verdict must not be {tag}.");
            Assert.DoesNotContain(result.Candidates, x => x.Role == "origin" && string.Equals(x.Tag, tag, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void MustCite(EvalCase c, IReadOnlyList<Passage> citations)
    {
        foreach (var label in c.MustCite)
        {
            Assert.True(citations.Any(p => p.SourceLabel.Contains(label, StringComparison.Ordinal)),
                $"{c.Id}: expected a citation whose source label contains '{label}'; got [{string.Join(" | ", citations.Select(p => p.SourceLabel))}].");
        }
    }

    public static void MustNotCite(EvalCase c, IReadOnlyList<Passage> citations)
    {
        foreach (var label in c.MustNotCite)
        {
            Assert.False(citations.Any(p => p.SourceLabel.Contains(label, StringComparison.Ordinal)),
                $"{c.Id}: no citation may come from a source labelled '{label}'.");
        }
    }
}

public sealed class EvalClock(DateTime now) : IDateTimeProvider
{
    public DateTime UtcNow => now;
}
