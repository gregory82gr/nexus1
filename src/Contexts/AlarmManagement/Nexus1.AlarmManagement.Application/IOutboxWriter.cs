namespace Nexus1.AlarmManagement.Application;

/// <summary>
/// Transport-agnostic on purpose — envelope construction (canonicalization,
/// fingerprinting) is Infrastructure's job (ADR-008). Enqueue only stages
/// the outbox row; it does not commit — callers must still call
/// IUnitOfWork.SaveChangesAsync() so the outbox row and the aggregate's own
/// changes land in the same transaction.
/// </summary>
public interface IOutboxWriter
{
    void Enqueue(
        string eventType, int schemaVersion, string routingKey, DateTime occurredAtUtc,
        object payload, Guid? correlationId = null, Guid? causationId = null);
}
