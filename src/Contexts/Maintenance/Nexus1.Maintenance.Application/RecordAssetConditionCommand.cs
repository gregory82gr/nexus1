using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Writes one AssetCondition plus its AssetConditionMeasurement rows in a single operation (ADR-021) — matching how a condition assessment actually gets produced (a health/RUL opinion tied to real measured evidence).</summary>
public sealed record RecordAssetConditionCommand(
    int AssetId, int ConditionGradeId, DateTime AssessedAtUtc, IReadOnlyList<AssetConditionMeasurementRequest> Measurements,
    int? AssessedByUserId = null, decimal? HealthScorePercent = null, int? RemainingUsefulLifeDays = null,
    string? Basis = null, string? Notes = null)
    : ICommand<long>;
