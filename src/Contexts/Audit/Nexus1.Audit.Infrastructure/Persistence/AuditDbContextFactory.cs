using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Audit.Infrastructure.Persistence;

/// <summary>Design-time only, for `dotnet ef migrations add` — mirrors RootCauseDbContextFactory.</summary>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuditDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AuditDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Audit"));

        return new AuditDbContext(optionsBuilder.Options);
    }
}
