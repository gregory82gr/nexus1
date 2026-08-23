using Microsoft.Extensions.DependencyInjection;

namespace Nexus1.Instrumentation.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstrumentationApplication(this IServiceCollection services) => services
        .AddScoped<GetActiveHistorizedSignalsForUnitQueryHandler>()
        .AddScoped<GetLatestMeasurementsForTagQueryHandler>()
        .AddScoped<GetOpenSignalQualityEventsForUnitQueryHandler>()
        .AddScoped<GetAcquisitionPathForTagQueryHandler>()
        .AddScoped<GetUnitSignalReadingsQueryHandler>()
        .AddScoped<GetUnitSignalQualityEventsQueryHandler>()
        .AddScoped<RecordMeasurementCommandHandler>()
        .AddScoped<OpenSignalQualityEventCommandHandler>()
        .AddScoped<CloseSignalQualityEventCommandHandler>();
}
