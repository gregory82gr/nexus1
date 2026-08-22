using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.AlarmManagement.Infrastructure.Persistence;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.AlarmManagement.Infrastructure.Messaging;

/// <summary>
/// Mirrors Nexus1.RootCause.Infrastructure.Messaging.RootCauseOutboxMetricSnapshotReader
/// exactly (ADR-014) — one EF query, count and oldest StoredAtUtc for
/// undispatched rows read together so Pending/OldestOccurredUtc/ObservedUtc
/// all describe the same instant. Per-context, "duplication until proven"
/// like RootCause's own copy — a different DbContext type, not a shared
/// abstraction.
/// </summary>
public sealed class AlarmManagementOutboxMetricSnapshotReader(IServiceScopeFactory scopeFactory, IDateTimeProvider dateTimeProvider)
    : IOutboxMetricSnapshotReader
{
    public async ValueTask<OutboxMetricSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AlarmManagementDbContext>();

        var pending = dbContext.OutboxMessages.Where(m => m.ProcessedAtUtc == null);

        var count = await pending.CountAsync(cancellationToken);
        DateTime? oldestStoredAtUtc = count == 0
            ? null
            : await pending.MinAsync(m => m.StoredAtUtc, cancellationToken);

        var observedUtc = new DateTimeOffset(dateTimeProvider.UtcNow, TimeSpan.Zero);
        var oldestOccurredUtc = oldestStoredAtUtc is null
            ? (DateTimeOffset?)null
            : new DateTimeOffset(oldestStoredAtUtc.Value, TimeSpan.Zero);

        return new OutboxMetricSnapshot(count, oldestOccurredUtc, observedUtc);
    }
}
