using Microsoft.EntityFrameworkCore;
using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-006) but keeps its own
/// migration history and only knows about ReactorFleet.* tables.
/// </summary>
public sealed class ReactorFleetDbContext(DbContextOptions<ReactorFleetDbContext> options) : DbContext(options)
{
    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<UnitPowerSnapshot> UnitPowerSnapshots => Set<UnitPowerSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReactorFleetDbContext).Assembly);
    }
}
