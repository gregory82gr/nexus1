using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.DigitalTwin.Application;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;

namespace Nexus1.DigitalTwin.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-020 — DigitalTwin shares that physical database, own schema, own migration history, joining ReactorFleet/CorePlatform/AlarmManagement/Instrumentation).</summary>
    public static IServiceCollection AddDigitalTwinInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DigitalTwinDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_DigitalTwin")));

        services.AddScoped<IRepository<TwinModelType, TwinModelTypeId>, EfRepository<TwinModelType, TwinModelTypeId>>();
        services.AddScoped<IRepository<TwinModelStatus, TwinModelStatusId>, EfRepository<TwinModelStatus, TwinModelStatusId>>();
        services.AddScoped<IRepository<TwinFidelityLevel, TwinFidelityLevelId>, EfRepository<TwinFidelityLevel, TwinFidelityLevelId>>();
        services.AddScoped<IRepository<ModelVariableType, ModelVariableTypeId>, EfRepository<ModelVariableType, ModelVariableTypeId>>();
        services.AddScoped<IRepository<SolverType, SolverTypeId>, EfRepository<SolverType, SolverTypeId>>();
        services.AddScoped<IRepository<ValidationStatus, ValidationStatusId>, EfRepository<ValidationStatus, ValidationStatusId>>();
        services.AddScoped<IRepository<BindingRole, BindingRoleId>, EfRepository<BindingRole, BindingRoleId>>();
        services.AddScoped<IRepository<BindingStatus, BindingStatusId>, EfRepository<BindingStatus, BindingStatusId>>();
        services.AddScoped<IRepository<SnapshotReason, SnapshotReasonId>, EfRepository<SnapshotReason, SnapshotReasonId>>();
        services.AddScoped<IRepository<DivergenceSeverity, DivergenceSeverityId>, EfRepository<DivergenceSeverity, DivergenceSeverityId>>();
        services.AddScoped<IRepository<DivergenceStatus, DivergenceStatusId>, EfRepository<DivergenceStatus, DivergenceStatusId>>();

        services.AddScoped<IRepository<TwinModel, TwinModelId>, EfRepository<TwinModel, TwinModelId>>();
        services.AddScoped<IRepository<TwinModelVersion, TwinModelVersionId>, EfRepository<TwinModelVersion, TwinModelVersionId>>();
        services.AddScoped<IRepository<TwinVariable, TwinVariableId>, EfRepository<TwinVariable, TwinVariableId>>();
        services.AddScoped<IRepository<SignalBinding, SignalBindingId>, EfRepository<SignalBinding, SignalBindingId>>();
        services.AddScoped<IRepository<TwinRuntimeSession, TwinRuntimeSessionId>, EfRepository<TwinRuntimeSession, TwinRuntimeSessionId>>();
        services.AddScoped<IRepository<TwinSnapshot, TwinSnapshotId>, EfRepository<TwinSnapshot, TwinSnapshotId>>();
        services.AddScoped<IRepository<TwinSnapshotValue, TwinSnapshotValueId>, EfRepository<TwinSnapshotValue, TwinSnapshotValueId>>();
        services.AddScoped<IRepository<TwinDivergence, TwinDivergenceId>, EfRepository<TwinDivergence, TwinDivergenceId>>();
        services.AddScoped<IRepository<TwinDivergenceReview, TwinDivergenceReviewId>, EfRepository<TwinDivergenceReview, TwinDivergenceReviewId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("DigitalTwin");

        services.AddScoped<IActiveTwinFinder, EfActiveTwinFinder>();
        services.AddScoped<IModelVariableSignalTraceFinder, EfModelVariableSignalTraceFinder>();
        services.AddScoped<IOpenDivergenceFinder, EfOpenDivergenceFinder>();

        return services;
    }
}
