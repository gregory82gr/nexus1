using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Instrumentation.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-019 (following ADR-006/ADR-015's precedent for ReactorFleet/CorePlatform).
/// </summary>
public sealed class InstrumentationDbContextFactory : IDesignTimeDbContextFactory<InstrumentationDbContext>
{
    public InstrumentationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InstrumentationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Instrumentation"));

        return new InstrumentationDbContext(optionsBuilder.Options);
    }
}
