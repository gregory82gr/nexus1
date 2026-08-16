using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// Maps a canonical signal to a raw acquisition point (atlas C.5.4.5).
/// Time-bounded history, the same behavior pattern already used for
/// Organization's DepartmentAssignment/TeamMembership (ADR-019):
/// EffectiveToUtc must never precede-or-equal EffectiveFromUtc, enforced
/// both at creation and on close-out (End).
/// </summary>
public sealed class SignalMapping : Entity<SignalMappingId>, IAggregateRoot
{
    private SignalMapping(
        SignalMappingId id, SignalId signalId, AcquisitionPointId acquisitionPointId, DateTime effectiveFromUtc,
        DateTime? effectiveToUtc, DateTime createdAtUtc)
        : base(id)
    {
        SignalId = signalId;
        AcquisitionPointId = acquisitionPointId;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public SignalId SignalId { get; }

    public AcquisitionPointId AcquisitionPointId { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public static SignalMapping Create(
        SignalMappingId id, SignalId signalId, AcquisitionPointId acquisitionPointId, DateTime effectiveFromUtc,
        DateTime createdAtUtc, DateTime? effectiveToUtc = null)
    {
        if (effectiveToUtc is { } end && end <= effectiveFromUtc)
        {
            throw new ArgumentException("EffectiveToUtc must be later than EffectiveFromUtc when set.", nameof(effectiveToUtc));
        }

        return new SignalMapping(id, signalId, acquisitionPointId, effectiveFromUtc, effectiveToUtc, createdAtUtc);
    }

    /// <summary>Closes out the mapping — re-validates the same effective-date-range invariant enforced at creation.</summary>
    public void End(DateTime effectiveToUtc)
    {
        if (effectiveToUtc <= EffectiveFromUtc)
        {
            throw new ArgumentException("EffectiveToUtc must be later than EffectiveFromUtc.", nameof(effectiveToUtc));
        }

        EffectiveToUtc = effectiveToUtc;
    }
}
