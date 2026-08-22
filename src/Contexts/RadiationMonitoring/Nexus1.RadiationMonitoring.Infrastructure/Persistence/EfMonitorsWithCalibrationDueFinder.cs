using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RadiationMonitoring.Application;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>
/// Matches the atlas's own C.13.5.2 query 2, verbatim: RadiationMonitor
/// WHERE CalibrationDueAtUtc IS NOT NULL AND CalibrationDueAtUtc &lt;= now,
/// JOIN MonitorType. Uses the injected IDateTimeProvider (this codebase's
/// deterministic-clock discipline) rather than DateTime.UtcNow/
/// SYSUTCDATETIME() directly, so the "now" comparison is testable and
/// deterministic per test run (ADR-024).
/// </summary>
internal sealed class EfMonitorsWithCalibrationDueFinder(RadiationMonitoringDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : IMonitorsWithCalibrationDueFinder
{
    public async Task<IReadOnlyList<MonitorCalibrationDueDto>> GetMonitorsWithCalibrationDueAsync(CancellationToken cancellationToken)
    {
        var nowUtc = dateTimeProvider.UtcNow;

        var query =
            from m in dbContext.RadiationMonitors
            where m.CalibrationDueAtUtc != null && m.CalibrationDueAtUtc <= nowUtc
            join t in dbContext.MonitorTypes on m.MonitorTypeId equals t.Id
            select new MonitorCalibrationDueDto(m.Code, m.Name, t.Code, m.CalibrationDueAtUtc!.Value);

        return await query.ToListAsync(cancellationToken);
    }
}
