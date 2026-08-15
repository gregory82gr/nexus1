using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Application;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

internal sealed class EfAlarmDefinitionFinder(AlarmManagementDbContext dbContext) : IAlarmDefinitionFinder
{
    public async Task<IReadOnlyList<AlarmDefinition>> GetForUnitAsync(UnitId unitId, CancellationToken cancellationToken) =>
        await dbContext.AlarmDefinitions.Where(d => d.UnitId == unitId).ToListAsync(cancellationToken);
}
