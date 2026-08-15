using Microsoft.EntityFrameworkCore;
using Nexus1.Reporting.Domain;
using Nexus1.Reporting.Infrastructure.Messaging;
using Nexus1.Reporting.Infrastructure.Projection;

namespace Nexus1.Reporting.Infrastructure.Persistence;

/// <summary>
/// ReportingDb is its own physical database, separate from RootCauseDb —
/// a data-ownership requirement, not a deployment-topology one (ADR-012),
/// same reasoning as AuditDb/ComplianceDb (ADR-010/ADR-011). Reporting's
/// process is still composed into Nexus1.ModularRuntime.
/// </summary>
public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<RootCauseCaseSummary> CaseSummaries => Set<RootCauseCaseSummary>();

    public DbSet<PendingVerdict> PendingVerdicts => Set<PendingVerdict>();

    public DbSet<InboxReceipt> InboxReceipts => Set<InboxReceipt>();

    public DbSet<RetryTicket> RetryTickets => Set<RetryTicket>();

    public DbSet<PoisonMessage> PoisonMessages => Set<PoisonMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
    }
}
