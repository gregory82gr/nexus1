using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.BuildingBlocks.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNexusMessaging(this IServiceCollection services, RabbitMqOptions options) => services
        .AddSingleton(options)
        .AddSingleton<RabbitMqConnectionManager>()
        .AddSingleton<IBrokerPublisher, RabbitMqBrokerPublisher>();
}
