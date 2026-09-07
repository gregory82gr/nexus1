using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Domain.Grounding;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// Persists a completed diagnosis run and its ranked candidates (App C.2). The
/// run is saved first so the database generates its identity, then each
/// candidate is stamped with that identity and saved -- the ruled-out FT-7 row
/// included, never discarded, so the record shows what was dismissed as well as
/// what was concluded.
/// </summary>
public sealed class EfDiagnosisRunStore(RootCauseDbContext db) : IDiagnosisRunStore
{
    public async Task<long> SaveAsync(DiagnosisRun run, IReadOnlyList<Candidate> candidates, CancellationToken cancellationToken)
    {
        db.DiagnosisRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            candidate.DiagnosisRunId = run.DiagnosisRunId;
        }

        db.Candidates.AddRange(candidates);
        await db.SaveChangesAsync(cancellationToken);

        return run.DiagnosisRunId;
    }
}
