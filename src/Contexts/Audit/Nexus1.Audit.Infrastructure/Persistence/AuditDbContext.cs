using Microsoft.EntityFrameworkCore;
using Nexus1.Audit.Domain;
using Nexus1.Audit.Infrastructure.Messaging;

namespace Nexus1.Audit.Infrastructure.Persistence;

/// <summary>
/// AuditDb is its own physical database, separate from RootCauseDb and
/// AlarmManagementDb — a data-ownership requirement (no FK to RootCauseDb,
/// ch.34 34-AH), not a deployment-topology one (ADR-010). Audit's process is
/// still composed into Nexus1.ModularRuntime, unlike RootCause.
/// </summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditEvidenceRecord> Evidence => Set<AuditEvidenceRecord>();

    public DbSet<InboxReceipt> InboxReceipts => Set<InboxReceipt>();

    public DbSet<RetryTicket> RetryTickets => Set<RetryTicket>();

    public DbSet<PoisonMessage> PoisonMessages => Set<PoisonMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }
}
