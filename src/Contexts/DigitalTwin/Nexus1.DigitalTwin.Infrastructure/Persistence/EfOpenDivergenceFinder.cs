using Microsoft.EntityFrameworkCore;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence.ExternalReferences;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

/// <summary>Matches the atlas's own C.6.8 query 3 exactly: open/acknowledged divergences with the measured signal and model value, ordered by DetectedAtUtc desc.</summary>
internal sealed class EfOpenDivergenceFinder(DigitalTwinDbContext dbContext) : IOpenDivergenceFinder
{
    public async Task<IReadOnlyList<OpenDivergenceDto>> GetOpenDivergencesAsync(CancellationToken cancellationToken)
    {
        var query =
            from td in dbContext.TwinDivergences
            join s in dbContext.Set<InstrumentationSignalReference>() on td.SignalId equals s.SignalId
            join tvOuter in dbContext.TwinVariables on td.TwinVariableId equals (TwinVariableId?)tvOuter.Id into tvGroup
            from tv in tvGroup.DefaultIfEmpty()
            join ds in dbContext.DivergenceSeverities on td.DivergenceSeverityId equals ds.Id
            join dst in dbContext.DivergenceStatuses on td.DivergenceStatusId equals dst.Id
            where dst.Code == "OPEN" || dst.Code == "ACKNOWLEDGED"
            orderby td.DetectedAtUtc descending
            select new OpenDivergenceDto(
                td.DetectedAtUtc, s.Tag, tv != null ? tv.Code : null, td.ModeledValue, td.MeasuredValue, td.DeltaValue,
                ds.Code, dst.Code);

        return await query.ToListAsync(cancellationToken);
    }
}
