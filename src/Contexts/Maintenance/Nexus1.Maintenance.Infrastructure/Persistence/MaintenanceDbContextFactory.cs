using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Maintenance.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-021 (following ADR-006/ADR-015/ADR-019/ADR-020's precedent for
/// ReactorFleet/CorePlatform/Instrumentation/DigitalTwin).
/// </summary>
public sealed class MaintenanceDbContextFactory : IDesignTimeDbContextFactory<MaintenanceDbContext>
{
    public MaintenanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MaintenanceDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Maintenance"));

        return new MaintenanceDbContext(optionsBuilder.Options);
    }
}
