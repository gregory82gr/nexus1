using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.DigitalTwin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDigitalTwinSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "DigitalTwin");

            migrationBuilder.CreateTable(
                name: "BindingRole",
                schema: "DigitalTwin",
                columns: table => new
                {
                    BindingRoleId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_BindingRole", x => x.BindingRoleId);
                });

            migrationBuilder.CreateTable(
                name: "BindingStatus",
                schema: "DigitalTwin",
                columns: table => new
                {
                    BindingStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_BindingStatus", x => x.BindingStatusId);
                });

            migrationBuilder.CreateTable(
                name: "DivergenceSeverity",
                schema: "DigitalTwin",
                columns: table => new
                {
                    DivergenceSeverityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_DivergenceSeverity", x => x.DivergenceSeverityId);
                });

            migrationBuilder.CreateTable(
                name: "DivergenceStatus",
                schema: "DigitalTwin",
                columns: table => new
                {
                    DivergenceStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_DivergenceStatus", x => x.DivergenceStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ModelVariableType",
                schema: "DigitalTwin",
                columns: table => new
                {
                    ModelVariableTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_ModelVariableType", x => x.ModelVariableTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotReason",
                schema: "DigitalTwin",
                columns: table => new
                {
                    SnapshotReasonId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_SnapshotReason", x => x.SnapshotReasonId);
                });

            migrationBuilder.CreateTable(
                name: "SolverType",
                schema: "DigitalTwin",
                columns: table => new
                {
                    SolverTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_SolverType", x => x.SolverTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TwinFidelityLevel",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinFidelityLevelId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinFidelityLevel", x => x.TwinFidelityLevelId);
                });

            migrationBuilder.CreateTable(
                name: "TwinModelStatus",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinModelStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinModelStatus", x => x.TwinModelStatusId);
                });

            migrationBuilder.CreateTable(
                name: "TwinModelType",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinModelTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinModelType", x => x.TwinModelTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ValidationStatus",
                schema: "DigitalTwin",
                columns: table => new
                {
                    ValidationStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_ValidationStatus", x => x.ValidationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "TwinModel",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinModelId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    TwinModelTypeId = table.Column<int>(type: "int", nullable: false),
                    TwinModelStatusId = table.Column<int>(type: "int", nullable: false),
                    TwinFidelityLevelId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ModelOwner = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsAuthoritative = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinModel", x => x.TwinModelId);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModel_TwinFidelityLevel",
                        column: x => x.TwinFidelityLevelId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinFidelityLevel",
                        principalColumn: "TwinFidelityLevelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModel_TwinModelStatus",
                        column: x => x.TwinModelStatusId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModelStatus",
                        principalColumn: "TwinModelStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModel_TwinModelType",
                        column: x => x.TwinModelTypeId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModelType",
                        principalColumn: "TwinModelTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModel_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinModelVersion",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinModelVersionId = table.Column<int>(type: "int", nullable: false),
                    TwinModelId = table.Column<int>(type: "int", nullable: false),
                    SolverTypeId = table.Column<int>(type: "int", nullable: false),
                    ValidationStatusId = table.Column<int>(type: "int", nullable: false),
                    VersionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SourceReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModelHash = table.Column<byte[]>(type: "varbinary(32)", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinModelVersion", x => x.TwinModelVersionId);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModelVersion_SolverType",
                        column: x => x.SolverTypeId,
                        principalSchema: "DigitalTwin",
                        principalTable: "SolverType",
                        principalColumn: "SolverTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModelVersion_TwinModel",
                        column: x => x.TwinModelId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModel",
                        principalColumn: "TwinModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinModelVersion_ValidationStatus",
                        column: x => x.ValidationStatusId,
                        principalSchema: "DigitalTwin",
                        principalTable: "ValidationStatus",
                        principalColumn: "ValidationStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinVariable",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinVariableId = table.Column<int>(type: "int", nullable: false),
                    TwinModelId = table.Column<int>(type: "int", nullable: false),
                    ModelVariableTypeId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsStateVariable = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    LowerBound = table.Column<double>(type: "float", nullable: true),
                    UpperBound = table.Column<double>(type: "float", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinVariable", x => x.TwinVariableId);
                    table.CheckConstraint("CK_DigitalTwin_TwinVariable_Bounds", "[LowerBound] IS NULL OR [UpperBound] IS NULL OR [LowerBound] <= [UpperBound]");
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinVariable_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinVariable_ModelVariableType",
                        column: x => x.ModelVariableTypeId,
                        principalSchema: "DigitalTwin",
                        principalTable: "ModelVariableType",
                        principalColumn: "ModelVariableTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinVariable_TwinModel",
                        column: x => x.TwinModelId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModel",
                        principalColumn: "TwinModelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinRuntimeSession",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinRuntimeSessionId = table.Column<long>(type: "bigint", nullable: false),
                    TwinModelVersionId = table.Column<int>(type: "int", nullable: false),
                    StartedByUserId = table.Column<int>(type: "int", nullable: true),
                    SessionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RuntimeMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinRuntimeSession", x => x.TwinRuntimeSessionId);
                    table.CheckConstraint("CK_DigitalTwin_TwinRuntimeSession_TimeRange", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]");
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinRuntimeSession_TwinModelVersion",
                        column: x => x.TwinModelVersionId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModelVersion",
                        principalColumn: "TwinModelVersionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SignalBinding",
                schema: "DigitalTwin",
                columns: table => new
                {
                    SignalBindingId = table.Column<int>(type: "int", nullable: false),
                    TwinModelId = table.Column<int>(type: "int", nullable: false),
                    TwinVariableId = table.Column<int>(type: "int", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    BindingRoleId = table.Column<int>(type: "int", nullable: false),
                    BindingStatusId = table.Column<int>(type: "int", nullable: false),
                    ModelVariable = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ScaleFactor = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    OffsetValue = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    ToleranceAbs = table.Column<double>(type: "float", nullable: true),
                    TolerancePercent = table.Column<double>(type: "float", nullable: true),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_SignalBinding", x => x.SignalBindingId);
                    table.CheckConstraint("CK_DigitalTwin_SignalBinding_EffectiveRange", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
                    table.ForeignKey(
                        name: "FK_DigitalTwin_SignalBinding_BindingRole",
                        column: x => x.BindingRoleId,
                        principalSchema: "DigitalTwin",
                        principalTable: "BindingRole",
                        principalColumn: "BindingRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_SignalBinding_BindingStatus",
                        column: x => x.BindingStatusId,
                        principalSchema: "DigitalTwin",
                        principalTable: "BindingStatus",
                        principalColumn: "BindingStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_SignalBinding_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_SignalBinding_TwinModel",
                        column: x => x.TwinModelId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinModel",
                        principalColumn: "TwinModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_SignalBinding_TwinVariable",
                        column: x => x.TwinVariableId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinVariable",
                        principalColumn: "TwinVariableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinSnapshot",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    TwinRuntimeSessionId = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotReasonId = table.Column<int>(type: "int", nullable: false),
                    SnapshotAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStepIndex = table.Column<long>(type: "bigint", nullable: true),
                    StateVectorHash = table.Column<byte[]>(type: "varbinary(32)", nullable: true),
                    SummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinSnapshot", x => x.TwinSnapshotId);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinSnapshot_SnapshotReason",
                        column: x => x.SnapshotReasonId,
                        principalSchema: "DigitalTwin",
                        principalTable: "SnapshotReason",
                        principalColumn: "SnapshotReasonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinSnapshot_TwinRuntimeSession",
                        column: x => x.TwinRuntimeSessionId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinRuntimeSession",
                        principalColumn: "TwinRuntimeSessionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinDivergence",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinDivergenceId = table.Column<long>(type: "bigint", nullable: false),
                    TwinSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    TwinVariableId = table.Column<int>(type: "int", nullable: true),
                    DivergenceSeverityId = table.Column<int>(type: "int", nullable: false),
                    DivergenceStatusId = table.Column<int>(type: "int", nullable: false),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModeledValue = table.Column<double>(type: "float", nullable: false),
                    MeasuredValue = table.Column<double>(type: "float", nullable: false),
                    DeltaValue = table.Column<double>(type: "float", nullable: false),
                    DeltaPercent = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    ThresholdAbs = table.Column<double>(type: "float", nullable: true),
                    ThresholdPercent = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinDivergence", x => x.TwinDivergenceId);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergence_DivergenceSeverity",
                        column: x => x.DivergenceSeverityId,
                        principalSchema: "DigitalTwin",
                        principalTable: "DivergenceSeverity",
                        principalColumn: "DivergenceSeverityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergence_DivergenceStatus",
                        column: x => x.DivergenceStatusId,
                        principalSchema: "DigitalTwin",
                        principalTable: "DivergenceStatus",
                        principalColumn: "DivergenceStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergence_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergence_TwinSnapshot",
                        column: x => x.TwinSnapshotId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinSnapshot",
                        principalColumn: "TwinSnapshotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergence_TwinVariable",
                        column: x => x.TwinVariableId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinVariable",
                        principalColumn: "TwinVariableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinSnapshotValue",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinSnapshotValueId = table.Column<long>(type: "bigint", nullable: false),
                    TwinSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    TwinVariableId = table.Column<int>(type: "int", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    JsonValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinSnapshotValue", x => x.TwinSnapshotValueId);
                    table.CheckConstraint("CK_DigitalTwin_TwinSnapshotValue_OneValue", "[NumericValue] IS NOT NULL OR [TextValue] IS NOT NULL OR [JsonValue] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinSnapshotValue_TwinSnapshot",
                        column: x => x.TwinSnapshotId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinSnapshot",
                        principalColumn: "TwinSnapshotId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinSnapshotValue_TwinVariable",
                        column: x => x.TwinVariableId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinVariable",
                        principalColumn: "TwinVariableId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwinDivergenceReview",
                schema: "DigitalTwin",
                columns: table => new
                {
                    TwinDivergenceReviewId = table.Column<long>(type: "bigint", nullable: false),
                    TwinDivergenceId = table.Column<long>(type: "bigint", nullable: false),
                    DivergenceStatusId = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalTwin_TwinDivergenceReview", x => x.TwinDivergenceReviewId);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergenceReview_DivergenceStatus",
                        column: x => x.DivergenceStatusId,
                        principalSchema: "DigitalTwin",
                        principalTable: "DivergenceStatus",
                        principalColumn: "DivergenceStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalTwin_TwinDivergenceReview_TwinDivergence",
                        column: x => x.TwinDivergenceId,
                        principalSchema: "DigitalTwin",
                        principalTable: "TwinDivergence",
                        principalColumn: "TwinDivergenceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_BindingRole_Code",
                schema: "DigitalTwin",
                table: "BindingRole",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_BindingStatus_Code",
                schema: "DigitalTwin",
                table: "BindingStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_DivergenceSeverity_Code",
                schema: "DigitalTwin",
                table: "DivergenceSeverity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_DivergenceStatus_Code",
                schema: "DigitalTwin",
                table: "DivergenceStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_ModelVariableType_Code",
                schema: "DigitalTwin",
                table: "ModelVariableType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalBinding_BindingRoleId",
                schema: "DigitalTwin",
                table: "SignalBinding",
                column: "BindingRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalBinding_BindingStatusId",
                schema: "DigitalTwin",
                table: "SignalBinding",
                column: "BindingStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalBinding_SignalId",
                schema: "DigitalTwin",
                table: "SignalBinding",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_SignalBinding_TwinVariableId",
                schema: "DigitalTwin",
                table: "SignalBinding",
                column: "TwinVariableId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_SignalBinding_Model_Variable_Signal",
                schema: "DigitalTwin",
                table: "SignalBinding",
                columns: new[] { "TwinModelId", "TwinVariableId", "SignalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_SnapshotReason_Code",
                schema: "DigitalTwin",
                table: "SnapshotReason",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_SolverType_Code",
                schema: "DigitalTwin",
                table: "SolverType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergence_DivergenceSeverityId",
                schema: "DigitalTwin",
                table: "TwinDivergence",
                column: "DivergenceSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergence_DivergenceStatusId",
                schema: "DigitalTwin",
                table: "TwinDivergence",
                column: "DivergenceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergence_SignalId",
                schema: "DigitalTwin",
                table: "TwinDivergence",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergence_TwinSnapshotId",
                schema: "DigitalTwin",
                table: "TwinDivergence",
                column: "TwinSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergence_TwinVariableId",
                schema: "DigitalTwin",
                table: "TwinDivergence",
                column: "TwinVariableId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergenceReview_DivergenceStatusId",
                schema: "DigitalTwin",
                table: "TwinDivergenceReview",
                column: "DivergenceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinDivergenceReview_TwinDivergenceId",
                schema: "DigitalTwin",
                table: "TwinDivergenceReview",
                column: "TwinDivergenceId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinFidelityLevel_Code",
                schema: "DigitalTwin",
                table: "TwinFidelityLevel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinModel_TwinFidelityLevelId",
                schema: "DigitalTwin",
                table: "TwinModel",
                column: "TwinFidelityLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinModel_TwinModelStatusId",
                schema: "DigitalTwin",
                table: "TwinModel",
                column: "TwinModelStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinModel_TwinModelTypeId",
                schema: "DigitalTwin",
                table: "TwinModel",
                column: "TwinModelTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinModel_Unit_Code",
                schema: "DigitalTwin",
                table: "TwinModel",
                columns: new[] { "UnitId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinModelStatus_Code",
                schema: "DigitalTwin",
                table: "TwinModelStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinModelType_Code",
                schema: "DigitalTwin",
                table: "TwinModelType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinModelVersion_SolverTypeId",
                schema: "DigitalTwin",
                table: "TwinModelVersion",
                column: "SolverTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinModelVersion_ValidationStatusId",
                schema: "DigitalTwin",
                table: "TwinModelVersion",
                column: "ValidationStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinModelVersion_Model_Version",
                schema: "DigitalTwin",
                table: "TwinModelVersion",
                columns: new[] { "TwinModelId", "VersionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinRuntimeSession_TwinModelVersionId",
                schema: "DigitalTwin",
                table: "TwinRuntimeSession",
                column: "TwinModelVersionId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinRuntimeSession_Code",
                schema: "DigitalTwin",
                table: "TwinRuntimeSession",
                column: "SessionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinSnapshot_SnapshotReasonId",
                schema: "DigitalTwin",
                table: "TwinSnapshot",
                column: "SnapshotReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinSnapshot_TwinRuntimeSessionId",
                schema: "DigitalTwin",
                table: "TwinSnapshot",
                column: "TwinRuntimeSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinSnapshotValue_TwinVariableId",
                schema: "DigitalTwin",
                table: "TwinSnapshotValue",
                column: "TwinVariableId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinSnapshotValue_Snapshot_Variable",
                schema: "DigitalTwin",
                table: "TwinSnapshotValue",
                columns: new[] { "TwinSnapshotId", "TwinVariableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwinVariable_EngineeringUnitId",
                schema: "DigitalTwin",
                table: "TwinVariable",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TwinVariable_ModelVariableTypeId",
                schema: "DigitalTwin",
                table: "TwinVariable",
                column: "ModelVariableTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_TwinVariable_Model_Code",
                schema: "DigitalTwin",
                table: "TwinVariable",
                columns: new[] { "TwinModelId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_DigitalTwin_ValidationStatus_Code",
                schema: "DigitalTwin",
                table: "ValidationStatus",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalBinding",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinDivergenceReview",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinSnapshotValue",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "BindingRole",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "BindingStatus",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinDivergence",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "DivergenceSeverity",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "DivergenceStatus",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinSnapshot",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinVariable",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "SnapshotReason",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinRuntimeSession",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "ModelVariableType",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinModelVersion",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "SolverType",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinModel",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "ValidationStatus",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinFidelityLevel",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinModelStatus",
                schema: "DigitalTwin");

            migrationBuilder.DropTable(
                name: "TwinModelType",
                schema: "DigitalTwin");
        }
    }
}
