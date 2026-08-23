using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.DigitalTwin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDigitalTwinApplication(this IServiceCollection services) => services
        .AddScoped<GetActiveTwinsForFleetQueryHandler>()
        .AddScoped<GetUnitTwinStateQueryHandler>()
        .AddScoped<TraceModelVariableToSignalQueryHandler>()
        .AddScoped<GetOpenDivergencesQueryHandler>()
        .AddScoped<CaptureTwinSnapshotCommandHandler>()
        .AddScoped<RecordTwinDivergenceCommandHandler>()
        .AddScoped<ReviewTwinDivergenceCommandHandler>();
}
