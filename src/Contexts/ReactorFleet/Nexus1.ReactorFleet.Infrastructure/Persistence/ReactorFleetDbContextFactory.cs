using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.ReactorFleet.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Real runtime wiring
/// (connection string, DI registration) is Host-layer work, not built yet
/// (§5 step 6). Shares AlarmManagementDb per ADR-006.
/// </summary>
public sealed class ReactorFleetDbContextFactory : IDesignTimeDbContextFactory<ReactorFleetDbContext>
{
    public ReactorFleetDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReactorFleetDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_ReactorFleet"));

        return new ReactorFleetDbContext(optionsBuilder.Options);
    }
}
