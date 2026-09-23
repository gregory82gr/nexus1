using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Application.Diagnosis;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Diagnosis;

/// <summary>
/// The validator of Listing 9.5 (ADR-032), the mechanical core of the
/// anti-hallucination engine. It decides nothing and generates nothing; it only
/// refuses. H3: every entity a draft answer names must exist in the component
/// registry (by tag), or a mistyped or invented component would reach the
/// operator dressed as fact. H4: every claim must cite a passage that is
/// actually in the retrieved set, or the answer has drifted beyond its evidence.
/// A single failure abstains the whole answer -- inconclusive is never a silent
/// pass. This validates a draft identically whether the draft came from a
/// fixture (today) or the deferred served model (later).
/// </summary>
public sealed class RegistryAntiHallucinationValidator(RootCauseDbContext db) : IAntiHallucinationValidator
{
    public async Task<Validation> ValidateAsync(DraftAnswer answer, IReadOnlyList<Passage> retrieved, CancellationToken cancellationToken)
    {
        var registryTags = await db.Components
            .Select(c => c.Tag)
            .ToListAsync(cancellationToken);
        var registry = registryTags.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // H3: every named entity must exist in the registry.
        foreach (var entity in answer.Entities)
        {
            if (!registry.Contains(entity))
            {
                return new Validation(false, $"unknown entity {entity}");
            }
        }

        // H4: every claim must cite a passage in the retrieved set.
        var retrievedIds = retrieved.Select(p => p.ChunkId).ToHashSet();
        foreach (var claim in answer.Claims)
        {
            if (!retrievedIds.Contains(claim.CitationChunkId))
            {
                return new Validation(false, $"uncited claim citing chunk {claim.CitationChunkId}");
            }
        }

        return new Validation(true, null);
    }
}
