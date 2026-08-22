using Microsoft.EntityFrameworkCore;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.6.8 query 2 exactly: for a twin code, SignalBinding -> TwinModel/TwinVariable/Instrumentation.Signal/BindingRole/BindingStatus.</summary>
internal sealed class EfModelVariableSignalTraceFinder(DigitalTwinDbContext dbContext) : IModelVariableSignalTraceFinder
{
    public async Task<IReadOnlyList<ModelVariableSignalTraceDto>> GetByTwinCodeAsync(string twinCode, CancellationToken cancellationToken) =>
        await dbContext.SignalBindings
            .Join(dbContext.TwinModels.Where(tm => tm.Code == twinCode), sb => sb.TwinModelId, tm => tm.Id, (sb, tm) => new { sb, tm })
            .Join(dbContext.TwinVariables, x => x.sb.TwinVariableId, tv => tv.Id, (x, tv) => new { x.sb, x.tm, tv })
            .Join(dbContext.Set<InstrumentationSignalReference>(), x => x.sb.SignalId, s => s.SignalId, (x, s) => new { x.sb, x.tm, x.tv, s })
            .Join(dbContext.BindingRoles, x => x.sb.BindingRoleId, br => br.Id, (x, br) => new { x.sb, x.tm, x.tv, x.s, br })
            .Join(dbContext.BindingStatuses, x => x.sb.BindingStatusId, bs => bs.Id, (x, bs) => new { x.tm, x.tv, x.s, x.br, bs })
            .Select(x => new ModelVariableSignalTraceDto(x.tm.Code, x.tv.Code, x.s.Tag, x.br.Code, x.bs.Code))
            .ToListAsync(cancellationToken);
}
