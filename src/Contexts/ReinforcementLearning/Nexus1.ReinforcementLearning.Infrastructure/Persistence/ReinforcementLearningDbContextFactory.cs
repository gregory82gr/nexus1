using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>
/// Design-time only, for `dotnet ef migrations add`. Shares AlarmManagementDb
/// per ADR-026 (following ADR-006/.../ADR-025's precedent for the other ten
/// sectors sharing it).
/// </summary>
public sealed class ReinforcementLearningDbContextFactory : IDesignTimeDbContextFactory<ReinforcementLearningDbContext>
{
    public ReinforcementLearningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReinforcementLearningDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AlarmManagementDb;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_ReinforcementLearning"));

        return new ReinforcementLearningDbContext(optionsBuilder.Options);
    }
}
