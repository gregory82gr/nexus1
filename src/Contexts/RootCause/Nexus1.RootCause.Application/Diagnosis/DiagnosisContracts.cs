namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Contracts for the fixed-incident diagnosis walking skeleton (ADR-032).
/// All LLM-free: the only stage that needs the served model is natural-language
/// generation, which is deferred (the DraftAnswer below is supplied by the
/// caller -- a fixture now, the model later -- and validated by H3/H4 either
/// way).
/// </summary>

/// <summary>
/// The input to one diagnosis run -- which incident, which unit, which components
/// alarmed, when the flood began, and the per-incident retrieval query text and
/// corpus version (moved off the runner's constants so more than one incident can
/// run; ADR-037). Each seeded incident has exactly one of these in
/// <see cref="IncidentRegistry"/>.
/// </summary>
public sealed record IncidentContext(
    string IncidentId,
    int UnitId,
    IReadOnlyList<int> AlarmedComponentIds,
    DateTime FloodStartUtc,
    string QueryText,
    string CorpusVersion);

/// <summary>One node-test result: derived role, attribution-share weight, and coverage (share of the flood explained) -- ADR-042.</summary>
public sealed record RankedCandidate(int ComponentId, string Tag, string Role, double Weight, double Coverage);

/// <summary>Ranked candidates, origin first. The origin is the highest-coverage node with a clean timing fit.</summary>
public sealed record GraphWalkResult(IReadOnlyList<RankedCandidate> Ranked);

/// <summary>Whether the origin hypothesis's cascade timing fits the modelled delay windows, with a human-readable detail.</summary>
public sealed record Corroboration(bool TimingFits, string Detail);

/// <summary>One retrieved corpus passage, carrying its honest source label for the citation line.</summary>
public sealed record Passage(long ChunkId, string Body, string SourceLabel, byte TrustTier);

/// <summary>One claim in a draft answer, citing a corpus chunk by id (H4 checks the id is in the retrieved set).</summary>
public sealed record Claim(string Text, long CitationChunkId);

/// <summary>
/// The explain step's output shape (H5). In the skeleton it is supplied by the
/// caller as a fixture standing in for the deferred served model; the pipeline
/// validates it identically regardless of origin.
/// </summary>
public sealed record DraftAnswer(string CauseTag, IReadOnlyList<string> Entities, IReadOnlyList<Claim> Claims);

/// <summary>Validator outcome (H3 + H4). A single failure abstains the whole answer.</summary>
public sealed record Validation(bool Ok, string? Reason);

/// <summary>
/// The explain stage's result: either a structured draft to validate, or an
/// abstention with a named reason (H8) -- the model declined, or its output could
/// not be parsed into the H5 schema. Never both.
/// </summary>
public sealed record ExplainOutcome(DraftAnswer? Draft, string? AbstainReason)
{
    public bool Abstained => AbstainReason is not null;

    public static ExplainOutcome Answer(DraftAnswer draft) => new(draft, null);

    public static ExplainOutcome Abstain(string reason) => new(null, reason);
}

/// <summary>
/// The engine's outcome for one run: either a verdict (origin tag) with its
/// ranked candidates and the retrieved citations, or an abstention with a
/// named reason -- never both, matching this project's Inconclusive-is-never-a-
/// silent-Pass discipline.
/// </summary>
public sealed record DiagnosisResult(
    long DiagnosisRunId,
    string IncidentId,
    string? Verdict,
    string? AbstainReason,
    IReadOnlyList<RankedCandidate> Candidates,
    IReadOnlyList<Passage> Citations,
    string AuditHash)
{
    public bool Abstained => AbstainReason is not null;
}
