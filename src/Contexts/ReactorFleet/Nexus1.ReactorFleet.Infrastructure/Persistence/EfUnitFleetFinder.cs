using Microsoft.EntityFrameworkCore;
using Nexus1.ReactorFleet.Application;
using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence;

/// <summary>Backs the BFF's fleet-overview and unit-detail screens (ADR-030).</summary>
internal sealed class EfUnitFleetFinder(ReactorFleetDbContext dbContext) : IUnitFleetFinder
{
    public async Task<IReadOnlyList<UnitSummaryDto>> GetAllSummariesAsync(CancellationToken cancellationToken) =>
        await dbContext.Units
            .OrderBy(u => u.Code)
            .Select(u => new UnitSummaryDto(
                u.Id.Value,
                u.Code,
                u.Name,
                dbContext.UnitPowerSnapshots
                    .Where(s => s.UnitId == u.Id)
                    .OrderByDescending(s => s.RecordedAtUtc)
                    .Select(s => (decimal?)s.PowerPercent.Value)
                    .FirstOrDefault(),
                dbContext.UnitPowerSnapshots
                    .Where(s => s.UnitId == u.Id)
                    .OrderByDescending(s => s.RecordedAtUtc)
                    .Select(s => (DateTime?)s.RecordedAtUtc)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

    public async Task<UnitDetailDto?> GetDetailByIdAsync(int unitId, CancellationToken cancellationToken)
    {
        var id = new UnitId(unitId);

        var unit = await dbContext.Units
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Code, u.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (unit is null)
        {
            return null;
        }

        var recentSnapshots = await dbContext.UnitPowerSnapshots
            .Where(s => s.UnitId == id)
            .OrderByDescending(s => s.RecordedAtUtc)
            .Take(10)
            .Select(s => new UnitPowerSnapshotDto(s.PowerPercent.Value, s.RecordedAtUtc))
            .ToListAsync(cancellationToken);

        var latest = recentSnapshots.Count > 0 ? recentSnapshots[0] : null;

        return new UnitDetailDto(
            unit.Id.Value,
            unit.Code,
            unit.Name,
            latest?.PowerPercent,
            latest?.RecordedAtUtc,
            recentSnapshots);
    }
}
