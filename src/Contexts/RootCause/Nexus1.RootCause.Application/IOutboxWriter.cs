namespace Nexus1.RootCause.Application;

/// <summary>
/// Mirrors Nexus1.AlarmManagement.Application.IOutboxWriter exactly
/// (ADR-008/ADR-010) — transport-agnostic, stages only, caller still calls
/// IUnitOfWork.SaveChangesAsync() so the outbox row and the aggregate's own
/// changes land in the same transaction.
/// </summary>
public interface IOutboxWriter
{
    void Enqueue(
        string eventType, int schemaVersion, string routingKey, DateTime occurredAtUtc,
        object payload, Guid? correlationId = null, Guid? causationId = null);
}
