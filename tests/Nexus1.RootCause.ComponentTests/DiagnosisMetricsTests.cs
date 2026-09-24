using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.ComponentTests;

/// <summary>
/// The Appendix-I diagnosis metrics (ADR-038), proven deterministically against the
/// real engine seams over LocalDB with a fixture explainer -- no live model, no
/// Prometheus. A MeterListener scoped to THIS runner's exact instrument instances
/// (identity match, so parallel test classes running the runner cannot pollute it)
/// captures what the runner emits. This is the deterministic evidence that the four
/// instruments move on the right events; the live /metrics + Grafana evidence proves
/// the export path separately.
///
/// grounding_hit is a PROXY, never recall (ADR-038): it is 1.0 when retrieval returned
/// a passage and 0.0 when it ran and returned none, and is absent when a run abstained
/// before retrieval.
/// </summary>
public class DiagnosisMetricsTests : RootCauseComponentTestDatabase
{
    private static readonly IncidentContext Incident = IncidentRegistry.Evt20260418;

    [Fact]
    public async Task Verdict_run_records_duration_and_grounding_hit_and_no_abstention()
    {
        await SeedAsync();
        var metrics = NewDiagnosticsMetrics();
        using var capture = new Capture(metrics);

        await using (var db = CreateDbContext())
        {
            var draft = await GoodDraftAsync(db);
            var result = await BuildRunner(db, metrics, ExplainOutcome.Answer(draft)).RunAsync(Incident, CancellationToken.None);
            Assert.False(result.Abstained);
        }

        Assert.Equal(1, capture.DurationCount);
        Assert.Equal(0, capture.Abstentions);
        Assert.Equal(0, capture.ValidatorRejections);
        Assert.Contains(1.0, capture.GroundingHits); // retrieval found grounding
    }

    [Fact]
    public async Task Abstention_run_increments_the_abstention_counter()
    {
        await SeedAsync();
        await using (var arrange = CreateDbContext())
        {
            await arrange.CorpusChunks.Where(c => c.UnitId == GroundingSeed.UnitId).ExecuteDeleteAsync();
        }

        var metrics = NewDiagnosticsMetrics();
        using var capture = new Capture(metrics);

        await using (var db = CreateDbContext())
        {
            var result = await BuildRunner(db, metrics, ExplainOutcome.Answer(new DraftAnswer("FV-104", ["FV-104"], []))).RunAsync(Incident, CancellationToken.None);
            Assert.True(result.Abstained);
        }

        Assert.Equal(1, capture.Abstentions);
        Assert.Equal(0, capture.ValidatorRejections);
        Assert.Equal(1, capture.DurationCount);            // latency recorded for every run
        Assert.Contains(0.0, capture.GroundingHits);       // retrieval ran, found nothing
    }

    [Fact]
    public async Task Validator_rejection_increments_both_the_rejection_and_abstention_counters()
    {
        await SeedAsync();
        var metrics = NewDiagnosticsMetrics();
        using var capture = new Capture(metrics);

        await using (var db = CreateDbContext())
        {
            // Deterministic bad draft (fixture, NOT a live-model hallucination): names an
            // entity that is not in the registry -> H3 rejects it.
            var citable = await db.CorpusChunks.Where(c => c.UnitId == GroundingSeed.UnitId).OrderBy(c => c.ChunkId).Select(c => c.ChunkId).FirstAsync();
            var badDraft = new DraftAnswer("FV-104", ["FV-999"], [new Claim("bogus", citable)]);
            var result = await BuildRunner(db, metrics, ExplainOutcome.Answer(badDraft)).RunAsync(Incident, CancellationToken.None);
            Assert.True(result.Abstained);
            Assert.Contains("validation failed", result.AbstainReason);
        }

        Assert.Equal(1, capture.ValidatorRejections);
        Assert.Equal(1, capture.Abstentions); // a validation failure is also an abstention -- both move
    }

    // ----- helpers -----

    private async Task SeedAsync()
    {
        await using var db = CreateDbContext();
        await GroundingSeed.SeedAsync(db);
    }

    private static FixedIncidentDiagnosisRunner BuildRunner(RootCauseDbContext db, NexusDiagnosticsMetrics metrics, ExplainOutcome outcome) =>
        new(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            new EfRetriever(db, new NoOpEmbedder()),
            new FixtureExplainer(outcome),
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock,
            metrics);

    private static async Task<DraftAnswer> GoodDraftAsync(RootCauseDbContext db)
    {
        var chunkId = await db.CorpusChunks.Where(c => c.UnitId == GroundingSeed.UnitId).OrderBy(c => c.ChunkId).Select(c => c.ChunkId).FirstAsync();
        return new DraftAnswer("FV-104", ["FV-104"], [new Claim("FV-104 actuator latency initiated the feedwater cascade", chunkId)]);
    }

    private sealed class FixtureExplainer(ExplainOutcome outcome) : IExplainer
    {
        public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken) => Task.FromResult(outcome);
    }

    private static readonly IDateTimeProvider Clock = new FixedClock(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc));

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }

    /// <summary>A MeterListener scoped to one runner's exact instrument instances (identity match), so parallel test classes emitting on the same-named meter cannot pollute the counts.</summary>
    private sealed class Capture : IDisposable
    {
        private readonly MeterListener _listener = new();
        private readonly NexusDiagnosticsMetrics _metrics;

        public long Abstentions { get; private set; }
        public long ValidatorRejections { get; private set; }
        public int DurationCount { get; private set; }
        public List<double> GroundingHits { get; } = [];

        public Capture(NexusDiagnosticsMetrics metrics)
        {
            _metrics = metrics;
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (ReferenceEquals(instrument, _metrics.Abstentions)
                    || ReferenceEquals(instrument, _metrics.ValidatorRejections)
                    || ReferenceEquals(instrument, _metrics.Duration)
                    || ReferenceEquals(instrument, _metrics.GroundingHit))
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _listener.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
            {
                if (ReferenceEquals(instrument, _metrics.Abstentions)) { Abstentions += value; }
                else if (ReferenceEquals(instrument, _metrics.ValidatorRejections)) { ValidatorRejections += value; }
            });
            _listener.SetMeasurementEventCallback<double>((instrument, value, _, _) =>
            {
                if (ReferenceEquals(instrument, _metrics.Duration)) { DurationCount++; }
                else if (ReferenceEquals(instrument, _metrics.GroundingHit)) { GroundingHits.Add(value); }
            });
            _listener.Start();
        }

        public void Dispose() => _listener.Dispose();
    }
}
