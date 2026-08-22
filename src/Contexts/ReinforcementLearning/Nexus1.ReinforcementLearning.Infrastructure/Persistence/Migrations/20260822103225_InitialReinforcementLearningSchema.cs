using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.ReinforcementLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReinforcementLearningSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ReinforcementLearning");

            migrationBuilder.CreateTable(
                name: "ActionSpaceType",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    ActionSpaceTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_ActionSpaceType", x => x.ActionSpaceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AdvisoryMode",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    AdvisoryModeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_AdvisoryMode", x => x.AdvisoryModeId);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentModelType",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    EnvironmentModelTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_EnvironmentModelType", x => x.EnvironmentModelTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Experiment",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_Experiment", x => x.ExperimentId);
                    table.ForeignKey(
                        name: "FK_RL_Experiment_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HyperparameterSet",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    HyperparameterSetId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LearningRateAlpha = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    DiscountGamma = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EpsilonStart = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EpsilonEnd = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EpsilonDecay = table.Column<decimal>(type: "decimal(10,6)", nullable: false),
                    EpisodeCount = table.Column<int>(type: "int", nullable: false),
                    StepsPerEpisode = table.Column<int>(type: "int", nullable: false),
                    RandomSeed = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_HyperparameterSet", x => x.HyperparameterSetId);
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_DiscountGamma", "[DiscountGamma] >= 0 AND [DiscountGamma] <= 1");
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpisodeCount", "[EpisodeCount] > 0");
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpsilonEnd", "[EpsilonEnd] >= 0 AND [EpsilonEnd] <= 1");
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_EpsilonStart", "[EpsilonStart] >= 0 AND [EpsilonStart] <= 1");
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_LearningRateAlpha", "[LearningRateAlpha] > 0 AND [LearningRateAlpha] <= 1");
                    table.CheckConstraint("CK_ReinforcementLearning_HyperparameterSet_StepsPerEpisode", "[StepsPerEpisode] > 0");
                });

            migrationBuilder.CreateTable(
                name: "LearningAlgorithm",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    LearningAlgorithmId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_LearningAlgorithm", x => x.LearningAlgorithmId);
                });

            migrationBuilder.CreateTable(
                name: "PolicyStatus",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    PolicyStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_PolicyStatus", x => x.PolicyStatusId);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationStatus",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    RecommendationStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_RecommendationStatus", x => x.RecommendationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "RewardFunctionType",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    RewardFunctionTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_RewardFunctionType", x => x.RewardFunctionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "StateSpaceType",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    StateSpaceTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_StateSpaceType", x => x.StateSpaceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRunStatus",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    TrainingRunStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_TrainingRunStatus", x => x.TrainingRunStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ActionSpace",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    ActionSpaceId = table.Column<int>(type: "int", nullable: false),
                    ActionSpaceTypeId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_ActionSpace", x => x.ActionSpaceId);
                    table.ForeignKey(
                        name: "FK_RL_ActionSpace_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_ActionSpace_ActionSpaceType",
                        column: x => x.ActionSpaceTypeId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionSpaceType",
                        principalColumn: "ActionSpaceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentModel",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    EnvironmentModelId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentModelTypeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    TwinModelId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VersionLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeStepSeconds = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    IsDeterministic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RandomSeed = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_EnvironmentModel", x => x.EnvironmentModelId);
                    table.CheckConstraint("CK_ReinforcementLearning_EnvironmentModel_TimeStepSeconds", "[TimeStepSeconds] > 0");
                    table.ForeignKey(
                        name: "FK_RL_EnvironmentModel_TwinModel",
                        column: x => x.TwinModelId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModel",
                        principalColumn: "TwinModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RL_EnvironmentModel_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_EnvironmentModel_EnvironmentModelType",
                        column: x => x.EnvironmentModelTypeId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "EnvironmentModelType",
                        principalColumn: "EnvironmentModelTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RewardFunction",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    RewardFunctionId = table.Column<int>(type: "int", nullable: false),
                    RewardFunctionTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FormulaText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ErrorWeight = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 100.0m),
                    MovePenalty = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 0.3m),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_RewardFunction", x => x.RewardFunctionId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_RewardFunction_RewardFunctionType",
                        column: x => x.RewardFunctionTypeId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "RewardFunctionType",
                        principalColumn: "RewardFunctionTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StateSpace",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    StateSpaceId = table.Column<int>(type: "int", nullable: false),
                    StateSpaceTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DimensionCount = table.Column<int>(type: "int", nullable: false),
                    IsDiscrete = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_StateSpace", x => x.StateSpaceId);
                    table.CheckConstraint("CK_ReinforcementLearning_StateSpace_DimensionCount", "[DimensionCount] > 0");
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_StateSpace_StateSpaceType",
                        column: x => x.StateSpaceTypeId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateSpaceType",
                        principalColumn: "StateSpaceTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActionDefinition",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    ActionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ActionSpaceId = table.Column<int>(type: "int", nullable: false),
                    ActionIndex = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsNoOp = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_ActionDefinition", x => x.ActionDefinitionId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_ActionDefinition_ActionSpace",
                        column: x => x.ActionSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionSpace",
                        principalColumn: "ActionSpaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StateDefinition",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    StateDefinitionId = table.Column<int>(type: "int", nullable: false),
                    StateSpaceId = table.Column<int>(type: "int", nullable: false),
                    StateIndex = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeviationBin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrendBin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsTerminal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_StateDefinition", x => x.StateDefinitionId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_StateDefinition_StateSpace",
                        column: x => x.StateSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateSpace",
                        principalColumn: "StateSpaceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRun",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    TrainingRunId = table.Column<int>(type: "int", nullable: false),
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    EnvironmentModelId = table.Column<int>(type: "int", nullable: false),
                    StateSpaceId = table.Column<int>(type: "int", nullable: false),
                    ActionSpaceId = table.Column<int>(type: "int", nullable: false),
                    RewardFunctionId = table.Column<int>(type: "int", nullable: false),
                    HyperparameterSetId = table.Column<int>(type: "int", nullable: false),
                    LearningAlgorithmId = table.Column<int>(type: "int", nullable: false),
                    TrainingRunStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EpisodeCountCompleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalReward = table.Column<decimal>(type: "decimal(20,6)", nullable: true),
                    AverageReward = table.Column<decimal>(type: "decimal(20,6)", nullable: true),
                    RunSeed = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_TrainingRun", x => x.TrainingRunId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_ActionSpace",
                        column: x => x.ActionSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionSpace",
                        principalColumn: "ActionSpaceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_EnvironmentModel",
                        column: x => x.EnvironmentModelId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "EnvironmentModel",
                        principalColumn: "EnvironmentModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_Experiment",
                        column: x => x.ExperimentId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "Experiment",
                        principalColumn: "ExperimentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_HyperparameterSet",
                        column: x => x.HyperparameterSetId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "HyperparameterSet",
                        principalColumn: "HyperparameterSetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_LearningAlgorithm",
                        column: x => x.LearningAlgorithmId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "LearningAlgorithm",
                        principalColumn: "LearningAlgorithmId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_RewardFunction",
                        column: x => x.RewardFunctionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "RewardFunction",
                        principalColumn: "RewardFunctionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_StateSpace",
                        column: x => x.StateSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateSpace",
                        principalColumn: "StateSpaceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_TrainingRun_TrainingRunStatus",
                        column: x => x.TrainingRunStatusId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "TrainingRunStatus",
                        principalColumn: "TrainingRunStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QTable",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    QTableId = table.Column<int>(type: "int", nullable: false),
                    TrainingRunId = table.Column<int>(type: "int", nullable: false),
                    StateSpaceId = table.Column<int>(type: "int", nullable: false),
                    ActionSpaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SnapshotAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    EntryCount = table.Column<int>(type: "int", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_QTable", x => x.QTableId);
                    table.CheckConstraint("CK_ReinforcementLearning_QTable_EntryCount", "[EntryCount] > 0");
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTable_ActionSpace",
                        column: x => x.ActionSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionSpace",
                        principalColumn: "ActionSpaceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTable_StateSpace",
                        column: x => x.StateSpaceId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateSpace",
                        principalColumn: "StateSpaceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTable_TrainingRun",
                        column: x => x.TrainingRunId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "TrainingRun",
                        principalColumn: "TrainingRunId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Policy",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    QTableId = table.Column<int>(type: "int", nullable: false),
                    PolicyStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExtractedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_Policy", x => x.PolicyId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_Policy_PolicyStatus",
                        column: x => x.PolicyStatusId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "PolicyStatus",
                        principalColumn: "PolicyStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_Policy_QTable",
                        column: x => x.QTableId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "QTable",
                        principalColumn: "QTableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QTableEntry",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    QTableEntryId = table.Column<long>(type: "bigint", nullable: false),
                    QTableId = table.Column<int>(type: "int", nullable: false),
                    StateDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ActionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    QValue = table.Column<decimal>(type: "decimal(20,10)", nullable: false),
                    VisitCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_QTableEntry", x => x.QTableEntryId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTableEntry_ActionDefinition",
                        column: x => x.ActionDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionDefinition",
                        principalColumn: "ActionDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTableEntry_QTable",
                        column: x => x.QTableId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "QTable",
                        principalColumn: "QTableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_QTableEntry_StateDefinition",
                        column: x => x.StateDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateDefinition",
                        principalColumn: "StateDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyDeployment",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    PolicyDeploymentId = table.Column<int>(type: "int", nullable: false),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    AdvisoryModeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DeployedByUserId = table.Column<int>(type: "int", nullable: true),
                    DeployedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeploymentNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_PolicyDeployment", x => x.PolicyDeploymentId);
                    table.ForeignKey(
                        name: "FK_RL_PolicyDeployment_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_PolicyDeployment_AdvisoryMode",
                        column: x => x.AdvisoryModeId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "AdvisoryMode",
                        principalColumn: "AdvisoryModeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_PolicyDeployment_Policy",
                        column: x => x.PolicyId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "Policy",
                        principalColumn: "PolicyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyEntry",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    PolicyEntryId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    StateDefinitionId = table.Column<int>(type: "int", nullable: false),
                    BestActionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    BestQValue = table.Column<decimal>(type: "decimal(20,10)", nullable: false),
                    SecondBestQValue = table.Column<decimal>(type: "decimal(20,10)", nullable: true),
                    ActionMargin = table.Column<decimal>(type: "decimal(20,10)", nullable: true),
                    IsTie = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_PolicyEntry", x => x.PolicyEntryId);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_PolicyEntry_ActionDefinition",
                        column: x => x.BestActionDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionDefinition",
                        principalColumn: "ActionDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_PolicyEntry_Policy",
                        column: x => x.PolicyId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "Policy",
                        principalColumn: "PolicyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_PolicyEntry_StateDefinition",
                        column: x => x.StateDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateDefinition",
                        principalColumn: "StateDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvisorySession",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    AdvisorySessionId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyDeploymentId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    StartedByUserId = table.Column<int>(type: "int", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_AdvisorySession", x => x.AdvisorySessionId);
                    table.ForeignKey(
                        name: "FK_RL_AdvisorySession_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_AdvisorySession_PolicyDeployment",
                        column: x => x.PolicyDeploymentId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "PolicyDeployment",
                        principalColumn: "PolicyDeploymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvisoryRecommendation",
                schema: "ReinforcementLearning",
                columns: table => new
                {
                    AdvisoryRecommendationId = table.Column<long>(type: "bigint", nullable: false),
                    AdvisorySessionId = table.Column<long>(type: "bigint", nullable: false),
                    RecommendationStatusId = table.Column<int>(type: "int", nullable: false),
                    StateDefinitionId = table.Column<int>(type: "int", nullable: false),
                    RecommendedActionDefinitionId = table.Column<int>(type: "int", nullable: false),
                    ClampedActionDefinitionId = table.Column<int>(type: "int", nullable: true),
                    ObservedPowerPercent = table.Column<decimal>(type: "decimal(12,6)", nullable: true),
                    TargetPowerPercent = table.Column<decimal>(type: "decimal(12,6)", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(10,6)", nullable: true),
                    WasClamped = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ClampReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReinforcementLearning_AdvisoryRecommendation", x => x.AdvisoryRecommendationId);
                    table.CheckConstraint("CK_ReinforcementLearning_AdvisoryRecommendation_ConfidenceScore", "[ConfidenceScore] IS NULL OR ([ConfidenceScore] >= 0 AND [ConfidenceScore] <= 1)");
                    table.ForeignKey(
                        name: "FK_RL_AdvisoryRecommendation_Action",
                        column: x => x.RecommendedActionDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionDefinition",
                        principalColumn: "ActionDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RL_AdvisoryRecommendation_ClampedAction",
                        column: x => x.ClampedActionDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "ActionDefinition",
                        principalColumn: "ActionDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_AdvisoryRecommendation_AdvisorySession",
                        column: x => x.AdvisorySessionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "AdvisorySession",
                        principalColumn: "AdvisorySessionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_AdvisoryRecommendation_RecommendationStatus",
                        column: x => x.RecommendationStatusId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "RecommendationStatus",
                        principalColumn: "RecommendationStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReinforcementLearning_AdvisoryRecommendation_StateDefinition",
                        column: x => x.StateDefinitionId,
                        principalSchema: "ReinforcementLearning",
                        principalTable: "StateDefinition",
                        principalColumn: "StateDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_ActionDefinition_ActionSpace_ActionIndex",
                schema: "ReinforcementLearning",
                table: "ActionDefinition",
                columns: new[] { "ActionSpaceId", "ActionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_ActionDefinition_ActionSpace_Code",
                schema: "ReinforcementLearning",
                table: "ActionDefinition",
                columns: new[] { "ActionSpaceId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionSpace_ActionSpaceTypeId",
                schema: "ReinforcementLearning",
                table: "ActionSpace",
                column: "ActionSpaceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionSpace_EngineeringUnitId",
                schema: "ReinforcementLearning",
                table: "ActionSpace",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_ActionSpace_Code",
                schema: "ReinforcementLearning",
                table: "ActionSpace",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_ActionSpaceType_Code",
                schema: "ReinforcementLearning",
                table: "ActionSpaceType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_AdvisoryMode_Code",
                schema: "ReinforcementLearning",
                table: "AdvisoryMode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvisoryRecommendation_AdvisorySessionId",
                schema: "ReinforcementLearning",
                table: "AdvisoryRecommendation",
                column: "AdvisorySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisoryRecommendation_ClampedActionDefinitionId",
                schema: "ReinforcementLearning",
                table: "AdvisoryRecommendation",
                column: "ClampedActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisoryRecommendation_RecommendationStatusId",
                schema: "ReinforcementLearning",
                table: "AdvisoryRecommendation",
                column: "RecommendationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisoryRecommendation_RecommendedActionDefinitionId",
                schema: "ReinforcementLearning",
                table: "AdvisoryRecommendation",
                column: "RecommendedActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisoryRecommendation_StateDefinitionId",
                schema: "ReinforcementLearning",
                table: "AdvisoryRecommendation",
                column: "StateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisorySession_PolicyDeploymentId",
                schema: "ReinforcementLearning",
                table: "AdvisorySession",
                column: "PolicyDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvisorySession_UnitId",
                schema: "ReinforcementLearning",
                table: "AdvisorySession",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentModel_EnvironmentModelTypeId",
                schema: "ReinforcementLearning",
                table: "EnvironmentModel",
                column: "EnvironmentModelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentModel_TwinModelId",
                schema: "ReinforcementLearning",
                table: "EnvironmentModel",
                column: "TwinModelId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentModel_UnitId",
                schema: "ReinforcementLearning",
                table: "EnvironmentModel",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_EnvironmentModel_Code",
                schema: "ReinforcementLearning",
                table: "EnvironmentModel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_EnvironmentModelType_Code",
                schema: "ReinforcementLearning",
                table: "EnvironmentModelType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Experiment_UnitId",
                schema: "ReinforcementLearning",
                table: "Experiment",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_Experiment_Code",
                schema: "ReinforcementLearning",
                table: "Experiment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_HyperparameterSet_Code",
                schema: "ReinforcementLearning",
                table: "HyperparameterSet",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_LearningAlgorithm_Code",
                schema: "ReinforcementLearning",
                table: "LearningAlgorithm",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policy_PolicyStatusId",
                schema: "ReinforcementLearning",
                table: "Policy",
                column: "PolicyStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Policy_QTableId",
                schema: "ReinforcementLearning",
                table: "Policy",
                column: "QTableId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_Policy_Code",
                schema: "ReinforcementLearning",
                table: "Policy",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDeployment_AdvisoryModeId",
                schema: "ReinforcementLearning",
                table: "PolicyDeployment",
                column: "AdvisoryModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDeployment_PolicyId",
                schema: "ReinforcementLearning",
                table: "PolicyDeployment",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyDeployment_UnitId",
                schema: "ReinforcementLearning",
                table: "PolicyDeployment",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyEntry_BestActionDefinitionId",
                schema: "ReinforcementLearning",
                table: "PolicyEntry",
                column: "BestActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyEntry_StateDefinitionId",
                schema: "ReinforcementLearning",
                table: "PolicyEntry",
                column: "StateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_PolicyEntry_Policy_StateDefinition",
                schema: "ReinforcementLearning",
                table: "PolicyEntry",
                columns: new[] { "PolicyId", "StateDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_PolicyStatus_Code",
                schema: "ReinforcementLearning",
                table: "PolicyStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QTable_ActionSpaceId",
                schema: "ReinforcementLearning",
                table: "QTable",
                column: "ActionSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_QTable_StateSpaceId",
                schema: "ReinforcementLearning",
                table: "QTable",
                column: "StateSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_QTable_TrainingRunId",
                schema: "ReinforcementLearning",
                table: "QTable",
                column: "TrainingRunId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_QTable_Code",
                schema: "ReinforcementLearning",
                table: "QTable",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QTableEntry_ActionDefinitionId",
                schema: "ReinforcementLearning",
                table: "QTableEntry",
                column: "ActionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_QTableEntry_StateDefinitionId",
                schema: "ReinforcementLearning",
                table: "QTableEntry",
                column: "StateDefinitionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_QTableEntry_QTable_State_Action",
                schema: "ReinforcementLearning",
                table: "QTableEntry",
                columns: new[] { "QTableId", "StateDefinitionId", "ActionDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_RecommendationStatus_Code",
                schema: "ReinforcementLearning",
                table: "RecommendationStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RewardFunction_RewardFunctionTypeId",
                schema: "ReinforcementLearning",
                table: "RewardFunction",
                column: "RewardFunctionTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_RewardFunction_Code",
                schema: "ReinforcementLearning",
                table: "RewardFunction",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_RewardFunctionType_Code",
                schema: "ReinforcementLearning",
                table: "RewardFunctionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_StateDefinition_StateSpace_Code",
                schema: "ReinforcementLearning",
                table: "StateDefinition",
                columns: new[] { "StateSpaceId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_StateDefinition_StateSpace_StateIndex",
                schema: "ReinforcementLearning",
                table: "StateDefinition",
                columns: new[] { "StateSpaceId", "StateIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StateSpace_StateSpaceTypeId",
                schema: "ReinforcementLearning",
                table: "StateSpace",
                column: "StateSpaceTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_StateSpace_Code",
                schema: "ReinforcementLearning",
                table: "StateSpace",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_StateSpaceType_Code",
                schema: "ReinforcementLearning",
                table: "StateSpaceType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_ActionSpaceId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "ActionSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_EnvironmentModelId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "EnvironmentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_ExperimentId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_HyperparameterSetId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "HyperparameterSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_LearningAlgorithmId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "LearningAlgorithmId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_RewardFunctionId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "RewardFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_StateSpaceId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "StateSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRun_TrainingRunStatusId",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "TrainingRunStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_TrainingRun_Code",
                schema: "ReinforcementLearning",
                table: "TrainingRun",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ReinforcementLearning_TrainingRunStatus_Code",
                schema: "ReinforcementLearning",
                table: "TrainingRunStatus",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvisoryRecommendation",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "PolicyEntry",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "QTableEntry",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "AdvisorySession",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "RecommendationStatus",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "ActionDefinition",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "StateDefinition",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "PolicyDeployment",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "AdvisoryMode",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "Policy",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "PolicyStatus",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "QTable",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "TrainingRun",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "ActionSpace",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "EnvironmentModel",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "Experiment",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "HyperparameterSet",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "LearningAlgorithm",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "RewardFunction",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "StateSpace",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "TrainingRunStatus",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "ActionSpaceType",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "EnvironmentModelType",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "RewardFunctionType",
                schema: "ReinforcementLearning");

            migrationBuilder.DropTable(
                name: "StateSpaceType",
                schema: "ReinforcementLearning");
        }
    }
}
