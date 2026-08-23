namespace Nexus1.ReactorFleet.Application;

/// <summary>
/// Read-side finder (Blueprint_to_Core's Finder pattern, matching Organization's
/// ISitePlantHierarchyFinder/etc.) — separate from IRepository&lt;Unit, UnitId&gt;,
/// which is Add/Get-by-id only and cannot list. Introduced for the BFF walking
/// skeleton (ADR-030); ReactorFleet.Application had no queries before this.
/// </summary>
public interface IUnitFleetFinder
{
    Task<IReadOnlyList<UnitSummaryDto>> GetAllSummariesAsync(CancellationToken cancellationToken);

    Task<UnitDetailDto?> GetDetailByIdAsync(int unitId, CancellationToken cancellationToken);
}
