using Microsoft.EntityFrameworkCore;
using Nexus1.EventManagement.Application;
using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

internal sealed class EfIncidentExistenceFinder(EventManagementDbContext dbContext) : IIncidentExistenceFinder
{
    public Task<bool> ExistsForOperationalEventAsync(long operationalEventId, CancellationToken cancellationToken)
    {
        var id = new OperationalEventId(operationalEventId);
        return dbContext.Incidents.AnyAsync(i => i.OperationalEventId == id, cancellationToken);
    }
}
