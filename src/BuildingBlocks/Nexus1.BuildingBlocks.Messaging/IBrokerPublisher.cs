namespace Nexus1.BuildingBlocks.Messaging;

public interface IBrokerPublisher
{
    /// <summary>Throws if the broker doesn't confirm the publish within the timeout — callers (the outbox relay) must not mark a message dispatched unless this returns without throwing.</summary>
    public Task PublishAsync(OutboundMessage message, CancellationToken cancellationToken);
}
