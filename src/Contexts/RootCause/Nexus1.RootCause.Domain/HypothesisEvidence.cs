using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.RootCause.Domain;

/// <summary>
/// Phase 1 slice only (ADR-005): a plain description, not the atlas's typed
/// EvidenceType/WitnessType and optional FKs to AlarmEvent/Signal/Measurement/
/// Equipment/WorkOrder/InspectionFinding.
/// </summary>
public sealed class HypothesisEvidence : Entity<HypothesisEvidenceId>
{
    internal HypothesisEvidence(HypothesisEvidenceId id, string description, DateTime recordedAtUtc)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Evidence description must not be empty.", nameof(description));
        }

        Description = description;
        RecordedAtUtc = recordedAtUtc;
    }

    public string Description { get; }

    public DateTime RecordedAtUtc { get; }
}
