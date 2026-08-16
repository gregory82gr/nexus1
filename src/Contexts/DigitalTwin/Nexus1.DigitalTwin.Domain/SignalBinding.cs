using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Wires a model variable to a real instrumentation signal (atlas C.6.1/
/// C.6.2, C.6.8 query 2's own subject). Real invariant: EffectiveToUtc, when
/// set, must be later than EffectiveFromUtc (CK_DigitalTwin_SignalBinding_EffectiveRange).
///
/// SignalId gets a real SQL FOREIGN KEY at the Infrastructure layer to
/// Instrumentation.Signal via the InstrumentationSignalReference
/// shadow-entity technique (ADR-020, following ADR-019's precedent).
/// </summary>
public sealed class SignalBinding : Entity<SignalBindingId>, IAggregateRoot
{
    private SignalBinding(
        SignalBindingId id, TwinModelId twinModelId, TwinVariableId twinVariableId, int signalId,
        BindingRoleId bindingRoleId, BindingStatusId bindingStatusId, string modelVariable, decimal? scaleFactor,
        decimal? offsetValue, double? toleranceAbs, double? tolerancePercent, DateTime effectiveFromUtc,
        DateTime? effectiveToUtc, DateTime createdAtUtc)
        : base(id)
    {
        TwinModelId = twinModelId;
        TwinVariableId = twinVariableId;
        SignalId = signalId;
        BindingRoleId = bindingRoleId;
        BindingStatusId = bindingStatusId;
        ModelVariable = modelVariable;
        ScaleFactor = scaleFactor;
        OffsetValue = offsetValue;
        ToleranceAbs = toleranceAbs;
        TolerancePercent = tolerancePercent;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public TwinModelId TwinModelId { get; }

    public TwinVariableId TwinVariableId { get; }

    /// <summary>Instrumentation.Signal real FK (ADR-020).</summary>
    public int SignalId { get; }

    public BindingRoleId BindingRoleId { get; }

    public BindingStatusId BindingStatusId { get; }

    public string ModelVariable { get; }

    public decimal? ScaleFactor { get; }

    public decimal? OffsetValue { get; }

    public double? ToleranceAbs { get; }

    public double? TolerancePercent { get; }

    public DateTime EffectiveFromUtc { get; }

    public DateTime? EffectiveToUtc { get; }

    public DateTime CreatedAtUtc { get; }

    public static SignalBinding Create(
        SignalBindingId id, TwinModelId twinModelId, TwinVariableId twinVariableId, int signalId,
        BindingRoleId bindingRoleId, BindingStatusId bindingStatusId, string modelVariable, DateTime effectiveFromUtc,
        DateTime createdAtUtc, decimal? scaleFactor = null, decimal? offsetValue = null, double? toleranceAbs = null,
        double? tolerancePercent = null, DateTime? effectiveToUtc = null)
    {
        if (string.IsNullOrWhiteSpace(modelVariable))
        {
            throw new ArgumentException("SignalBinding model variable must not be empty.", nameof(modelVariable));
        }

        if (effectiveToUtc is not null && effectiveToUtc <= effectiveFromUtc)
        {
            throw new ArgumentException(
                "EffectiveToUtc must be later than EffectiveFromUtc when set (CK_DigitalTwin_SignalBinding_EffectiveRange).",
                nameof(effectiveToUtc));
        }

        return new SignalBinding(
            id, twinModelId, twinVariableId, signalId, bindingRoleId, bindingStatusId, modelVariable, scaleFactor,
            offsetValue, toleranceAbs, tolerancePercent, effectiveFromUtc, effectiveToUtc, createdAtUtc);
    }
}
