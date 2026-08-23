namespace Nexus1.Organization.Application;

/// <summary>
/// New finder for the BFF's Personnel screen (ADR-030 follow-up). Scoped to
/// a Department, not a ReactorFleet unit, because there is no such
/// connection in this domain model at all — not even passport-only.
/// Plant.cs's own doc comment records this explicitly: "ReactorFleet.Unit
/// will later carry its passport to this table through PlantId... that
/// wiring is not performed by this ADR" (ADR-017). A "per-unit personnel
/// roster" endpoint would have nothing real to query; a per-Department
/// roster is what Organization's own hierarchy (Department -&gt;
/// DepartmentAssignment -&gt; Person) actually supports, so that's what this
/// endpoint is shaped around instead.
/// </summary>
public interface IDepartmentRosterFinder
{
    /// <summary>Currently active assignments only (EndDate IS NULL) — mirrors IPersonOrganizationContextFinder's own "current" convention.</summary>
    Task<IReadOnlyList<DepartmentRosterEntryDto>> GetRosterAsync(int departmentId, CancellationToken cancellationToken);
}
