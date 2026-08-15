namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// ManagementPort is RabbitMQ's separate HTTP management API port (ch.25.5) —
/// used only for operational policy provisioning (ADR-009), never for AMQP
/// traffic, which stays on Port.
/// </summary>
public sealed record RabbitMqOptions(
    string HostName, int Port, string UserName, string Password, string VirtualHost, int ManagementPort = 15672)
{
    public static RabbitMqOptions LocalDefault => new("localhost", 5672, "guest", "guest", "/");
}
