using Microsoft.EntityFrameworkCore;
using Nexus1.AlarmManagement.Domain;
using Nexus1.AlarmManagement.Infrastructure.Messaging;

namespace Nexus1.AlarmManagement.Infrastructure.Persistence;

public sealed class AlarmManagementDbContext(DbContextOptions<AlarmManagementDbContext> options) : DbContext(options)
{
    public DbSet<AlarmDefinition> AlarmDefinitions => Set<AlarmDefinition>();

    public DbSet<AlarmEvent> AlarmEvents => Set<AlarmEvent>();

    public DbSet<AlarmFlood> AlarmFloods => Set<AlarmFlood>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlarmManagementDbContext).Assembly);
    }
}
