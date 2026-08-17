using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.EventManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-022 (following ADR-006/ADR-015/ADR-019/ADR-020/ADR-021's precedent
/// for ReactorFleet/CorePlatform/Instrumentation/DigitalTwin/Maintenance).
/// </summary>
public sealed class EventManagementDbContextFactory : IDesignTimeDbContextFactory<EventManagementDbContext>
{
    public EventManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventManagementDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_EventManagement"));

        return new EventManagementDbContext(optionsBuilder.Options);
    }
}
