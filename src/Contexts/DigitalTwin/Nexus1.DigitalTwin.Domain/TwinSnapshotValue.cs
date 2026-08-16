using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// One model variable value inside a snapshot (atlas C.6.1/C.6.2). Real
/// invariant: at least one of NumericValue/TextValue/JsonValue must be set
/// (CK_DigitalTwin_TwinSnapshotValue_OneValue) — the same "a value without
/// substance is not evidence" shape as Instrumentation.Measurement's
/// CK_Instrumentation_Measurement_OneValue.
///
/// Leaner audit shape than most tables in this sector — the atlas DDL gives
/// this table only CreatedAtUtc (no RowVersion), verified directly against
/// C.6.4.5. Not modeled here: it is pure row-insertion bookkeeping with a
/// SQL DEFAULT, no domain behavior depends on it.
/// </summary>
public sealed class TwinSnapshotValue : Entity<TwinSnapshotValueId>, IAggregateRoot
{
    private TwinSnapshotValue(
        TwinSnapshotValueId id, TwinSnapshotId twinSnapshotId, TwinVariableId twinVariableId, double? numericValue,
        string? textValue, string? jsonValue)
        : base(id)
    {
        TwinSnapshotId = twinSnapshotId;
        TwinVariableId = twinVariableId;
        NumericValue = numericValue;
        TextValue = textValue;
        JsonValue = jsonValue;
    }

    public TwinSnapshotId TwinSnapshotId { get; }

    public TwinVariableId TwinVariableId { get; }

    public double? NumericValue { get; }

    public string? TextValue { get; }

    public string? JsonValue { get; }

    public static TwinSnapshotValue Create(
        TwinSnapshotValueId id, TwinSnapshotId twinSnapshotId, TwinVariableId twinVariableId,
        double? numericValue = null, string? textValue = null, string? jsonValue = null)
    {
        if (numericValue is null && string.IsNullOrEmpty(textValue) && string.IsNullOrEmpty(jsonValue))
        {
            throw new ArgumentException(
                "A TwinSnapshotValue must carry a NumericValue, TextValue or JsonValue, not none of the three " +
                "(CK_DigitalTwin_TwinSnapshotValue_OneValue).");
        }

        return new TwinSnapshotValue(id, twinSnapshotId, twinVariableId, numericValue, textValue, jsonValue);
    }
}
