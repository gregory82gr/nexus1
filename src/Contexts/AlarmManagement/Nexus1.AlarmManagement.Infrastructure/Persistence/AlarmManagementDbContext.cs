using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Domain;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

public sealed class AlarmManagementDbContext(DbContextOptions<AlarmManagementDbContext> options) : DbContext(options)
{
    public DbSet<AlarmDefinition> AlarmDefinitions => Set<AlarmDefinition>();

    public DbSet<AlarmEvent> AlarmEvents => Set<AlarmEvent>();

    public DbSet<AlarmFlood> AlarmFloods => Set<AlarmFlood>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlarmManagementDbContext).Assembly);
    }
}
