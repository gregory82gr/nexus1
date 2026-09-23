using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Run the One-Truth Pipeline for the fixed incident and return its sealed result
/// (ADR-034). A command, not a query: each run writes a new DiagnosisRun, its
/// candidates, and a new audit link. Only EVT-2026-0418 is valid.
/// </summary>
public sealed record RunFixedIncidentDiagnosisCommand(string IncidentId) : ICommand<DiagnosisResponse>;

/// <summary>
/// The route-facing shape of a diagnosis run (ADR-034): a verdict or a first-class
/// abstention, the ranked candidates (the ruled-out one included), the citations
/// with their honest source labels, and the audit-chain head that sealed it.
/// Citations carry only id + source label, not the full passage body.
/// </summary>
public sealed record DiagnosisResponse(
    long DiagnosisRunId,
    string IncidentId,
    string? Verdict,
    bool Abstained,
    string? AbstainReason,
    IReadOnlyList<DiagnosisCandidateDto> Candidates,
    IReadOnlyList<DiagnosisCitationDto> Citations,
    string AuditHash);

public sealed record DiagnosisCandidateDto(string Tag, string Role, double Weight, double? Coverage);

public sealed record DiagnosisCitationDto(long ChunkId, string SourceLabel);
