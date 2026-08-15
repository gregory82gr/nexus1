using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Compliance.Infrastructure.Persistence;

/// <summary>Design-time only, for `dotnet ef migrations add` — mirrors AuditDbContextFactory.</summary>
public sealed class ComplianceDbContextFactory : IDesignTimeDbContextFactory<ComplianceDbContext>
{
    public ComplianceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ComplianceDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=ComplianceDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Compliance"));

        return new ComplianceDbContext(optionsBuilder.Options);
    }
}
