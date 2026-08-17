using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-024 (following ADR-006/ADR-015/ADR-019/ADR-020/ADR-021/ADR-022/
/// ADR-023's precedent for ReactorFleet/CorePlatform/Instrumentation/
/// DigitalTwin/Maintenance/EventManagement/Robotics).
/// </summary>
public sealed class RadiationMonitoringDbContextFactory : IDesignTimeDbContextFactory<RadiationMonitoringDbContext>
{
    public RadiationMonitoringDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RadiationMonitoringDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RadiationMonitoring"));

        return new RadiationMonitoringDbContext(optionsBuilder.Options);
    }
}
