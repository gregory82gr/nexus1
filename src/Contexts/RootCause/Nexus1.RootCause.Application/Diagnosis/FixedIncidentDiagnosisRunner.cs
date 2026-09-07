using System.Text.Json;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The One-Truth Pipeline for the fixed EVT-2026-0418 skeleton (ADR-032),
/// deterministic engine side only: graph walk decides the origin, then three
/// grounding gates (telemetry corroboration, corpus retrieval, H3/H4
/// validation of the supplied draft) each get a veto. Any gate failing yields
/// an abstention with a named reason and no verdict -- never a silent verdict.
/// The natural-language generation step is deferred (the served model); the
/// DraftAnswer is supplied by the caller and validated identically.
///
/// "The engine decides" = the origin from the graph walk. "The model explains"
/// = the deferred generation. This runner is entirely the engine side.
/// </summary>
public sealed class FixedIncidentDiagnosisRunner(
    IGraphWalker graphWalker,
    ITelemetryCorroborator corroborator,
    IRetriever retriever,
    IAntiHallucinationValidator validator,
    IAuditChainWriter auditChain,
    IDiagnosisRunStore store,
    IDateTimeProvider clock)
{
    /// <summary>What this run could read -- sealed into the audit payload (H10).</summary>
    public const string CorpusVersion = "evt-2026-0418-book-worked-example-v1";

    private const string QueryText = "feedwater control valve actuator latency cascade root cause";

    public async Task<DiagnosisResult> RunAsync(IncidentContext ctx, DraftAnswer draft, CancellationToken cancellationToken)
    {
        var walk = await graphWalker.WalkAsync(ctx, cancellationToken);

        var candidateRows = walk.Ranked
            .Select(c => new Candidate { ComponentId = c.ComponentId, Role = c.Role, Weight = c.Weight, Coverage = c.Coverage })
            .ToList();

        var origin = walk.Ranked.FirstOrDefault(c => c.Role == "origin");
        if (origin is null)
        {
            return await FinishAsync(ctx, verdict: null, abstain: "graph walk found no origin candidate", walk.Ranked, citations: [], candidateRows, cancellationToken);
        }

        // Grounding gate 1 -- telemetry corroboration (fault injection: missing historian data).
        var corroboration = await corroborator.CheckAsync(ctx, origin.ComponentId, cancellationToken);
        if (!corroboration.TimingFits)
        {
            return await FinishAsync(ctx, verdict: null, abstain: $"telemetry corroboration failed: {corroboration.Detail}", walk.Ranked, citations: [], candidateRows, cancellationToken);
        }

        // Grounding gate 2 -- corpus retrieval (fault injection: missing corpus chunk). H2: below the floor = nothing to ground on.
        var passages = await retriever.RetrieveAsync(QueryText, origin.Tag, ctx.UnitId, cancellationToken);
        if (passages.Count == 0)
        {
            return await FinishAsync(ctx, verdict: null, abstain: "no grounding: corpus retrieval returned no passages", walk.Ranked, citations: [], candidateRows, cancellationToken);
        }

        // Grounding gate 3 -- H3/H4 validation of the draft (fault injection: draft names an unregistered entity, or cites a passage not retrieved).
        var validation = await validator.ValidateAsync(draft, passages, cancellationToken);
        if (!validation.Ok)
        {
            return await FinishAsync(ctx, verdict: null, abstain: $"validation failed: {validation.Reason}", walk.Ranked, passages, candidateRows, cancellationToken);
        }

        return await FinishAsync(ctx, verdict: origin.Tag, abstain: null, walk.Ranked, passages, candidateRows, cancellationToken);
    }

    private async Task<DiagnosisResult> FinishAsync(
        IncidentContext ctx, string? verdict, string? abstain, IReadOnlyList<RankedCandidate> ranked,
        IReadOnlyList<Passage> citations, List<Candidate> candidateRows, CancellationToken cancellationToken)
    {
        var run = new DiagnosisRun
        {
            IncidentId = ctx.IncidentId,
            StartedAtUtc = clock.UtcNow,
            Verdict = verdict,
            AbstainReason = abstain,
            CorpusVersion = CorpusVersion,
        };

        var runId = await store.SaveAsync(run, candidateRows, cancellationToken);

        // Seal the conclusion (or abstention) into the audit chain (H10). The
        // payload is canonical JSON so the same run seals to the same hash.
        var payload = JsonSerializer.Serialize(new
        {
            run.IncidentId,
            Verdict = verdict,
            AbstainReason = abstain,
            run.CorpusVersion,
            Candidates = ranked.Select(c => new { c.Tag, c.Role, c.Weight, c.Coverage }),
            Citations = citations.Select(p => new { p.ChunkId, p.SourceLabel }),
        });
        var hash = await auditChain.AppendAsync(payload, cancellationToken);

        return new DiagnosisResult(runId, ctx.IncidentId, verdict, abstain, ranked, citations, hash);
    }
}
