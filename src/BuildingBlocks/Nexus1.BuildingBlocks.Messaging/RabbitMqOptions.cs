namespace Nexus1.BuildingBlocks.Messaging;

public sealed record RabbitMqOptions(string HostName, int Port, string UserName, string Password, string VirtualHost)
{
    public static RabbitMqOptions LocalDefault => new("localhost", 5672, "guest", "guest", "/");
}
