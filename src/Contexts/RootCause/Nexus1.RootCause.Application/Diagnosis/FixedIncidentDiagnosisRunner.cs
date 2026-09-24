using System.Text.Json;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The full One-Truth Pipeline for a seeded incident (ADR-032, ADR-033; extended to
/// the registry of incidents in ADR-037 -- the per-incident query text and corpus
/// version travel on the <see cref="IncidentContext"/>, not as constants here). The
/// deterministic engine decides: the graph walk names the origin,
/// then two grounding gates (telemetry corroboration, corpus retrieval) each get
/// a veto. Only once those pass does the served model explain -- the
/// <see cref="IExplainer"/> seam turns the retrieved passages (the Unified
/// Context) into a structured draft, which the H3/H4 validator then checks. Any
/// gate failing -- including the model declining or its output failing to parse,
/// or the draft failing validation -- yields an abstention with a named reason
/// and no verdict, never a silent verdict and never a salvaged narration.
///
/// The runner itself is LLM-free: it depends only on the <see cref="IExplainer"/>
/// interface. Every implementation that talks to a model lives in
/// Nexus1.RootCause.Explain; a fixture explainer (tests) and the real Ollama-
/// backed one are composed and validated identically.
/// </summary>
public sealed class FixedIncidentDiagnosisRunner(
    IGraphWalker graphWalker,
    ITelemetryCorroborator corroborator,
    IRetriever retriever,
    IExplainer explainer,
    IAntiHallucinationValidator validator,
    IAuditChainWriter auditChain,
    IDiagnosisRunStore store,
    IDateTimeProvider clock)
{
    public async Task<DiagnosisResult> RunAsync(IncidentContext ctx, CancellationToken cancellationToken)
    {
        var walk = await graphWalker.WalkAsync(ctx, cancellationToken);

        var candidateRows = walk.Ranked
            .Select(c => new Candidate { ComponentId = c.ComponentId, Role = c.Role, Weight = c.Weight, Coverage = c.Coverage })
            .ToList();

        var origin = walk.Ranked.FirstOrDefault(c => c.Role == "origin");
        if (origin is null)
        {
            return await FinishAsync(ctx, verdict: null, abstain: "graph walk found no origin candidate", walk.Ranked, citations: [], draft: null, candidateRows, cancellationToken);
        }

        // Grounding gate 1 -- telemetry corroboration (fault injection: missing historian data).
        var corroboration = await corroborator.CheckAsync(ctx, origin.ComponentId, cancellationToken);
        if (!corroboration.TimingFits)
        {
            return await FinishAsync(ctx, verdict: null, abstain: $"telemetry corroboration failed: {corroboration.Detail}", walk.Ranked, citations: [], draft: null, candidateRows, cancellationToken);
        }

        // Grounding gate 2 -- corpus retrieval (fault injection: missing corpus chunk). H2: below the floor = nothing to ground on.
        var passages = await retriever.RetrieveAsync(ctx.QueryText, origin.Tag, ctx.UnitId, cancellationToken);
        if (passages.Count == 0)
        {
            return await FinishAsync(ctx, verdict: null, abstain: "no grounding: corpus retrieval returned no passages", walk.Ranked, citations: [], draft: null, candidateRows, cancellationToken);
        }

        // Generation (H1/H5/H6) -- the served model turns the Unified Context into
        // a structured draft. A model that declines, or output that will not parse,
        // is a real abstention (H8), not a salvage attempt.
        var outcome = await explainer.ExplainAsync(ctx, origin.Tag, passages, cancellationToken);
        if (outcome.Abstained)
        {
            return await FinishAsync(ctx, verdict: null, abstain: $"explain abstained: {outcome.AbstainReason}", walk.Ranked, passages, draft: null, candidateRows, cancellationToken);
        }

        // Grounding gate 3 -- H3/H4 validation of the model's draft (unregistered entity, or a claim citing a passage not retrieved).
        var validation = await validator.ValidateAsync(outcome.Draft!, passages, cancellationToken);
        if (!validation.Ok)
        {
            return await FinishAsync(ctx, verdict: null, abstain: $"validation failed: {validation.Reason}", walk.Ranked, passages, draft: null, candidateRows, cancellationToken);
        }

        return await FinishAsync(ctx, verdict: origin.Tag, abstain: null, walk.Ranked, passages, outcome.Draft, candidateRows, cancellationToken);
    }

    private async Task<DiagnosisResult> FinishAsync(
        IncidentContext ctx, string? verdict, string? abstain, IReadOnlyList<RankedCandidate> ranked,
        IReadOnlyList<Passage> citations, DraftAnswer? draft, List<Candidate> candidateRows, CancellationToken cancellationToken)
    {
        var run = new DiagnosisRun
        {
            IncidentId = ctx.IncidentId,
            StartedAtUtc = clock.UtcNow,
            Verdict = verdict,
            AbstainReason = abstain,
            CorpusVersion = ctx.CorpusVersion,
        };

        var runId = await store.SaveAsync(run, candidateRows, cancellationToken);

        // Seal inputs, the engine's conclusion (or abstention), the model's own
        // validated output, and its citations into the audit chain (H10). The
        // payload is canonical JSON so the same run seals to the same hash.
        var payload = JsonSerializer.Serialize(new
        {
            run.IncidentId,
            Verdict = verdict,
            AbstainReason = abstain,
            run.CorpusVersion,
            Candidates = ranked.Select(c => new { c.Tag, c.Role, c.Weight, c.Coverage }),
            Citations = citations.Select(p => new { p.ChunkId, p.SourceLabel }),
            Answer = draft is null
                ? null
                : new
                {
                    draft.CauseTag,
                    draft.Entities,
                    Claims = draft.Claims.Select(cl => new { cl.Text, cl.CitationChunkId }),
                },
        });
        var hash = await auditChain.AppendAsync(payload, cancellationToken);

        return new DiagnosisResult(runId, ctx.IncidentId, verdict, abstain, ranked, citations, hash);
    }
}
