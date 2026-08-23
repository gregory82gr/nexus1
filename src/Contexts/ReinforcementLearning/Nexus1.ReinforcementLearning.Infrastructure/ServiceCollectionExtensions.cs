using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.ReinforcementLearning.Application;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at AlarmManagementDb (ADR-026 — ReinforcementLearning shares that physical database, own schema, own migration history). Training/persistence only — no messaging registrations of any kind.</summary>
    public static IServiceCollection AddReinforcementLearningInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReinforcementLearningDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_ReinforcementLearning")));

        services.AddScoped<IRepository<EnvironmentModelType, EnvironmentModelTypeId>, EfRepository<EnvironmentModelType, EnvironmentModelTypeId>>();
        services.AddScoped<IRepository<StateSpaceType, StateSpaceTypeId>, EfRepository<StateSpaceType, StateSpaceTypeId>>();
        services.AddScoped<IRepository<ActionSpaceType, ActionSpaceTypeId>, EfRepository<ActionSpaceType, ActionSpaceTypeId>>();
        services.AddScoped<IRepository<RewardFunctionType, RewardFunctionTypeId>, EfRepository<RewardFunctionType, RewardFunctionTypeId>>();
        services.AddScoped<IRepository<LearningAlgorithm, LearningAlgorithmId>, EfRepository<LearningAlgorithm, LearningAlgorithmId>>();
        services.AddScoped<IRepository<TrainingRunStatus, TrainingRunStatusId>, EfRepository<TrainingRunStatus, TrainingRunStatusId>>();
        services.AddScoped<IRepository<PolicyStatus, PolicyStatusId>, EfRepository<PolicyStatus, PolicyStatusId>>();
        services.AddScoped<IRepository<AdvisoryMode, AdvisoryModeId>, EfRepository<AdvisoryMode, AdvisoryModeId>>();
        services.AddScoped<IRepository<RecommendationStatus, RecommendationStatusId>, EfRepository<RecommendationStatus, RecommendationStatusId>>();

        services.AddScoped<IRepository<EnvironmentModel, EnvironmentModelId>, EfRepository<EnvironmentModel, EnvironmentModelId>>();
        services.AddScoped<IRepository<StateSpace, StateSpaceId>, EfRepository<StateSpace, StateSpaceId>>();
        services.AddScoped<IRepository<StateDefinition, StateDefinitionId>, EfRepository<StateDefinition, StateDefinitionId>>();
        services.AddScoped<IRepository<ActionSpace, ActionSpaceId>, EfRepository<ActionSpace, ActionSpaceId>>();
        services.AddScoped<IRepository<ActionDefinition, ActionDefinitionId>, EfRepository<ActionDefinition, ActionDefinitionId>>();
        services.AddScoped<IRepository<RewardFunction, RewardFunctionId>, EfRepository<RewardFunction, RewardFunctionId>>();
        services.AddScoped<IRepository<HyperparameterSet, HyperparameterSetId>, EfRepository<HyperparameterSet, HyperparameterSetId>>();
        services.AddScoped<IRepository<Experiment, ExperimentId>, EfRepository<Experiment, ExperimentId>>();
        services.AddScoped<IRepository<TrainingRun, TrainingRunId>, EfRepository<TrainingRun, TrainingRunId>>();
        services.AddScoped<IRepository<QTable, QTableId>, EfRepository<QTable, QTableId>>();
        services.AddScoped<IRepository<QTableEntry, QTableEntryId>, EfRepository<QTableEntry, QTableEntryId>>();
        services.AddScoped<IRepository<Policy, PolicyId>, EfRepository<Policy, PolicyId>>();
        services.AddScoped<IRepository<PolicyEntry, PolicyEntryId>, EfRepository<PolicyEntry, PolicyEntryId>>();
        services.AddScoped<IRepository<PolicyDeployment, PolicyDeploymentId>, EfRepository<PolicyDeployment, PolicyDeploymentId>>();
        services.AddScoped<IRepository<AdvisorySession, AdvisorySessionId>, EfRepository<AdvisorySession, AdvisorySessionId>>();
        services.AddScoped<IRepository<AdvisoryRecommendation, AdvisoryRecommendationId>, EfRepository<AdvisoryRecommendation, AdvisoryRecommendationId>>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IPolicyEntryCountFinder, EfPolicyEntryCountFinder>();
        services.AddScoped<IFinalQTableEntryCountFinder, EfFinalQTableEntryCountFinder>();
        services.AddScoped<IPolicyGridFinder, EfPolicyGridFinder>();
        services.AddScoped<IClampedRecommendationsFinder, EfClampedRecommendationsFinder>();
        services.AddScoped<IActivePolicyFinder, EfActivePolicyFinder>();

        return services;
    }
}
