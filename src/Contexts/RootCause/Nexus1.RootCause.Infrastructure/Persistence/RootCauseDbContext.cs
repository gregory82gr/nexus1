using Microsoft.EntityFrameworkCore;
using Nexus1.RootCause.Domain;
using Nexus1.RootCause.Infrastructure.Messaging;

namespace Nexus1.RootCause.Infrastructure.Persistence;

/// <summary>
/// RootCauseDb is its own physical database (unlike ReactorFleet/
/// AlarmManagement, ADR-006) — RootCause is the one service ADR-001
/// extracts to its own independently-deployed host in Phase 1.
/// </summary>
public sealed class RootCauseDbContext(DbContextOptions<RootCauseDbContext> options) : DbContext(options)
{
    public DbSet<RootCauseAnalysis> RootCauseAnalyses => Set<RootCauseAnalysis>();

    public DbSet<InboxReceipt> InboxReceipts => Set<InboxReceipt>();

    public DbSet<RetryTicket> RetryTickets => Set<RetryTicket>();

    public DbSet<PoisonMessage> PoisonMessages => Set<PoisonMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RootCauseDbContext).Assembly);
    }
}
