using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-025 (following ADR-006/.../ADR-024's precedent for the other nine
/// sectors sharing it).
/// </summary>
public sealed class EmergencyPreparednessDbContextFactory : IDesignTimeDbContextFactory<EmergencyPreparednessDbContext>
{
    public EmergencyPreparednessDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmergencyPreparednessDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_EmergencyPreparedness"));

        return new EmergencyPreparednessDbContext(optionsBuilder.Options);
    }
}
