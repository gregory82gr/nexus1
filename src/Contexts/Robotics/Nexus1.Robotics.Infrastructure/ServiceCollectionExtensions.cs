using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Robotics.Application;
using Nexus1.Robotics.Domain;
using Nexus1.Robotics.Infrastructure.Persistence;

namespace Nexus1.Robotics.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-023 — Robotics shares that physical database, own schema, own migration history, joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation/DigitalTwin/Maintenance/EventManagement).</summary>
    public static IServiceCollection AddRoboticsInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<RoboticsDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Robotics")));

        services.AddScoped<IRepository<RobotType, RobotTypeId>, EfRepository<RobotType, RobotTypeId>>();
        services.AddScoped<IRepository<RobotStatus, RobotStatusId>, EfRepository<RobotStatus, RobotStatusId>>();
        services.AddScoped<IRepository<BatteryStatus, BatteryStatusId>, EfRepository<BatteryStatus, BatteryStatusId>>();
        services.AddScoped<IRepository<CommunicationStatus, CommunicationStatusId>, EfRepository<CommunicationStatus, CommunicationStatusId>>();
        services.AddScoped<IRepository<MissionType, MissionTypeId>, EfRepository<MissionType, MissionTypeId>>();
        services.AddScoped<IRepository<MissionStatus, MissionStatusId>, EfRepository<MissionStatus, MissionStatusId>>();
        services.AddScoped<IRepository<MissionPriority, MissionPriorityId>, EfRepository<MissionPriority, MissionPriorityId>>();
        services.AddScoped<IRepository<ReadinessStatus, ReadinessStatusId>, EfRepository<ReadinessStatus, ReadinessStatusId>>();

        services.AddScoped<IRepository<RobotModel, RobotModelId>, EfRepository<RobotModel, RobotModelId>>();
        services.AddScoped<IRepository<Robot, RobotId>, EfRepository<Robot, RobotId>>();
        services.AddScoped<IRepository<RobotHealthSnapshot, RobotHealthSnapshotId>, EfRepository<RobotHealthSnapshot, RobotHealthSnapshotId>>();
        services.AddScoped<IRepository<Mission, MissionId>, EfRepository<Mission, MissionId>>();
        services.AddScoped<IRepository<MissionEvent, MissionEventId>, EfRepository<MissionEvent, MissionEventId>>();
        services.AddScoped<IRepository<MissionReadinessAssessment, MissionReadinessAssessmentId>, EfRepository<MissionReadinessAssessment, MissionReadinessAssessmentId>>();
        services.AddScoped<IRepository<MissionReadinessItem, MissionReadinessItemId>, EfRepository<MissionReadinessItem, MissionReadinessItemId>>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IAvailableRobotsFinder, EfAvailableRobotsFinder>();
        services.AddScoped<ILatestHealthSnapshotFinder, EfLatestHealthSnapshotFinder>();
        services.AddScoped<IMissionTimelineFinder, EfMissionTimelineFinder>();
        services.AddScoped<IBlockingReadinessFailuresFinder, EfBlockingReadinessFailuresFinder>();

        return services;
    }
}
