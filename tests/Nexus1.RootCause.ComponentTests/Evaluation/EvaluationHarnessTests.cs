using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;
using Xunit.Abstractions;

namespace Nexus1.RootCause.ComponentTests.Evaluation;

/// <summary>
/// The H9 evaluation harness, deterministic tier (ADR-041; From Flood to Cause, Appendix J).
/// Every case in Evaluation/Cases/*.json with tier "deterministic" runs here under its own
/// name, against a real LocalDB with the real seeded fixture and the real seams -- walker,
/// corroborator, retriever, validator, audit chain, run store. The only stand-in is the
/// explainer (<see cref="CaseExplainer"/>): the golden good draft, or the case's injected
/// trap draft. No model is involved, so this tier never flakes and always runs.
///
/// A case with a named `skip` reason is reported Skipped under its own id, never silently
/// absent (trap-leading-question: no CauseTag-vs-origin check exists yet).
/// </summary>
public class EvaluationHarnessTests(ITestOutputHelper output) : RootCauseComponentTestDatabase
{
    private static readonly EvalClock Clock = new(new DateTime(2026, 4, 18, 17, 12, 14, DateTimeKind.Utc));

    public static TheoryData<string> Cases => EvalCaseCatalog.IdsFor("deterministic");

    [SkippableTheory]
    [MemberData(nameof(Cases))]
    public async Task Case(string id)
    {
        var c = EvalCaseCatalog.Get(id);
        Skip.If(c.Skip is not null, c.Skip);
        output.WriteLine($"[{c.Id}] {c.Category} / {c.Seam} / {c.Target.IncidentId} -- intent (documentation only): {c.Intent}");

        await using (var seed = CreateDbContext())
        {
            await GroundingSeed.SeedAsync(seed);
        }

        switch (c.Seam)
        {
            case "pipeline":
                await RunPipelineCaseAsync(c);
                break;
            case "record":
                await RunRecordCaseAsync(c);
                break;
            default:
                throw new InvalidOperationException($"{c.Id}: seam '{c.Seam}' is not runnable on the deterministic tier.");
        }
    }

    private async Task RunPipelineCaseAsync(EvalCase c)
    {
        await using var db = CreateDbContext();
        var corpus = await db.CorpusChunks.Select(x => new CorpusRef(x.ChunkId, x.SourceLabel)).ToListAsync();
        var runner = BuildRunner(db, new CaseExplainer(c.InjectedDraft, corpus));

        if (c.Expect == "refused")
        {
            // Through the real command handler: the incident is outside the registry, so the
            // engine is never reached -- a refusal, not an abstention and not a verdict.
            var refused = await new RunFixedIncidentDiagnosisCommandHandler(runner)
                .Handle(new RunFixedIncidentDiagnosisCommand(c.Target.IncidentId), CancellationToken.None);

            Assert.True(refused.IsFailure, $"{c.Id}: expected the handler to refuse {c.Target.IncidentId}.");
            Assert.Contains(c.ExpectReasonContains.Single(), refused.Error);
            Assert.Equal(0, await db.DiagnosisRuns.CountAsync(r => r.IncidentId == c.Target.IncidentId));
            output.WriteLine($"[{c.Id}] refused: {refused.Error}");
            return;
        }

        Assert.True(IncidentRegistry.TryGet(c.Target.IncidentId, out var incident), $"{c.Id}: target not in the registry.");
        var result = await runner.RunAsync(incident, CancellationToken.None);
        output.WriteLine($"[{c.Id}] verdict={result.Verdict ?? "-"} abstain={result.AbstainReason ?? "-"} citations=[{string.Join(" | ", result.Citations.Select(p => p.SourceLabel))}]");

        // Every run -- verdict or abstention -- is persisted and sealed.
        Assert.NotEmpty(result.AuditHash);
        await using var verify = CreateDbContext();
        var run = await verify.DiagnosisRuns.SingleAsync(r => r.DiagnosisRunId == result.DiagnosisRunId);
        Assert.True(await verify.AuditEntries.AnyAsync(a => a.Hash == result.AuditHash), $"{c.Id}: run was not sealed into the audit chain.");
        Assert.Equal(incident.CorpusVersion, run.CorpusVersion);

        EvalAssertions.MustNotAssert(c, result);

        switch (c.Expect)
        {
            case "verdict":
                Assert.False(result.Abstained, $"{c.Id}: unexpected abstention: {result.AbstainReason}");
                Assert.Equal(c.ExpectVerdict, result.Verdict);
                Assert.Equal(c.ExpectVerdict, run.Verdict);
                Assert.Null(run.AbstainReason);
                Assert.True(await verify.Candidates.AnyAsync(x => x.DiagnosisRunId == result.DiagnosisRunId), $"{c.Id}: no candidates persisted.");
                AssertCandidates(c, result);
                EvalAssertions.MustCite(c, result.Citations);
                EvalAssertions.MustNotCite(c, result.Citations);
                break;

            case "abstain":
                Assert.True(result.Abstained, $"{c.Id}: expected an abstention, got verdict {result.Verdict}.");
                Assert.Null(result.Verdict);
                Assert.Null(run.Verdict);
                Assert.Contains(c.ExpectAbstainReasonContains!, result.AbstainReason);
                break;

            default:
                throw new InvalidOperationException($"{c.Id}: expect '{c.Expect}' is not valid for a deterministic pipeline case.");
        }
    }

    private async Task RunRecordCaseAsync(EvalCase c)
    {
        // The human case record (ADR-040): 0420 is not solvable by the engine, so what is
        // evaluated is the provisioned Inconclusive record -- built by the SAME holder the
        // host provisioning uses, persisted, and read back from LocalDB.
        Assert.Equal(Evt20260420QaEscape.IncidentId, c.Target.IncidentId);
        Assert.False(IncidentRegistry.TryGet(c.Target.IncidentId, out _), $"{c.Id}: 0420 must stay outside the engine's registry.");

        await using (var arrange = CreateDbContext())
        {
            await arrange.RootCauseAnalyses.AddAsync(Evt20260420QaEscape.BuildCase());
            await arrange.SaveChangesAsync();
        }

        await using var verify = CreateDbContext();
        var id = new RootCauseAnalysisId(Evt20260420QaEscape.AnalysisId);
        var analysis = await verify.RootCauseAnalyses.SingleAsync(a => a.Id == id);
        output.WriteLine($"[{c.Id}] status={analysis.Status} verdict={analysis.Verdict ?? "-"} flood={analysis.AlarmFloodId?.Value.ToString() ?? "-"} reason={analysis.InconclusiveReason}");

        Assert.Equal("inconclusive", c.Expect);
        Assert.Equal(AnalysisStatus.Inconclusive, analysis.Status);
        Assert.Null(analysis.Verdict);
        Assert.Null(analysis.AlarmFloodId);
        Assert.Equal(Evt20260420QaEscape.UnitId, analysis.UnitId.Value);
        foreach (var fragment in c.ExpectReasonContains)
        {
            Assert.Contains(fragment, analysis.InconclusiveReason);
        }

        foreach (var tag in c.MustNotAssert)
        {
            Assert.NotEqual(tag, analysis.Verdict);
        }
    }

    private static void AssertCandidates(EvalCase c, DiagnosisResult result)
    {
        if (c.ExpectCandidates.Count > 0)
        {
            Assert.Equal(
                c.ExpectCandidates.Select(x => $"{x.Tag}:{x.Role}:{x.Weight:0.00}"),
                result.Candidates.Select(x => $"{x.Tag}:{x.Role}:{x.Weight:0.00}"));
        }

        if (c.ExpectOriginCoverage is { } coverage)
        {
            var parts = coverage.Split('/');
            var numerator = int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
            var denominator = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            var origin = Assert.Single(result.Candidates, x => x.Role == "origin");
            Assert.Equal(numerator, (int)Math.Round(origin.Coverage * denominator));
        }
    }

    private FixedIncidentDiagnosisRunner BuildRunner(RootCauseDbContext db, IExplainer explainer) =>
        new(
            new EfGraphWalker(db),
            new EfTelemetryCorroborator(db),
            new EfRetriever(db, new NoOpEmbedder()),
            explainer,
            new RegistryAntiHallucinationValidator(db),
            new Sha256AuditChainWriter(db, Clock),
            new EfDiagnosisRunStore(db),
            Clock,
            NewDiagnosticsMetrics());
}
