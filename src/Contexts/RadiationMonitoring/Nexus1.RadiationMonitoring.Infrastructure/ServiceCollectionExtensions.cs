using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.RadiationMonitoring.Application;
using Nexus1.RadiationMonitoring.Domain;
using Nexus1.RadiationMonitoring.Infrastructure.Persistence;

namespace Nexus1.RadiationMonitoring.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-024 — RadiationMonitoring shares that physical database, own schema, own migration history, joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation/DigitalTwin/Maintenance/EventManagement/Robotics).</summary>
    public static IServiceCollection AddRadiationMonitoringInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RadiationMonitoringDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_RadiationMonitoring")));

        services.AddScoped<IRepository<RadiationZoneType, RadiationZoneTypeId>, EfRepository<RadiationZoneType, RadiationZoneTypeId>>();
        services.AddScoped<IRepository<RadiationZoneStatus, RadiationZoneStatusId>, EfRepository<RadiationZoneStatus, RadiationZoneStatusId>>();
        services.AddScoped<IRepository<RadiationAreaClassification, RadiationAreaClassificationId>, EfRepository<RadiationAreaClassification, RadiationAreaClassificationId>>();
        services.AddScoped<IRepository<MonitorType, MonitorTypeId>, EfRepository<MonitorType, MonitorTypeId>>();
        services.AddScoped<IRepository<MonitorStatus, MonitorStatusId>, EfRepository<MonitorStatus, MonitorStatusId>>();
        services.AddScoped<IRepository<MeasurementType, MeasurementTypeId>, EfRepository<MeasurementType, MeasurementTypeId>>();
        services.AddScoped<IRepository<MeasurementQuality, MeasurementQualityId>, EfRepository<MeasurementQuality, MeasurementQualityId>>();
        services.AddScoped<IRepository<DoseType, DoseTypeId>, EfRepository<DoseType, DoseTypeId>>();
        services.AddScoped<IRepository<DosimeterType, DosimeterTypeId>, EfRepository<DosimeterType, DosimeterTypeId>>();
        services.AddScoped<IRepository<DosimeterStatus, DosimeterStatusId>, EfRepository<DosimeterStatus, DosimeterStatusId>>();
        services.AddScoped<IRepository<LimitType, LimitTypeId>, EfRepository<LimitType, LimitTypeId>>();
        services.AddScoped<IRepository<AlertStatus, AlertStatusId>, EfRepository<AlertStatus, AlertStatusId>>();

        services.AddScoped<IRepository<RadiationZone, RadiationZoneId>, EfRepository<RadiationZone, RadiationZoneId>>();
        services.AddScoped<IRepository<RadiationMonitor, RadiationMonitorId>, EfRepository<RadiationMonitor, RadiationMonitorId>>();
        services.AddScoped<IRepository<RadiationReading, RadiationReadingId>, EfRepository<RadiationReading, RadiationReadingId>>();
        services.AddScoped<IRepository<Dosimeter, DosimeterId>, EfRepository<Dosimeter, DosimeterId>>();
        services.AddScoped<IRepository<PersonDosimeterAssignment, PersonDosimeterAssignmentId>, EfRepository<PersonDosimeterAssignment, PersonDosimeterAssignmentId>>();
        services.AddScoped<IRepository<PersonDoseReading, PersonDoseReadingId>, EfRepository<PersonDoseReading, PersonDoseReadingId>>();
        services.AddScoped<IRepository<DoseLimit, DoseLimitId>, EfRepository<DoseLimit, DoseLimitId>>();
        services.AddScoped<IRepository<DoseAlert, DoseAlertId>, EfRepository<DoseAlert, DoseAlertId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("RadiationMonitoring");

        services.AddScoped<IActiveRadiationZonesFinder, EfActiveRadiationZonesFinder>();
        services.AddScoped<IMonitorsWithCalibrationDueFinder, EfMonitorsWithCalibrationDueFinder>();
        services.AddScoped<ILatestReadingPerMonitorFinder, EfLatestReadingPerMonitorFinder>();
        services.AddScoped<IOpenDoseAlertsFinder, EfOpenDoseAlertsFinder>();

        return services;
    }
}
