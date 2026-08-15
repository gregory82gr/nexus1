using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// IIdGenerator/IDateTimeProvider are context-agnostic — registered once per
/// host, not once per composed context, so every context sharing a host
/// shares one SequentialIdGenerator instance (harmless: each aggregate's id
/// is a distinct strongly-typed wrapper, so cross-context id-stream sharing
/// never collides).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildingBlocksApplication(this IServiceCollection services) => services
        .AddSingleton<IIdGenerator, SequentialIdGenerator>()
        .AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
}
