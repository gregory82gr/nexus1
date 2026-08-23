using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

/// <summary>Per ReactorFleet UnitId (int), unlike GetAssetsByUnitQuery/GetLatestConditionPerAssetQuery (both fleet-wide despite their names).</summary>
public sealed record GetUnitAssetConditionsQuery(int UnitId) : IQuery<IReadOnlyList<UnitAssetConditionDto>>;
