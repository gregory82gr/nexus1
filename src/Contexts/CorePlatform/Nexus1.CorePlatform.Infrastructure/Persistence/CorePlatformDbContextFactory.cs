using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.CorePlatform.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-015 (following ADR-006's precedent for ReactorFleet).
/// </summary>
public sealed class CorePlatformDbContextFactory : IDesignTimeDbContextFactory<CorePlatformDbContext>
{
    public CorePlatformDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CorePlatformDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_CorePlatform"));

        return new CorePlatformDbContext(optionsBuilder.Options);
    }
}
