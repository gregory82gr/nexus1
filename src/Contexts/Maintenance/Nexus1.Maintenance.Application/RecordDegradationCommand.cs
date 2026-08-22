using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Writes one DegradationRecord plus its initial DegradationTrendPoint rows in a single operation (ADR-021) — mirroring RecordAssetConditionCommand's own AssetCondition/AssetConditionMeasurement pairing.</summary>
public sealed record RecordDegradationCommand(
    int AssetId, int DegradationMechanismId, int FindingSeverityId, DateTime DetectedAtUtc, string Description,
    IReadOnlyList<DegradationTrendPointRequest> TrendPoints, int? AssetComponentId = null, int? ConditionGradeId = null,
    int? DetectedByUserId = null, decimal? EstimatedRatePerYear = null)
    : ICommand<long>;
