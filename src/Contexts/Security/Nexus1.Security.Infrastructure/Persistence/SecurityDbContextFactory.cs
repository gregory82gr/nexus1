using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Security.Infrastructure.Persistence;

/// <summary>Design-time only, for `dotnet ef migrations add`. Own physical database (SecurityDb, ADR-016) — not shared with any other context.</summary>
public sealed class SecurityDbContextFactory : IDesignTimeDbContextFactory<SecurityDbContext>
{
    public SecurityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SecurityDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=SecurityDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Security"));

        return new SecurityDbContext(optionsBuilder.Options);
    }
}
