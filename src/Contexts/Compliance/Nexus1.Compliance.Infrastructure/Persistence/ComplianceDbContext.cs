using Microsoft.EntityFrameworkCore;
using Nexus1.Compliance.Domain;
using Nexus1.Compliance.Infrastructure.Messaging;

namespace Nexus1.Compliance.Infrastructure.Persistence;

/// <summary>
/// ComplianceDb is its own physical database, separate from RootCauseDb and
/// AuditDb — a data-ownership requirement (no FK to RootCauseDb/AuditDb,
/// ch.34 34-AN), not a deployment-topology one (ADR-011), same reasoning as
/// AuditDb (ADR-010). Unlike AuditDbContext, no append-only interceptor is
/// registered here — ComplianceReview is deliberately mutable (ADR-011).
/// </summary>
public sealed class ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : DbContext(options)
{
    public DbSet<ComplianceReview> Reviews => Set<ComplianceReview>();

    public DbSet<InboxReceipt> InboxReceipts => Set<InboxReceipt>();

    public DbSet<RetryTicket> RetryTickets => Set<RetryTicket>();

    public DbSet<PoisonMessage> PoisonMessages => Set<PoisonMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ComplianceDbContext).Assembly);
    }
}
