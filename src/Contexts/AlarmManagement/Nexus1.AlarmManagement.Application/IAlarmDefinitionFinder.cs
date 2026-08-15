using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Application;

/// <summary>
/// Query-side port, separate from IRepository&lt;TRoot,TId&gt; — the generic
/// repository only supports get-by-id/add (Blueprint_to_Core's shape,
/// ADR-002-amend); "all definitions for a unit" is a read need specific to
/// this context, not part of the shared kernel's minimal repository port.
/// </summary>
public interface IAlarmDefinitionFinder
{
    Task<IReadOnlyList<AlarmDefinition>> GetForUnitAsync(UnitId unitId, CancellationToken cancellationToken);
}
