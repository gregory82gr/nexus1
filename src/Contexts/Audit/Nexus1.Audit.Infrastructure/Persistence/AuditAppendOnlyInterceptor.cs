using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nexus1.Audit.Domain;

namespace Nexus1.Audit.Infrastructure.Persistence;

/// <summary>
/// Executable Asset 34-AJ, adopted as-is (ADR-010): "Audit deliberately does
/// not... erase or overwrite history" (ch.34, 34-AF) enforced in code, not
/// just call-site discipline — the domain shape (no public mutators) is the
/// first line of defense; this interceptor is the second, catching any
/// future code path that manages to attach an EF-tracked change anyway.
/// </summary>
public sealed class AuditAppendOnlyInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Guard(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Guard(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Guard(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var forbidden = context.ChangeTracker.Entries<AuditEvidenceRecord>()
            .Where(x => x.State is EntityState.Modified or EntityState.Deleted)
            .Select(x => x.Entity.Id)
            .ToArray();

        if (forbidden.Length != 0)
        {
            throw new AuditMutationRejectedException(forbidden);
        }
    }
}
