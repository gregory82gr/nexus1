using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Application;

public interface IAlarmEventFinder
{
    Task<IReadOnlyList<DateTime>> GetRaisedAtUtcSinceAsync(UnitId unitId, DateTime sinceUtc, CancellationToken cancellationToken);

    Task<IReadOnlyList<AlarmEvent>> GetActiveForUnitAsync(UnitId unitId, CancellationToken cancellationToken);

    /// <summary>Fleet-wide, unlike GetActiveForUnitAsync — added for the BFF's alarm-monitoring screen (ADR-030).</summary>
    Task<IReadOnlyList<AlarmEvent>> GetAllActiveAsync(CancellationToken cancellationToken);
}
