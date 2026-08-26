using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.EmergencyPreparedness.Application;
using Nexus1.EmergencyPreparedness.Domain;
using Nexus1.EmergencyPreparedness.Infrastructure.Persistence;

namespace Nexus1.EmergencyPreparedness.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-025 — EmergencyPreparedness shares that physical database, own schema, own migration history).</summary>
    public static IServiceCollection AddEmergencyPreparednessInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<EmergencyPreparednessDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_EmergencyPreparedness")));

        services.AddScoped<IRepository<PlanStatus, PlanStatusId>, EfRepository<PlanStatus, PlanStatusId>>();
        services.AddScoped<IRepository<RouteStatus, RouteStatusId>, EfRepository<RouteStatus, RouteStatusId>>();
        services.AddScoped<IRepository<ResourceType, ResourceTypeId>, EfRepository<ResourceType, ResourceTypeId>>();
        services.AddScoped<IRepository<ReadinessStatus, ReadinessStatusId>, EfRepository<ReadinessStatus, ReadinessStatusId>>();
        services.AddScoped<IRepository<ExerciseType, ExerciseTypeId>, EfRepository<ExerciseType, ExerciseTypeId>>();
        services.AddScoped<IRepository<ExerciseStatus, ExerciseStatusId>, EfRepository<ExerciseStatus, ExerciseStatusId>>();
        services.AddScoped<IRepository<ObservationSeverity, ObservationSeverityId>, EfRepository<ObservationSeverity, ObservationSeverityId>>();
        services.AddScoped<IRepository<ResourceStatus, ResourceStatusId>, EfRepository<ResourceStatus, ResourceStatusId>>();

        services.AddScoped<IRepository<EmergencyPlan, EmergencyPlanId>, EfRepository<EmergencyPlan, EmergencyPlanId>>();
        services.AddScoped<IRepository<EmergencyPlanRevision, EmergencyPlanRevisionId>, EfRepository<EmergencyPlanRevision, EmergencyPlanRevisionId>>();
        services.AddScoped<IRepository<Exercise, ExerciseId>, EfRepository<Exercise, ExerciseId>>();
        services.AddScoped<IRepository<ExerciseObservation, ExerciseObservationId>, EfRepository<ExerciseObservation, ExerciseObservationId>>();
        services.AddScoped<IRepository<AssemblyPoint, AssemblyPointId>, EfRepository<AssemblyPoint, AssemblyPointId>>();
        services.AddScoped<IRepository<EvacuationRoute, EvacuationRouteId>, EfRepository<EvacuationRoute, EvacuationRouteId>>();
        services.AddScoped<IRepository<EvacuationRouteZone, EvacuationRouteZoneId>, EfRepository<EvacuationRouteZone, EvacuationRouteZoneId>>();
        services.AddScoped<IRepository<EmergencyResource, EmergencyResourceId>, EfRepository<EmergencyResource, EmergencyResourceId>>();
        services.AddScoped<IRepository<ResourceReadinessCheck, ResourceReadinessCheckId>, EfRepository<ResourceReadinessCheck, ResourceReadinessCheckId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("EmergencyPreparedness");

        services.AddScoped<ISiteActivePlansFinder, EfSiteActivePlansFinder>();
        services.AddScoped<IExercisesWithCorrectiveObservationsFinder, EfExercisesWithCorrectiveObservationsFinder>();
        services.AddScoped<IOpenOrRestrictedRoutesFinder, EfOpenOrRestrictedRoutesFinder>();
        services.AddScoped<IResourceReadinessDashboardFinder, EfResourceReadinessDashboardFinder>();

        return services;
    }
}
