namespace Nexus1.Maintenance.Application;

public interface IAssetsByUnitFinder
{
    Task<IReadOnlyList<AssetByUnitDto>> GetAssetsByUnitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Keyed by ReactorFleet UnitId (int) directly, filtering Asset.UnitId
    /// rather than joining through ReactorFleetUnitReference.Code — added
    /// for the BFF's Rod Inspection cluster (ADR-030 follow-up). Includes
    /// each asset's latest condition assessment, which GetAssetsByUnitAsync
    /// alone doesn't. Includes an asset with zero condition assessments
    /// (null Latest* fields) rather than excluding it, same reasoning as
    /// every other per-unit "latest X" finder added so far.
    /// </summary>
    Task<IReadOnlyList<UnitAssetConditionDto>> GetAssetConditionsForUnitAsync(int unitId, CancellationToken cancellationToken);
}
