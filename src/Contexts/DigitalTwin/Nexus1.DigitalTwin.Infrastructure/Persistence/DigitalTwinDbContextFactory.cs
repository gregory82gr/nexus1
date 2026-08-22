using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.DigitalTwin.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-020 (following ADR-006/ADR-015/ADR-019's precedent for
/// ReactorFleet/CorePlatform/Instrumentation).
/// </summary>
public sealed class DigitalTwinDbContextFactory : IDesignTimeDbContextFactory<DigitalTwinDbContext>
{
    public DigitalTwinDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DigitalTwinDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_DigitalTwin"));

        return new DigitalTwinDbContext(optionsBuilder.Options);
    }
}
