using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.RootCause.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Real runtime wiring
/// (connection string, DI registration) is Host-layer work, not built yet
/// (§5 step 6).
/// </summary>
public sealed class RootCauseDbContextFactory : IDesignTimeDbContextFactory<RootCauseDbContext>
{
    public RootCauseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RootCauseDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=RootCauseDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RootCause"));

        return new RootCauseDbContext(optionsBuilder.Options);
    }
}
