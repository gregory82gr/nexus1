using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.BuildingBlocks.Observability;
using Nexus1.RootCause.Infrastructure.Persistence;

namespace Nexus1.RootCause.Infrastructure.Messaging;

/// <summary>
/// One EF query — count and oldest StoredAtUtc for undispatched rows — read
/// together so Pending/OldestOccurredUtc/ObservedUtc all describe the same
/// instant (ch.52 52-G's "SNAPSHOT CONSISTENCY"). Per-context, "duplication
/// until proven" like the retry/poison readers — AlarmManagement's own
/// outbox needs its own copy of this class, not a shared abstraction over
/// two different DbContext types.
/// </summary>
public sealed class RootCauseOutboxMetricSnapshotReader(IServiceScopeFactory scopeFactory, IDateTimeProvider dateTimeProvider)
    : IOutboxMetricSnapshotReader
{
    public async ValueTask<OutboxMetricSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RootCauseDbContext>();

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
