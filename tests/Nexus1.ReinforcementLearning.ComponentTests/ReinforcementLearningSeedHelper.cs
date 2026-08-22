using Nexus1.CorePlatform.Domain;
using Nexus1.CorePlatform.Infrastructure.Persistence;
using Nexus1.DigitalTwin.Domain;
using Nexus1.DigitalTwin.Infrastructure.Persistence;
using Nexus1.ReactorFleet.Domain;
using Nexus1.ReactorFleet.Infrastructure.Persistence;
using Nexus1.ReinforcementLearning.Domain;
using Nexus1.ReinforcementLearning.Infrastructure.Persistence;

namespace Nexus1.ReinforcementLearning.ComponentTests;

/// <summary>
/// Shared seed: one ReactorFleet.Unit row, one CorePlatform.EngineeringUnit
/// row, the three DigitalTwin lookup rows a TwinModel needs plus one
/// TwinModel row (the sector's three real cross-context FK targets,
/// ADR-026), the nine ReinforcementLearning lookup rows, and one
/// EnvironmentModel/StateSpace(+2 StateDefinitions)/ActionSpace(+2
/// ActionDefinitions)/RewardFunction/HyperparameterSet/Experiment/
/// TrainingRun/QTable(final, +4 QTableEntries)/Policy(+2 PolicyEntries)/
/// PolicyDeployment/AdvisorySession/AdvisoryRecommendation graph. Every
/// component test below builds on this same shape so the seven Application
/// operations are exercised against a realistic, FK-consistent graph rather
/// than isolated rows — copying EmergencyPreparednessSeedHelper's own
/// precedent pattern exactly, with two state/action rows (rather than one)
/// so the policy-grid and Q-table-entry-count queries have more than a
/// single trivial row to report on.
/// </summary>
internal static class ReinforcementLearningSeedHelper
{
    public const string UnitCode = "UNIT-RL-001";
    public const string EngineeringUnitSymbol = "pct";
    public const string TwinModelCode = "TWIN-RL-001";
    public const string EnvironmentModelCode = "ENV-001";
    public const string StateSpaceCode = "SS-001";
    public const string ActionSpaceCode = "AS-001";
    public const string RewardFunctionCode = "RF-001";
    public const string HyperparameterSetCode = "HP-001";
    public const string ExperimentCode = "EXP-001";
    public const string TrainingRunCode = "TR-001";
    public const string QTableCode = "QT-001";
    public const string PolicyCode = "POL-001";

    public sealed record SeedResult(
        int UnitId, int EngineeringUnitId, int TwinModelId, int EnvironmentModelTypeId, int StateSpaceTypeId,
        int ActionSpaceTypeId, int RewardFunctionTypeId, int LearningAlgorithmId, int TrainingRunStatusId,
        int PolicyStatusId, int AdvisoryModeId, int RecommendationStatusId, int EnvironmentModelId,
        int StateSpaceId, int StateDefinitionId1, int StateDefinitionId2, int ActionSpaceId, int ActionDefinitionId1,
        int ActionDefinitionId2, int RewardFunctionId, int HyperparameterSetId, int ExperimentId, int TrainingRunId,
        int QTableId, int PolicyId, int PolicyDeploymentId, long AdvisorySessionId, long AdvisoryRecommendationId);

    public static async Task<SeedResult> SeedCoreAsync(
        ReactorFleetDbContext reactorFleetContext, CorePlatformDbContext corePlatformContext,
        DigitalTwinDbContext digitalTwinContext, ReinforcementLearningDbContext rlContext, DateTime nowUtc)
    {
        var unit = Unit.Create(new UnitId(1), UnitCode, "Unit One");
        await reactorFleetContext.Units.AddAsync(unit);
        await reactorFleetContext.SaveChangesAsync();

        var engineeringUnit = EngineeringUnit.Create(
            new EngineeringUnitId(1), EngineeringUnitSymbol, "Percent Power", EngineeringQuantityType.RadiationDoseRate, nowUtc);
        await corePlatformContext.EngineeringUnits.AddAsync(engineeringUnit);
        await corePlatformContext.SaveChangesAsync();

        var twinModelType = TwinModelType.Create(new TwinModelTypeId(1), "POINT_KINETICS", "Point Kinetics Model", nowUtc);
        var twinModelStatus = TwinModelStatus.Create(new TwinModelStatusId(1), "ACTIVE", "Active", nowUtc);
        var twinFidelityLevel = TwinFidelityLevel.Create(new TwinFidelityLevelId(1), "TRAINING", "Training", nowUtc);
        await digitalTwinContext.TwinModelTypes.AddAsync(twinModelType);
        await digitalTwinContext.TwinModelStatuses.AddAsync(twinModelStatus);
        await digitalTwinContext.TwinFidelityLevels.AddAsync(twinFidelityLevel);
        await digitalTwinContext.SaveChangesAsync();

        var twinModel = TwinModel.Create(
            new TwinModelId(1), unit.Id.Value, twinModelType.Id, twinModelStatus.Id, twinFidelityLevel.Id,
            TwinModelCode, "RL Training Surrogate", nowUtc);
        await digitalTwinContext.TwinModels.AddAsync(twinModel);
        await digitalTwinContext.SaveChangesAsync();

        var environmentModelType = EnvironmentModelType.Create(new EnvironmentModelTypeId(1), "POINT_KINETICS", "Point Kinetics Surrogate", nowUtc);
        var stateSpaceType = StateSpaceType.Create(new StateSpaceTypeId(1), "DEVIATION_TREND", "Deviation x Trend", nowUtc);
        var actionSpaceType = ActionSpaceType.Create(new ActionSpaceTypeId(1), "ROD_MOVE", "Rod Move", nowUtc);
        var rewardFunctionType = RewardFunctionType.Create(new RewardFunctionTypeId(1), "ERROR_PENALTY", "Error and Move Penalty", nowUtc);
        var learningAlgorithm = LearningAlgorithm.Create(new LearningAlgorithmId(1), "Q_LEARNING", "Q-Learning", nowUtc);
        var trainingRunStatus = TrainingRunStatus.Create(new TrainingRunStatusId(1), "COMPLETED", "Completed", nowUtc);
        var policyStatus = PolicyStatus.Create(new PolicyStatusId(1), "EXTRACTED", "Extracted", nowUtc);
        var advisoryMode = AdvisoryMode.Create(new AdvisoryModeId(1), "SHADOW", "Shadow Advisory", nowUtc);
        var recommendationStatus = RecommendationStatus.Create(new RecommendationStatusId(1), "OFFERED", "Offered", nowUtc);

        await rlContext.EnvironmentModelTypes.AddAsync(environmentModelType);
        await rlContext.StateSpaceTypes.AddAsync(stateSpaceType);
        await rlContext.ActionSpaceTypes.AddAsync(actionSpaceType);
        await rlContext.RewardFunctionTypes.AddAsync(rewardFunctionType);
        await rlContext.LearningAlgorithms.AddAsync(learningAlgorithm);
        await rlContext.TrainingRunStatuses.AddAsync(trainingRunStatus);
        await rlContext.PolicyStatuses.AddAsync(policyStatus);
        await rlContext.AdvisoryModes.AddAsync(advisoryMode);
        await rlContext.RecommendationStatuses.AddAsync(recommendationStatus);
        await rlContext.SaveChangesAsync();

        var environmentModel = EnvironmentModel.Create(
            new EnvironmentModelId(1), environmentModelType.Id, unit.Id.Value, EnvironmentModelCode,
            "Point Kinetics Surrogate v1", "v1.0", 1.0m, twinModel.Id.Value);
        await rlContext.EnvironmentModels.AddAsync(environmentModel);
        await rlContext.SaveChangesAsync();

        var stateSpace = StateSpace.Create(new StateSpaceId(1), stateSpaceType.Id, StateSpaceCode, "Deviation x Trend Grid", 2);
        await rlContext.StateSpaces.AddAsync(stateSpace);
        await rlContext.SaveChangesAsync();

        var stateDefinition1 = StateDefinition.Create(new StateDefinitionId(1), stateSpace.Id, 0, "S0", "On Target, Steady");
        var stateDefinition2 = StateDefinition.Create(new StateDefinitionId(2), stateSpace.Id, 1, "S1", "Low, Rising");
        await rlContext.StateDefinitions.AddAsync(stateDefinition1);
        await rlContext.StateDefinitions.AddAsync(stateDefinition2);
        await rlContext.SaveChangesAsync();

        var actionSpace = ActionSpace.Create(new ActionSpaceId(1), actionSpaceType.Id, ActionSpaceCode, "Rod Moves", engineeringUnit.Id.Value);
        await rlContext.ActionSpaces.AddAsync(actionSpace);
        await rlContext.SaveChangesAsync();

        var actionDefinition1 = ActionDefinition.Create(new ActionDefinitionId(1), actionSpace.Id, 0, "A0", "Hold", 0m, isNoOp: true);
        var actionDefinition2 = ActionDefinition.Create(new ActionDefinitionId(2), actionSpace.Id, 1, "A1", "Withdraw Small Step", 0.5m);
        await rlContext.ActionDefinitions.AddAsync(actionDefinition1);
        await rlContext.ActionDefinitions.AddAsync(actionDefinition2);
        await rlContext.SaveChangesAsync();

        var rewardFunction = RewardFunction.Create(
            new RewardFunctionId(1), rewardFunctionType.Id, RewardFunctionCode, "Error and Move Penalty",
            "reward = -ErrorWeight * |error| - MovePenalty * |move|");
        await rlContext.RewardFunctions.AddAsync(rewardFunction);
        await rlContext.SaveChangesAsync();

        var hyperparameterSet = HyperparameterSet.Create(
            new HyperparameterSetId(1), HyperparameterSetCode, "Default Hyperparameters", 0.1m, 0.95m, 1.0m, 0.05m,
            0.995m, 500, 200, randomSeed: 42);
        await rlContext.HyperparameterSets.AddAsync(hyperparameterSet);
        await rlContext.SaveChangesAsync();

        var experiment = Experiment.Create(new ExperimentId(1), unit.Id.Value, ExperimentCode, "Baseline RL Experiment", ownerUserId: 701);
        await rlContext.Experiments.AddAsync(experiment);
        await rlContext.SaveChangesAsync();

        var trainingRun = TrainingRun.Create(
            new TrainingRunId(1), experiment.Id, environmentModel.Id, stateSpace.Id, actionSpace.Id, rewardFunction.Id,
            hyperparameterSet.Id, learningAlgorithm.Id, trainingRunStatus.Id, TrainingRunCode,
            startedAtUtc: nowUtc.AddHours(-2), completedAtUtc: nowUtc.AddHours(-1), episodeCountCompleted: 500,
            totalReward: 1200.5m, averageReward: 2.401m, runSeed: 42);
        await rlContext.TrainingRuns.AddAsync(trainingRun);
        await rlContext.SaveChangesAsync();

        var qTable = QTable.Create(new QTableId(1), trainingRun.Id, stateSpace.Id, actionSpace.Id, QTableCode, nowUtc, entryCount: 4, isFinal: true);
        await rlContext.QTables.AddAsync(qTable);
        await rlContext.SaveChangesAsync();

        var qTableEntries = new[]
        {
            QTableEntry.Create(new QTableEntryId(1), qTable.Id, stateDefinition1.Id, actionDefinition1.Id, 10.5m, visitCount: 12),
            QTableEntry.Create(new QTableEntryId(2), qTable.Id, stateDefinition1.Id, actionDefinition2.Id, 8.1m, visitCount: 9),
            QTableEntry.Create(new QTableEntryId(3), qTable.Id, stateDefinition2.Id, actionDefinition1.Id, 5.2m, visitCount: 7),
            QTableEntry.Create(new QTableEntryId(4), qTable.Id, stateDefinition2.Id, actionDefinition2.Id, 12.9m, visitCount: 15),
        };
        foreach (var entry in qTableEntries)
        {
            await rlContext.QTableEntries.AddAsync(entry);
        }

        await rlContext.SaveChangesAsync();

        var policy = Policy.Create(new PolicyId(1), qTable.Id, policyStatus.Id, PolicyCode, "Extracted Policy v1", nowUtc, entryCount: 2);
        await rlContext.Policies.AddAsync(policy);
        await rlContext.SaveChangesAsync();

        var policyEntries = new[]
        {
            PolicyEntry.Create(new PolicyEntryId(1), policy.Id, stateDefinition1.Id, actionDefinition1.Id, 10.5m, 8.1m, 2.4m),
            PolicyEntry.Create(new PolicyEntryId(2), policy.Id, stateDefinition2.Id, actionDefinition2.Id, 12.9m, 5.2m, 7.7m),
        };
        foreach (var entry in policyEntries)
        {
            await rlContext.PolicyEntries.AddAsync(entry);
        }

        await rlContext.SaveChangesAsync();

        var policyDeployment = PolicyDeployment.Create(
            new PolicyDeploymentId(1), policy.Id, advisoryMode.Id, unit.Id.Value, nowUtc.AddMinutes(-30), deployedByUserId: 702);
        await rlContext.PolicyDeployments.AddAsync(policyDeployment);
        await rlContext.SaveChangesAsync();

        var advisorySession = AdvisorySession.Create(
            new AdvisorySessionId(1), policyDeployment.Id, unit.Id.Value, nowUtc.AddMinutes(-10), startedByUserId: 703);
        await rlContext.AdvisorySessions.AddAsync(advisorySession);
        await rlContext.SaveChangesAsync();

        var advisoryRecommendation = AdvisoryRecommendation.Create(
            new AdvisoryRecommendationId(1), advisorySession.Id, recommendationStatus.Id, stateDefinition2.Id,
            actionDefinition2.Id, nowUtc.AddMinutes(-9), clampedActionDefinitionId: actionDefinition1.Id,
            observedPowerPercent: 92.5m, targetPowerPercent: 100m, confidenceScore: 0.82m, wasClamped: true,
            clampReason: "Clamped to validated band.");
        await rlContext.AdvisoryRecommendations.AddAsync(advisoryRecommendation);
        await rlContext.SaveChangesAsync();

        return new SeedResult(
            unit.Id.Value, engineeringUnit.Id.Value, twinModel.Id.Value, environmentModelType.Id.Value,
            stateSpaceType.Id.Value, actionSpaceType.Id.Value, rewardFunctionType.Id.Value, learningAlgorithm.Id.Value,
            trainingRunStatus.Id.Value, policyStatus.Id.Value, advisoryMode.Id.Value, recommendationStatus.Id.Value,
            environmentModel.Id.Value, stateSpace.Id.Value, stateDefinition1.Id.Value, stateDefinition2.Id.Value,
            actionSpace.Id.Value, actionDefinition1.Id.Value, actionDefinition2.Id.Value, rewardFunction.Id.Value,
            hyperparameterSet.Id.Value, experiment.Id.Value, trainingRun.Id.Value, qTable.Id.Value, policy.Id.Value,
            policyDeployment.Id.Value, advisorySession.Id.Value, advisoryRecommendation.Id.Value);
    }
}
