using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// High-volume raw time-series fact table (atlas C.5.4.7). Deliberately
/// narrow per the atlas's own design choice (C.5.1): SignalId, timestamp,
/// value, quality and source — rich metadata lives beside it, not in it.
/// Real invariant, not decoration: CK_Instrumentation_Measurement_OneValue —
/// NumericValue or TextValue must be present, never neither.
/// </summary>
public sealed class Measurement : Entity<MeasurementId>, IAggregateRoot
{
    private Measurement(
        MeasurementId id, SignalId signalId, SignalQualityId signalQualityId, MeasurementSourceId measurementSourceId,
        DateTime timestampUtc, double? numericValue, string? textValue, bool isEstimated, DateTime insertedAtUtc)
        : base(id)
    {
        SignalId = signalId;
        SignalQualityId = signalQualityId;
        MeasurementSourceId = measurementSourceId;
        TimestampUtc = timestampUtc;
        NumericValue = numericValue;
        TextValue = textValue;
        IsEstimated = isEstimated;
        InsertedAtUtc = insertedAtUtc;
    }

    public SignalId SignalId { get; }

    public SignalQualityId SignalQualityId { get; }

    public MeasurementSourceId MeasurementSourceId { get; }

    public DateTime TimestampUtc { get; }

    public double? NumericValue { get; }

    public string? TextValue { get; }

    public bool IsEstimated { get; }

    public DateTime InsertedAtUtc { get; }

    public static Measurement Create(
        MeasurementId id, SignalId signalId, SignalQualityId signalQualityId, MeasurementSourceId measurementSourceId,
        DateTime timestampUtc, DateTime insertedAtUtc, double? numericValue = null, string? textValue = null,
        bool isEstimated = false)
    {
        if (numericValue is null && string.IsNullOrEmpty(textValue))
        {
            throw new ArgumentException(
                "A Measurement must carry a NumericValue or a TextValue, not neither (CK_Instrumentation_Measurement_OneValue).");
        }

        return new Measurement(id, signalId, signalQualityId, measurementSourceId, timestampUtc, numericValue, textValue, isEstimated, insertedAtUtc);
    }
}
