using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.ReactorFleet.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReactorFleetApplication(this IServiceCollection services) =>
        services.AddScoped<RecordUnitPowerSnapshotCommandHandler>();
}
