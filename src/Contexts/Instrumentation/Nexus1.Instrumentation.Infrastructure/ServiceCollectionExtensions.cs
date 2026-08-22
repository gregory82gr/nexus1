using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Instrumentation.Application;
using Nexus1.Instrumentation.Domain;
using Nexus1.Instrumentation.Infrastructure.Persistence;

namespace Nexus1.Instrumentation.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-019 — Instrumentation shares that physical database, own schema, own migration history).</summary>
    public static IServiceCollection AddInstrumentationInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<InstrumentationDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Instrumentation")));

        services.AddScoped<IRepository<SignalType, SignalTypeId>, EfRepository<SignalType, SignalTypeId>>();
        services.AddScoped<IRepository<SignalCategory, SignalCategoryId>, EfRepository<SignalCategory, SignalCategoryId>>();
        services.AddScoped<IRepository<SignalRole, SignalRoleId>, EfRepository<SignalRole, SignalRoleId>>();
        services.AddScoped<IRepository<SamplingMode, SamplingModeId>, EfRepository<SamplingMode, SamplingModeId>>();
        services.AddScoped<IRepository<HistorianRetentionClass, HistorianRetentionClassId>, EfRepository<HistorianRetentionClass, HistorianRetentionClassId>>();
        services.AddScoped<IRepository<SignalQuality, SignalQualityId>, EfRepository<SignalQuality, SignalQualityId>>();
        services.AddScoped<IRepository<MeasurementSource, MeasurementSourceId>, EfRepository<MeasurementSource, MeasurementSourceId>>();
        services.AddScoped<IRepository<ChannelStatus, ChannelStatusId>, EfRepository<ChannelStatus, ChannelStatusId>>();
        services.AddScoped<IRepository<DataAcquisitionNode, DataAcquisitionNodeId>, EfRepository<DataAcquisitionNode, DataAcquisitionNodeId>>();
        services.AddScoped<IRepository<AcquisitionConnection, AcquisitionConnectionId>, EfRepository<AcquisitionConnection, AcquisitionConnectionId>>();
        services.AddScoped<IRepository<AcquisitionPoint, AcquisitionPointId>, EfRepository<AcquisitionPoint, AcquisitionPointId>>();
        services.AddScoped<IRepository<Signal, SignalId>, EfRepository<Signal, SignalId>>();
        services.AddScoped<IRepository<SignalMapping, SignalMappingId>, EfRepository<SignalMapping, SignalMappingId>>();
        services.AddScoped<IRepository<Measurement, MeasurementId>, EfRepository<Measurement, MeasurementId>>();
        services.AddScoped<IRepository<SignalQualityEvent, SignalQualityEventId>, EfRepository<SignalQualityEvent, SignalQualityEventId>>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IActiveHistorizedSignalFinder, EfActiveHistorizedSignalFinder>();
        services.AddScoped<ILatestMeasurementFinder, EfLatestMeasurementFinder>();
        services.AddScoped<IOpenSignalQualityEventFinder, EfOpenSignalQualityEventFinder>();
        services.AddScoped<IAcquisitionPathFinder, EfAcquisitionPathFinder>();

        return services;
    }
}
