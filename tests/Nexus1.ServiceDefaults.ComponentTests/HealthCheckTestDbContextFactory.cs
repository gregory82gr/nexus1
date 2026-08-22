using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.ServiceDefaults.ComponentTests;

/// <summary>Design-time only, for `dotnet ef migrations add`.</summary>
public sealed class HealthCheckTestDbContextFactory : IDesignTimeDbContextFactory<HealthCheckTestDbContext>
{
    public HealthCheckTestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HealthCheckTestDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=HealthCheckTestDbContextFactoryDesignTime;Trusted_Connection=True;");

        return new HealthCheckTestDbContext(optionsBuilder.Options);
    }
}
