using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>Persists a completed diagnosis run and its ranked candidates, returning the new run id.</summary>
public interface IDiagnosisRunStore
{
    Task<long> SaveAsync(DiagnosisRun run, IReadOnlyList<Candidate> candidates, CancellationToken cancellationToken);
}
