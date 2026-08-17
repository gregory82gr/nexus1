using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Maintenance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMaintenanceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Maintenance");

            migrationBuilder.CreateTable(
                name: "AssetCategory",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetCategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetCategory", x => x.AssetCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "AssetCriticality",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetCriticalityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetCriticality", x => x.AssetCriticalityId);
                });

            migrationBuilder.CreateTable(
                name: "AssetStatus",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetStatus", x => x.AssetStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ConditionGrade",
                schema: "Maintenance",
                columns: table => new
                {
                    ConditionGradeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_ConditionGrade", x => x.ConditionGradeId);
                });

            migrationBuilder.CreateTable(
                name: "DegradationMechanism",
                schema: "Maintenance",
                columns: table => new
                {
                    DegradationMechanismId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_DegradationMechanism", x => x.DegradationMechanismId);
                });

            migrationBuilder.CreateTable(
                name: "FindingSeverity",
                schema: "Maintenance",
                columns: table => new
                {
                    FindingSeverityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_FindingSeverity", x => x.FindingSeverityId);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderStatus",
                schema: "Maintenance",
                columns: table => new
                {
                    WorkOrderStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_WorkOrderStatus", x => x.WorkOrderStatusId);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderType",
                schema: "Maintenance",
                columns: table => new
                {
                    WorkOrderTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_WorkOrderType", x => x.WorkOrderTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WorkPriority",
                schema: "Maintenance",
                columns: table => new
                {
                    WorkPriorityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_WorkPriority", x => x.WorkPriorityId);
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    EquipmentId = table.Column<int>(type: "int", nullable: true),
                    SystemId = table.Column<int>(type: "int", nullable: true),
                    AssetCategoryId = table.Column<int>(type: "int", nullable: false),
                    AssetStatusId = table.Column<int>(type: "int", nullable: false),
                    AssetCriticalityId = table.Column<int>(type: "int", nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LocationText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ModelNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CommissionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSafetyRelated = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_Asset", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Maintenance_Asset_AssetCategory",
                        column: x => x.AssetCategoryId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetCategory",
                        principalColumn: "AssetCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_Asset_AssetCriticality",
                        column: x => x.AssetCriticalityId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetCriticality",
                        principalColumn: "AssetCriticalityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_Asset_AssetStatus",
                        column: x => x.AssetStatusId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetStatus",
                        principalColumn: "AssetStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_Asset_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetComponent",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetComponentId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ParentAssetComponentId = table.Column<int>(type: "int", nullable: true),
                    ComponentCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsReplaceable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetComponent", x => x.AssetComponentId);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetComponent_Asset",
                        column: x => x.AssetId,
                        principalSchema: "Maintenance",
                        principalTable: "Asset",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetComponent_Parent",
                        column: x => x.ParentAssetComponentId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetComponent",
                        principalColumn: "AssetComponentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetCondition",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetConditionId = table.Column<long>(type: "bigint", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    ConditionGradeId = table.Column<int>(type: "int", nullable: false),
                    AssessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessedByUserId = table.Column<int>(type: "int", nullable: true),
                    HealthScorePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    RemainingUsefulLifeDays = table.Column<int>(type: "int", nullable: true),
                    Basis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetCondition", x => x.AssetConditionId);
                    table.CheckConstraint("CK_Maintenance_AssetCondition_HealthScore", "[HealthScorePercent] IS NULL OR ([HealthScorePercent] >= 0 AND [HealthScorePercent] <= 100)");
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetCondition_Asset",
                        column: x => x.AssetId,
                        principalSchema: "Maintenance",
                        principalTable: "Asset",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetCondition_ConditionGrade",
                        column: x => x.ConditionGradeId,
                        principalSchema: "Maintenance",
                        principalTable: "ConditionGrade",
                        principalColumn: "ConditionGradeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrder",
                schema: "Maintenance",
                columns: table => new
                {
                    WorkOrderId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: true),
                    MaintenancePlanId = table.Column<int>(type: "int", nullable: true),
                    MaintenanceWindowId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderTypeId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderStatusId = table.Column<int>(type: "int", nullable: false),
                    WorkPriorityId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedTeamId = table.Column<int>(type: "int", nullable: true),
                    AssignedPersonId = table.Column<int>(type: "int", nullable: true),
                    OriginOperationalEventId = table.Column<long>(type: "bigint", nullable: true),
                    OriginIncidentActionId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_WorkOrder", x => x.WorkOrderId);
                    table.ForeignKey(
                        name: "FK_Maintenance_WorkOrder_Asset",
                        column: x => x.AssetId,
                        principalSchema: "Maintenance",
                        principalTable: "Asset",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_WorkOrder_Priority",
                        column: x => x.WorkPriorityId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkPriority",
                        principalColumn: "WorkPriorityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_WorkOrder_Status",
                        column: x => x.WorkOrderStatusId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrderStatus",
                        principalColumn: "WorkOrderStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_WorkOrder_Type",
                        column: x => x.WorkOrderTypeId,
                        principalSchema: "Maintenance",
                        principalTable: "WorkOrderType",
                        principalColumn: "WorkOrderTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_WorkOrder_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DegradationRecord",
                schema: "Maintenance",
                columns: table => new
                {
                    DegradationRecordId = table.Column<long>(type: "bigint", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetComponentId = table.Column<int>(type: "int", nullable: true),
                    DegradationMechanismId = table.Column<int>(type: "int", nullable: false),
                    FindingSeverityId = table.Column<int>(type: "int", nullable: false),
                    ConditionGradeId = table.Column<int>(type: "int", nullable: true),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DetectedByUserId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EstimatedRatePerYear = table.Column<decimal>(type: "decimal(12,6)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_DegradationRecord", x => x.DegradationRecordId);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationRecord_Asset",
                        column: x => x.AssetId,
                        principalSchema: "Maintenance",
                        principalTable: "Asset",
                        principalColumn: "AssetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationRecord_AssetComponent",
                        column: x => x.AssetComponentId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetComponent",
                        principalColumn: "AssetComponentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationRecord_ConditionGrade",
                        column: x => x.ConditionGradeId,
                        principalSchema: "Maintenance",
                        principalTable: "ConditionGrade",
                        principalColumn: "ConditionGradeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationRecord_DegradationMechanism",
                        column: x => x.DegradationMechanismId,
                        principalSchema: "Maintenance",
                        principalTable: "DegradationMechanism",
                        principalColumn: "DegradationMechanismId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationRecord_FindingSeverity",
                        column: x => x.FindingSeverityId,
                        principalSchema: "Maintenance",
                        principalTable: "FindingSeverity",
                        principalColumn: "FindingSeverityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetConditionMeasurement",
                schema: "Maintenance",
                columns: table => new
                {
                    AssetConditionMeasurementId = table.Column<long>(type: "bigint", nullable: false),
                    AssetConditionId = table.Column<long>(type: "bigint", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: true),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    MeasuredValue = table.Column<double>(type: "float", nullable: false),
                    MeasuredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeasurementNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_AssetConditionMeasurement", x => x.AssetConditionMeasurementId);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetConditionMeasurement_AssetCondition",
                        column: x => x.AssetConditionId,
                        principalSchema: "Maintenance",
                        principalTable: "AssetCondition",
                        principalColumn: "AssetConditionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetConditionMeasurement_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_AssetConditionMeasurement_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DegradationTrendPoint",
                schema: "Maintenance",
                columns: table => new
                {
                    DegradationTrendPointId = table.Column<long>(type: "bigint", nullable: false),
                    DegradationRecordId = table.Column<long>(type: "bigint", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    SourceSignalId = table.Column<int>(type: "int", nullable: true),
                    MeasuredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maintenance_DegradationTrendPoint", x => x.DegradationTrendPointId);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationTrendPoint_DegradationRecord",
                        column: x => x.DegradationRecordId,
                        principalSchema: "Maintenance",
                        principalTable: "DegradationRecord",
                        principalColumn: "DegradationRecordId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationTrendPoint_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Maintenance_DegradationTrendPoint_SourceSignal",
                        column: x => x.SourceSignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetCategoryId",
                schema: "Maintenance",
                table: "Asset",
                column: "AssetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetCriticalityId",
                schema: "Maintenance",
                table: "Asset",
                column: "AssetCriticalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetStatusId",
                schema: "Maintenance",
                table: "Asset",
                column: "AssetStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_UnitId",
                schema: "Maintenance",
                table: "Asset",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_Asset_AssetCode",
                schema: "Maintenance",
                table: "Asset",
                column: "AssetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_AssetCategory_Code",
                schema: "Maintenance",
                table: "AssetCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetComponent_ParentAssetComponentId",
                schema: "Maintenance",
                table: "AssetComponent",
                column: "ParentAssetComponentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_AssetComponent_Asset_ComponentCode",
                schema: "Maintenance",
                table: "AssetComponent",
                columns: new[] { "AssetId", "ComponentCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetCondition_AssetId",
                schema: "Maintenance",
                table: "AssetCondition",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCondition_ConditionGradeId",
                schema: "Maintenance",
                table: "AssetCondition",
                column: "ConditionGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetConditionMeasurement_AssetConditionId",
                schema: "Maintenance",
                table: "AssetConditionMeasurement",
                column: "AssetConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetConditionMeasurement_EngineeringUnitId",
                schema: "Maintenance",
                table: "AssetConditionMeasurement",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetConditionMeasurement_SignalId",
                schema: "Maintenance",
                table: "AssetConditionMeasurement",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_AssetCriticality_Code",
                schema: "Maintenance",
                table: "AssetCriticality",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_AssetStatus_Code",
                schema: "Maintenance",
                table: "AssetStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_ConditionGrade_Code",
                schema: "Maintenance",
                table: "ConditionGrade",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_DegradationMechanism_Code",
                schema: "Maintenance",
                table: "DegradationMechanism",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DegradationRecord_AssetComponentId",
                schema: "Maintenance",
                table: "DegradationRecord",
                column: "AssetComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationRecord_AssetId",
                schema: "Maintenance",
                table: "DegradationRecord",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationRecord_ConditionGradeId",
                schema: "Maintenance",
                table: "DegradationRecord",
                column: "ConditionGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationRecord_DegradationMechanismId",
                schema: "Maintenance",
                table: "DegradationRecord",
                column: "DegradationMechanismId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationRecord_FindingSeverityId",
                schema: "Maintenance",
                table: "DegradationRecord",
                column: "FindingSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationTrendPoint_DegradationRecordId",
                schema: "Maintenance",
                table: "DegradationTrendPoint",
                column: "DegradationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationTrendPoint_EngineeringUnitId",
                schema: "Maintenance",
                table: "DegradationTrendPoint",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DegradationTrendPoint_SourceSignalId",
                schema: "Maintenance",
                table: "DegradationTrendPoint",
                column: "SourceSignalId");

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_FindingSeverity_Code",
                schema: "Maintenance",
                table: "FindingSeverity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_AssetId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_UnitId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_WorkOrderStatusId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "WorkOrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_WorkOrderTypeId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "WorkOrderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_WorkPriorityId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "WorkPriorityId");

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_WorkOrder_Code",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "WorkOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_WorkOrderStatus_Code",
                schema: "Maintenance",
                table: "WorkOrderStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_WorkOrderType_Code",
                schema: "Maintenance",
                table: "WorkOrderType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Maintenance_WorkPriority_Code",
                schema: "Maintenance",
                table: "WorkPriority",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetConditionMeasurement",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "DegradationTrendPoint",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrder",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "AssetCondition",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "DegradationRecord",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkPriority",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderStatus",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "WorkOrderType",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "AssetComponent",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "ConditionGrade",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "DegradationMechanism",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "FindingSeverity",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "Asset",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "AssetCategory",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "AssetCriticality",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "AssetStatus",
                schema: "Maintenance");
        }
    }
}
