using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// Runs the diagnosis pipeline for a seeded incident and maps its result to the
/// route DTO (ADR-034, ADR-037). Hand-rolled dispatch, no MediatR (ADR-002-amend).
/// The incident id is resolved against <see cref="IncidentRegistry"/>; an unknown
/// id is a first-class refusal (Result.Failure -> 404 at the edge), so
/// EVT-2026-0420 (unregistered) still 404s. An abstention is a successful result
/// with Abstained = true (-> 200). Genuine infrastructure failures (model
/// unreachable, database down) surface as exceptions and are mapped to 503 at the
/// edge, never swallowed into a verdict.
/// </summary>
public sealed class RunFixedIncidentDiagnosisCommandHandler(FixedIncidentDiagnosisRunner runner)
    : ICommandHandler<RunFixedIncidentDiagnosisCommand, DiagnosisResponse>
{
    public async Task<Result<DiagnosisResponse>> Handle(RunFixedIncidentDiagnosisCommand command, CancellationToken cancellationToken)
    {
        if (!IncidentRegistry.TryGet(command.IncidentId, out var incident))
        {
            return Result<DiagnosisResponse>.Failure($"unknown incident '{command.IncidentId}'");
        }

        var result = await runner.RunAsync(incident, cancellationToken);

        var response = new DiagnosisResponse(
            result.DiagnosisRunId,
            result.IncidentId,
            result.Verdict,
            result.Abstained,
            result.AbstainReason,
            result.Candidates.Select(c => new DiagnosisCandidateDto(c.Tag, c.Role, c.Weight, c.Coverage)).ToList(),
            result.Citations.Select(p => new DiagnosisCitationDto(p.ChunkId, p.SourceLabel)).ToList(),
            result.AuditHash);

        return Result<DiagnosisResponse>.Success(response);
    }
}
