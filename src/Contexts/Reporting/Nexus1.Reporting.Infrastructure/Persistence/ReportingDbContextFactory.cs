using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.Reporting.Infrastructure.Persistence;

/// <summary>Design-time only, for `dotnet ef migrations add` — mirrors AuditDbContextFactory/ComplianceDbContextFactory.</summary>
public sealed class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
    public ReportingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=ReportingDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Reporting"));

        return new ReportingDbContext(optionsBuilder.Options);
    }
}
