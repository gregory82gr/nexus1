using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-023 (following ADR-006/ADR-015/ADR-019/ADR-020/ADR-021/ADR-022's
/// precedent for ReactorFleet/CorePlatform/Instrumentation/DigitalTwin/
/// Maintenance/EventManagement).
/// </summary>
public sealed class RoboticsDbContextFactory : IDesignTimeDbContextFactory<RoboticsDbContext>
{
    public RoboticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RoboticsDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Robotics"));

        return new RoboticsDbContext(optionsBuilder.Options);
    }
}
