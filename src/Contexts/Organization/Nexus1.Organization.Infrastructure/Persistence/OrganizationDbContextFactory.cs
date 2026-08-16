using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Organization.Infrastructure.Persistence;

/// <summary>Design-time only, for `dotnet ef migrations add`. Own physical database (OrganizationDb, ADR-017) — not shared with any other context.</summary>
public sealed class OrganizationDbContextFactory : IDesignTimeDbContextFactory<OrganizationDbContext>
{
    public OrganizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Organization"));

        return new OrganizationDbContext(optionsBuilder.Options);
    }
}
