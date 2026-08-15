using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Real runtime wiring
/// (connection string, DI registration) is Host-layer work, not built yet
/// (§5 step 6).
/// </summary>
public sealed class AlarmManagementDbContextFactory : IDesignTimeDbContextFactory<AlarmManagementDbContext>
{
    public AlarmManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AlarmManagementDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_AlarmManagement"));

        return new AlarmManagementDbContext(optionsBuilder.Options);
    }
}
