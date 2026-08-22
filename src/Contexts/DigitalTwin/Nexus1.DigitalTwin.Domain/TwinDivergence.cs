using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Where modelled value and measured value disagree — the "conscience
/// table" of the sector (atlas C.6.1's own design choice: "The divergence
/// table is not an error log. It is a first-class data object. A twin that
/// hides disagreement becomes unsafe to trust; a twin that records
/// disagreement can be corrected, audited and used honestly.").
///
/// Real invariant, the sector's defining behavior (ADR-020): DeltaValue is
/// COMPUTED by this factory as MeasuredValue - ModeledValue, never accepted
/// as an independent caller-supplied parameter — even though the atlas's
/// own DDL does not mark DeltaValue as a SQL computed column (unlike
/// Organization.StaffingScenarioGap.GapCount), the book's own worked
/// TwinDivergence examples (From_Domain_to_Twin Ch.14/Ch.23:
/// "public decimal Difference => MeasuredValue - ModelValue;") make the
/// domain intent unambiguous: a twin that could disagree with its own
/// arithmetic about how much it disagrees with reality would undermine the
/// sector's entire premise. DeltaPercent is left caller-supplied (nullable,
/// threshold-relative — no single correct derivation the atlas specifies).
///
/// SignalId gets a real SQL FOREIGN KEY at the Infrastructure layer to
/// Instrumentation.Signal via the InstrumentationSignalReference
/// shadow-entity technique (ADR-020, following ADR-019's precedent).
/// </summary>
public sealed class TwinDivergence : Entity<TwinDivergenceId>, IAggregateRoot
{
    private TwinDivergence(
        TwinDivergenceId id, TwinSnapshotId twinSnapshotId, int signalId, TwinVariableId? twinVariableId,
        DivergenceSeverityId divergenceSeverityId, DivergenceStatusId divergenceStatusId, DateTime detectedAtUtc,
        double modeledValue, double measuredValue, double deltaValue, decimal? deltaPercent, double? thresholdAbs,
        decimal? thresholdPercent, string? explanation)
        : base(id)
    {
        TwinSnapshotId = twinSnapshotId;
        SignalId = signalId;
        TwinVariableId = twinVariableId;
        DivergenceSeverityId = divergenceSeverityId;
        DivergenceStatusId = divergenceStatusId;
        DetectedAtUtc = detectedAtUtc;
        ModeledValue = modeledValue;
        MeasuredValue = measuredValue;
        DeltaValue = deltaValue;
        DeltaPercent = deltaPercent;
        ThresholdAbs = thresholdAbs;
        ThresholdPercent = thresholdPercent;
        Explanation = explanation;
    }

    public TwinSnapshotId TwinSnapshotId { get; }

    /// <summary>Instrumentation.Signal real FK (ADR-020) — the measured signal that disagreed.</summary>
    public int SignalId { get; }

    public TwinVariableId? TwinVariableId { get; }

    public DivergenceSeverityId DivergenceSeverityId { get; }

    public DivergenceStatusId DivergenceStatusId { get; }

    public DateTime DetectedAtUtc { get; }

    public double ModeledValue { get; }

    public double MeasuredValue { get; }

    /// <summary>Computed by this factory as MeasuredValue - ModeledValue, never accepted as a caller-supplied value.</summary>
    public double DeltaValue { get; }

    public decimal? DeltaPercent { get; }

    public double? ThresholdAbs { get; }

    public decimal? ThresholdPercent { get; }

    public string? Explanation { get; }

    public static TwinDivergence Create(
        TwinDivergenceId id, TwinSnapshotId twinSnapshotId, int signalId, DivergenceSeverityId divergenceSeverityId,
        DivergenceStatusId divergenceStatusId, DateTime detectedAtUtc, double modeledValue, double measuredValue,
        TwinVariableId? twinVariableId = null, decimal? deltaPercent = null, double? thresholdAbs = null,
        decimal? thresholdPercent = null, string? explanation = null)
    {
        var deltaValue = measuredValue - modeledValue;

        return new TwinDivergence(
            id, twinSnapshotId, signalId, twinVariableId, divergenceSeverityId, divergenceStatusId, detectedAtUtc,
            modeledValue, measuredValue, deltaValue, deltaPercent, thresholdAbs, thresholdPercent, explanation);
    }
}
