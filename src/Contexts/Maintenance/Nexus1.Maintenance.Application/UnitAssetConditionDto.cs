namespace Nexus1.Maintenance.Application;

/// <summary>
/// Shaped for the BFF's Rod Inspection cluster (Inspection Overview, NDT
/// Methods, Rod Type/Film — three of the book's screens). Maintenance's
/// domain model has no rod-specific entity anywhere — no control-rod type,
/// no NDT-method taxonomy, no film/rod-type table. It is a generic
/// asset/condition model (any maintainable equipment item, generic
/// category/status lookups); this DTO is exactly that generic shape, not a
/// rod-specific one. NDT Methods and Rod Type/Film have nothing to map to
/// at all — not missing fields on this DTO, but concepts absent from the
/// schema entirely (see IAssetsByUnitFinder.GetAssetConditionsForUnitAsync's
/// doc comment). Latest* fields are nullable — an asset can exist with zero
/// recorded condition assessments yet.
/// </summary>
public sealed record UnitAssetConditionDto(
    string AssetCode,
    string Name,
    string Category,
    string Status,
    bool IsSafetyRelated,
    DateTime? LatestAssessedAtUtc,
    string? LatestConditionGrade,
    decimal? LatestHealthScorePercent,
    int? LatestRemainingUsefulLifeDays);
