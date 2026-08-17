namespace Nexus1.Maintenance.Application;

/// <summary>Atlas C.9.5.2 query 4 projection, verbatim: AssetCode, Name, AssessedAtUtc, ConditionGrade, HealthScorePercent, RemainingUsefulLifeDays — one row per asset, the latest AssetCondition (if any).</summary>
public sealed record LatestConditionDto(
    string AssetCode, string Name, DateTime? AssessedAtUtc, string? ConditionGrade,
    decimal? HealthScorePercent, int? RemainingUsefulLifeDays);
