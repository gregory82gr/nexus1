using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

internal sealed class EfAlarmEventFinder(AlarmManagementDbContext dbContext) : IAlarmEventFinder
{
    public async Task<IReadOnlyList<DateTime>> GetRaisedAtUtcSinceAsync(
        UnitId unitId, DateTime sinceUtc, CancellationToken cancellationToken) =>
        await dbContext.AlarmEvents
            .Where(e => e.UnitId == unitId && e.RaisedAtUtc >= sinceUtc)
            .Select(e => e.RaisedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AlarmEvent>> GetActiveForUnitAsync(UnitId unitId, CancellationToken cancellationToken) =>
        await dbContext.AlarmEvents
            .Where(e => e.UnitId == unitId && e.State == AlarmState.Active)
            .ToListAsync(cancellationToken);
}
