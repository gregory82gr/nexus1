using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.UnitTests;

/// <summary>
/// Proves the runner's composition and veto logic (ADR-032, ADR-033) in
/// isolation, with in-memory fakes for every seam -- no database, no broker, no
/// model. The graph walk decides, two grounding gates then the model then H3/H4
/// each get a veto; the happy path reaches a verdict, and every abstention path
/// (no origin, telemetry, corpus, the model declining, validation) yields a named
/// reason with no verdict. Every run is persisted and sealed. The fake explainer
/// stands in for the served model exactly as the real Ollama-backed one is
/// composed -- validated identically.
/// </summary>
public class FixedIncidentDiagnosisRunnerTests
{
    private static readonly IncidentContext Incident = new("EVT-2026-0418", UnitId: 1, AlarmedComponentIds: [1, 2, 3], FloodStartUtc: new DateTime(2026, 4, 18, 17, 9, 52, DateTimeKind.Utc));

    private static readonly DraftAnswer GoodDraft = new(
        CauseTag: "FV-104",
        Entities: ["FV-104"],
        Claims: [new Claim("FV-104 actuator latency initiated the cascade", CitationChunkId: 100)]);

    private static RankedCandidate Origin => new(1, "FV-104", "origin", 0.66, 0.7857);

    private static GraphWalkResult WalkWithOrigin => new([Origin, new RankedCandidate(4, "PB-2A", "proximate", 0.20, 0.5)]);

    private static IReadOnlyList<Passage> OnePassage => [new Passage(100, "worked example body", "From Flood to Cause, worked example", 0)];

    [Fact]
    public async Task Happy_path_reaches_a_verdict_and_seals_it()
    {
        var store = new FakeStore();
        var audit = new FakeAudit();
        var runner = Build(
            walk: WalkWithOrigin,
            corroboration: new Corroboration(true, "fits"),
            passages: OnePassage,
            explain: ExplainOutcome.Answer(GoodDraft),
            validation: new Validation(true, null),
            store: store,
            audit: audit);

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.False(result.Abstained);
        Assert.Equal("FV-104", result.Verdict);
        Assert.Null(result.AbstainReason);
        Assert.Equal(audit.Hash, result.AuditHash);
        Assert.Single(result.Citations);
        Assert.Equal("FV-104", store.SavedRun!.Verdict);
        Assert.Equal(2, store.SavedCandidates!.Count); // both ranked candidates persisted, ruled-out kept
        Assert.NotNull(audit.LastPayload);
        Assert.Contains("FV-104 actuator latency initiated the cascade", audit.LastPayload); // the model's own answer is sealed (H10)
    }

    [Fact]
    public async Task No_origin_from_the_walk_abstains_before_the_model()
    {
        var store = new FakeStore();
        var explainer = new FakeExplainer(ExplainOutcome.Answer(GoodDraft));
        var runner = Build(
            walk: new GraphWalkResult([new RankedCandidate(4, "PB-2A", "proximate", 0.20, 0.5)]),
            corroboration: new Corroboration(true, "fits"),
            passages: OnePassage,
            explainer: explainer,
            validation: new Validation(true, null),
            store: store);

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Null(result.Verdict);
        Assert.Contains("no origin", result.AbstainReason);
        Assert.Empty(result.Citations);
        Assert.False(explainer.WasCalled); // never reached the model
        Assert.Equal("EVT-2026-0418", store.SavedRun!.IncidentId); // abstention still recorded
    }

    [Fact]
    public async Task Failed_telemetry_corroboration_abstains_before_the_model()
    {
        var explainer = new FakeExplainer(ExplainOutcome.Answer(GoodDraft));
        var runner = Build(
            walk: WalkWithOrigin,
            corroboration: new Corroboration(false, "missing historian data for component 6"),
            passages: OnePassage,
            explainer: explainer,
            validation: new Validation(true, null));

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("telemetry corroboration failed", result.AbstainReason);
        Assert.Contains("missing historian data", result.AbstainReason);
        Assert.Empty(result.Citations);
        Assert.False(explainer.WasCalled);
    }

    [Fact]
    public async Task Empty_corpus_retrieval_abstains_before_the_model()
    {
        var explainer = new FakeExplainer(ExplainOutcome.Answer(GoodDraft));
        var runner = Build(
            walk: WalkWithOrigin,
            corroboration: new Corroboration(true, "fits"),
            passages: [],
            explainer: explainer,
            validation: new Validation(true, null));

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("no grounding", result.AbstainReason);
        Assert.False(explainer.WasCalled);
    }

    [Fact]
    public async Task Model_declining_abstains_with_its_reason()
    {
        var runner = Build(
            walk: WalkWithOrigin,
            corroboration: new Corroboration(true, "fits"),
            passages: OnePassage,
            explain: ExplainOutcome.Abstain("insufficient grounding in context"),
            validation: new Validation(true, null));

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Null(result.Verdict);
        Assert.Contains("explain abstained", result.AbstainReason);
        Assert.Contains("insufficient grounding", result.AbstainReason);
        Assert.Single(result.Citations); // the retrieved passages are still shown
    }

    [Fact]
    public async Task Failed_validation_abstains_with_the_validator_reason()
    {
        var runner = Build(
            walk: WalkWithOrigin,
            corroboration: new Corroboration(true, "fits"),
            passages: OnePassage,
            explain: ExplainOutcome.Answer(GoodDraft),
            validation: new Validation(false, "unknown entity FV-999"));

        var result = await runner.RunAsync(Incident, CancellationToken.None);

        Assert.True(result.Abstained);
        Assert.Contains("validation failed", result.AbstainReason);
        Assert.Contains("FV-999", result.AbstainReason);
        Assert.Single(result.Citations); // the passages that were retrieved are still shown
    }

    private static FixedIncidentDiagnosisRunner Build(
        GraphWalkResult walk,
        Corroboration corroboration,
        IReadOnlyList<Passage> passages,
        Validation validation,
        ExplainOutcome? explain = null,
        FakeExplainer? explainer = null,
        FakeStore? store = null,
        FakeAudit? audit = null) =>
        new(
            new FakeWalker(walk),
            new FakeCorroborator(corroboration),
            new FakeRetriever(passages),
            explainer ?? new FakeExplainer(explain ?? ExplainOutcome.Abstain("no draft configured")),
            new FakeValidator(validation),
            audit ?? new FakeAudit(),
            store ?? new FakeStore(),
            new FixedClock(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc)));

    private sealed class FakeWalker(GraphWalkResult result) : IGraphWalker
    {
        public Task<GraphWalkResult> WalkAsync(IncidentContext ctx, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class FakeCorroborator(Corroboration result) : ITelemetryCorroborator
    {
        public Task<Corroboration> CheckAsync(IncidentContext ctx, int originComponentId, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class FakeRetriever(IReadOnlyList<Passage> result) : IRetriever
    {
        public Task<IReadOnlyList<Passage>> RetrieveAsync(string queryText, string tagText, int unitId, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class FakeExplainer(ExplainOutcome outcome) : IExplainer
    {
        public bool WasCalled { get; private set; }

        public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(outcome);
        }
    }

    private sealed class FakeValidator(Validation result) : IAntiHallucinationValidator
    {
        public Task<Validation> ValidateAsync(DraftAnswer answer, IReadOnlyList<Passage> retrieved, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class FakeAudit : IAuditChainWriter
    {
        public string Hash { get; } = "deadbeef";

        public string? LastPayload { get; private set; }

        public Task<string> AppendAsync(string payload, CancellationToken cancellationToken)
        {
            LastPayload = payload;
            return Task.FromResult(Hash);
        }
    }

    private sealed class FakeStore : IDiagnosisRunStore
    {
        public DiagnosisRun? SavedRun { get; private set; }

        public IReadOnlyList<Candidate>? SavedCandidates { get; private set; }

        public Task<long> SaveAsync(DiagnosisRun run, IReadOnlyList<Candidate> candidates, CancellationToken cancellationToken)
        {
            SavedRun = run;
            SavedCandidates = candidates;
            return Task.FromResult(7L);
        }
    }

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }
}
