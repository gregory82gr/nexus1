using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The default embedder when no served model is wired (ADR-033): returns null,
/// so <see cref="EfRetriever"/>'s semantic path stays dormant and the engine
/// grounds lexically only. Replaced by the Ollama-backed embedder when
/// AddRootCauseExplain is composed. LLM-free by construction -- this is why the
/// deterministic Infrastructure can depend on the IEmbedder interface without any
/// served-model package.
/// </summary>
public sealed class NoOpEmbedder : IEmbedder
{
    public Task<IReadOnlyList<double>?> EmbedAsync(string text, EmbedKind kind, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<double>?>(null);
}

/// <summary>
/// The default explainer when no served model is wired (ADR-033): abstains with a
/// named reason rather than fabricating a narration. A host that composes the
/// deterministic engine but not Nexus1.RootCause.Explain therefore reaches the
/// generation stage and honestly declines -- inconclusive is never a silent pass,
/// and a missing model is never a silent verdict.
/// </summary>
public sealed class NoOpExplainer : IExplainer
{
    public Task<ExplainOutcome> ExplainAsync(IncidentContext ctx, string originTag, IReadOnlyList<Passage> passages, CancellationToken cancellationToken) =>
        Task.FromResult(ExplainOutcome.Abstain("no served model configured (Nexus1.RootCause.Explain not wired)"));
}
