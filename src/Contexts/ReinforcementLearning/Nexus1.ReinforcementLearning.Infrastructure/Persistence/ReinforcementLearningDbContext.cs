using Microsoft.EntityFrameworkCore;
using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-026, following
/// ADR-006/ADR-015/.../ADR-025's precedent) but keeps its own migration
/// history and only owns ReinforcementLearning.* tables. The
/// ExternalReferences types (ReactorFleetUnitReference,
/// CorePlatformEngineeringUnitReference, DigitalTwinTwinModelReference) are
/// also part of this model — configured via IEntityTypeConfiguration and
/// picked up by ApplyConfigurationsFromAssembly below — but are excluded
/// from this context's migrations; they exist only so FK relationships to
/// tables owned by ReactorFleet/CorePlatform/DigitalTwin can be declared.
///
/// Training/persistence only per ADR-026 Option A — no messaging, no
/// inbox/outbox, no broker consumer of any kind.
/// </summary>
public sealed class ReinforcementLearningDbContext(DbContextOptions<ReinforcementLearningDbContext> options) : DbContext(options)
{
    public DbSet<EnvironmentModelType> EnvironmentModelTypes => Set<EnvironmentModelType>();

    public DbSet<StateSpaceType> StateSpaceTypes => Set<StateSpaceType>();

    public DbSet<ActionSpaceType> ActionSpaceTypes => Set<ActionSpaceType>();

    public DbSet<RewardFunctionType> RewardFunctionTypes => Set<RewardFunctionType>();

    public DbSet<LearningAlgorithm> LearningAlgorithms => Set<LearningAlgorithm>();

    public DbSet<TrainingRunStatus> TrainingRunStatuses => Set<TrainingRunStatus>();

    public DbSet<PolicyStatus> PolicyStatuses => Set<PolicyStatus>();

    public DbSet<AdvisoryMode> AdvisoryModes => Set<AdvisoryMode>();

    public DbSet<RecommendationStatus> RecommendationStatuses => Set<RecommendationStatus>();

    public DbSet<EnvironmentModel> EnvironmentModels => Set<EnvironmentModel>();

    public DbSet<StateSpace> StateSpaces => Set<StateSpace>();

    public DbSet<StateDefinition> StateDefinitions => Set<StateDefinition>();

    public DbSet<ActionSpace> ActionSpaces => Set<ActionSpace>();

    public DbSet<ActionDefinition> ActionDefinitions => Set<ActionDefinition>();

    public DbSet<RewardFunction> RewardFunctions => Set<RewardFunction>();

    public DbSet<HyperparameterSet> HyperparameterSets => Set<HyperparameterSet>();

    public DbSet<Experiment> Experiments => Set<Experiment>();

    public DbSet<TrainingRun> TrainingRuns => Set<TrainingRun>();

    public DbSet<QTable> QTables => Set<QTable>();

    public DbSet<QTableEntry> QTableEntries => Set<QTableEntry>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<PolicyEntry> PolicyEntries => Set<PolicyEntry>();

    public DbSet<PolicyDeployment> PolicyDeployments => Set<PolicyDeployment>();

    public DbSet<AdvisorySession> AdvisorySessions => Set<AdvisorySession>();

    public DbSet<AdvisoryRecommendation> AdvisoryRecommendations => Set<AdvisoryRecommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReinforcementLearningDbContext).Assembly);
    }
}
