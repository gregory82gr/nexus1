using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Maintenance.Application;
using Nexus1.Maintenance.Domain;
using Nexus1.Maintenance.Infrastructure.Persistence;

namespace Nexus1.Maintenance.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-021 — Maintenance shares that physical database, own schema, own migration history, joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation/DigitalTwin).</summary>
    public static IServiceCollection AddMaintenanceInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MaintenanceDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Maintenance")));

        services.AddScoped<IRepository<AssetCategory, AssetCategoryId>, EfRepository<AssetCategory, AssetCategoryId>>();
        services.AddScoped<IRepository<AssetStatus, AssetStatusId>, EfRepository<AssetStatus, AssetStatusId>>();
        services.AddScoped<IRepository<AssetCriticality, AssetCriticalityId>, EfRepository<AssetCriticality, AssetCriticalityId>>();
        services.AddScoped<IRepository<ConditionGrade, ConditionGradeId>, EfRepository<ConditionGrade, ConditionGradeId>>();
        services.AddScoped<IRepository<DegradationMechanism, DegradationMechanismId>, EfRepository<DegradationMechanism, DegradationMechanismId>>();
        services.AddScoped<IRepository<FindingSeverity, FindingSeverityId>, EfRepository<FindingSeverity, FindingSeverityId>>();
        services.AddScoped<IRepository<WorkOrderType, WorkOrderTypeId>, EfRepository<WorkOrderType, WorkOrderTypeId>>();
        services.AddScoped<IRepository<WorkOrderStatus, WorkOrderStatusId>, EfRepository<WorkOrderStatus, WorkOrderStatusId>>();
        services.AddScoped<IRepository<WorkPriority, WorkPriorityId>, EfRepository<WorkPriority, WorkPriorityId>>();

        services.AddScoped<IRepository<Asset, AssetId>, EfRepository<Asset, AssetId>>();
        services.AddScoped<IRepository<AssetComponent, AssetComponentId>, EfRepository<AssetComponent, AssetComponentId>>();
        services.AddScoped<IRepository<AssetCondition, AssetConditionId>, EfRepository<AssetCondition, AssetConditionId>>();
        services.AddScoped<IRepository<AssetConditionMeasurement, AssetConditionMeasurementId>, EfRepository<AssetConditionMeasurement, AssetConditionMeasurementId>>();
        services.AddScoped<IRepository<DegradationRecord, DegradationRecordId>, EfRepository<DegradationRecord, DegradationRecordId>>();
        services.AddScoped<IRepository<DegradationTrendPoint, DegradationTrendPointId>, EfRepository<DegradationTrendPoint, DegradationTrendPointId>>();
        services.AddScoped<IRepository<WorkOrder, WorkOrderId>, EfRepository<WorkOrder, WorkOrderId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("Maintenance");

        services.AddScoped<IAssetsByUnitFinder, EfAssetsByUnitFinder>();
        services.AddScoped<IOpenWorkOrdersFinder, EfOpenWorkOrdersFinder>();
        services.AddScoped<IWorkOrdersWithOriginFinder, EfWorkOrdersWithOriginFinder>();
        services.AddScoped<ILatestConditionPerAssetFinder, EfLatestConditionPerAssetFinder>();
        services.AddScoped<IActiveDegradationCasesFinder, EfActiveDegradationCasesFinder>();

        return services;
    }
}
