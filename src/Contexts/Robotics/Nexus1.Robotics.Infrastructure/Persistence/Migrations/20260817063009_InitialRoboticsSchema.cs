using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Robotics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRoboticsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Robotics");

            migrationBuilder.CreateTable(
                name: "BatteryStatus",
                schema: "Robotics",
                columns: table => new
                {
                    BatteryStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_BatteryStatus", x => x.BatteryStatusId);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationStatus",
                schema: "Robotics",
                columns: table => new
                {
                    CommunicationStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_CommunicationStatus", x => x.CommunicationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "MissionPriority",
                schema: "Robotics",
                columns: table => new
                {
                    MissionPriorityId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_MissionPriority", x => x.MissionPriorityId);
                });

            migrationBuilder.CreateTable(
                name: "MissionStatus",
                schema: "Robotics",
                columns: table => new
                {
                    MissionStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_MissionStatus", x => x.MissionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "MissionType",
                schema: "Robotics",
                columns: table => new
                {
                    MissionTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_MissionType", x => x.MissionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ReadinessStatus",
                schema: "Robotics",
                columns: table => new
                {
                    ReadinessStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_ReadinessStatus", x => x.ReadinessStatusId);
                });

            migrationBuilder.CreateTable(
                name: "RobotStatus",
                schema: "Robotics",
                columns: table => new
                {
                    RobotStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_RobotStatus", x => x.RobotStatusId);
                });

            migrationBuilder.CreateTable(
                name: "RobotType",
                schema: "Robotics",
                columns: table => new
                {
                    RobotTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Robotics_RobotType", x => x.RobotTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Mission",
                schema: "Robotics",
                columns: table => new
                {
                    MissionId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    MissionTypeId = table.Column<int>(type: "int", nullable: false),
                    MissionStatusId = table.Column<int>(type: "int", nullable: false),
                    MissionPriorityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_Mission", x => x.MissionId);
                    table.CheckConstraint("CK_Robotics_Mission_ActualDateRange", "[ActualEndUtc] IS NULL OR [ActualStartUtc] IS NULL OR [ActualEndUtc] >= [ActualStartUtc]");
                    table.CheckConstraint("CK_Robotics_Mission_PlannedDateRange", "[PlannedEndUtc] IS NULL OR [PlannedStartUtc] IS NULL OR [PlannedEndUtc] >= [PlannedStartUtc]");
                    table.ForeignKey(
                        name: "FK_Robotics_Mission_MissionPriority",
                        column: x => x.MissionPriorityId,
                        principalSchema: "Robotics",
                        principalTable: "MissionPriority",
                        principalColumn: "MissionPriorityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_Mission_MissionStatus",
                        column: x => x.MissionStatusId,
                        principalSchema: "Robotics",
                        principalTable: "MissionStatus",
                        principalColumn: "MissionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_Mission_MissionType",
                        column: x => x.MissionTypeId,
                        principalSchema: "Robotics",
                        principalTable: "MissionType",
                        principalColumn: "MissionTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_Mission_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RobotModel",
                schema: "Robotics",
                columns: table => new
                {
                    RobotModelId = table.Column<int>(type: "int", nullable: false),
                    RobotTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaxPayloadKg = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    MaxSpeedMps = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    BatteryCapacityWh = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    NominalRuntimeMin = table.Column<int>(type: "int", nullable: true),
                    IsAutonomousCapable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_RobotModel", x => x.RobotModelId);
                    table.CheckConstraint("CK_Robotics_RobotModel_MaxPayloadKg", "[MaxPayloadKg] IS NULL OR [MaxPayloadKg] >= 0");
                    table.CheckConstraint("CK_Robotics_RobotModel_MaxSpeedMps", "[MaxSpeedMps] IS NULL OR [MaxSpeedMps] >= 0");
                    table.ForeignKey(
                        name: "FK_Robotics_RobotModel_RobotType",
                        column: x => x.RobotTypeId,
                        principalSchema: "Robotics",
                        principalTable: "RobotType",
                        principalColumn: "RobotTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionReadinessAssessment",
                schema: "Robotics",
                columns: table => new
                {
                    MissionReadinessAssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false),
                    ReadinessStatusId = table.Column<int>(type: "int", nullable: false),
                    AssessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessedByUserId = table.Column<int>(type: "int", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_MissionReadinessAssessment", x => x.MissionReadinessAssessmentId);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionReadinessAssessment_Mission",
                        column: x => x.MissionId,
                        principalSchema: "Robotics",
                        principalTable: "Mission",
                        principalColumn: "MissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionReadinessAssessment_ReadinessStatus",
                        column: x => x.ReadinessStatusId,
                        principalSchema: "Robotics",
                        principalTable: "ReadinessStatus",
                        principalColumn: "ReadinessStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Robot",
                schema: "Robotics",
                columns: table => new
                {
                    RobotId = table.Column<int>(type: "int", nullable: false),
                    RobotModelId = table.Column<int>(type: "int", nullable: false),
                    RobotStatusId = table.Column<int>(type: "int", nullable: false),
                    HomeUnitId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CommissionedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetiredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_Robot", x => x.RobotId);
                    table.ForeignKey(
                        name: "FK_Robotics_Robot_RobotModel",
                        column: x => x.RobotModelId,
                        principalSchema: "Robotics",
                        principalTable: "RobotModel",
                        principalColumn: "RobotModelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_Robot_RobotStatus",
                        column: x => x.RobotStatusId,
                        principalSchema: "Robotics",
                        principalTable: "RobotStatus",
                        principalColumn: "RobotStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_Robot_Unit",
                        column: x => x.HomeUnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionReadinessItem",
                schema: "Robotics",
                columns: table => new
                {
                    MissionReadinessItemId = table.Column<long>(type: "bigint", nullable: false),
                    MissionReadinessAssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    ReadinessStatusId = table.Column<int>(type: "int", nullable: false),
                    CheckName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsBlocking = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_MissionReadinessItem", x => x.MissionReadinessItemId);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionReadinessItem_MissionReadinessAssessment",
                        column: x => x.MissionReadinessAssessmentId,
                        principalSchema: "Robotics",
                        principalTable: "MissionReadinessAssessment",
                        principalColumn: "MissionReadinessAssessmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionReadinessItem_ReadinessStatus",
                        column: x => x.ReadinessStatusId,
                        principalSchema: "Robotics",
                        principalTable: "ReadinessStatus",
                        principalColumn: "ReadinessStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MissionEvent",
                schema: "Robotics",
                columns: table => new
                {
                    MissionEventId = table.Column<long>(type: "bigint", nullable: false),
                    MissionId = table.Column<long>(type: "bigint", nullable: false),
                    RobotId = table.Column<int>(type: "int", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsFault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RecordedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_MissionEvent", x => x.MissionEventId);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionEvent_Mission",
                        column: x => x.MissionId,
                        principalSchema: "Robotics",
                        principalTable: "Mission",
                        principalColumn: "MissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_MissionEvent_Robot",
                        column: x => x.RobotId,
                        principalSchema: "Robotics",
                        principalTable: "Robot",
                        principalColumn: "RobotId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RobotHealthSnapshot",
                schema: "Robotics",
                columns: table => new
                {
                    RobotHealthSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    RobotId = table.Column<int>(type: "int", nullable: false),
                    BatteryStatusId = table.Column<int>(type: "int", nullable: false),
                    CommunicationStatusId = table.Column<int>(type: "int", nullable: false),
                    SnapshotAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatteryPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EstimatedRuntimeMin = table.Column<int>(type: "int", nullable: true),
                    CpuLoadPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    FaultCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robotics_RobotHealthSnapshot", x => x.RobotHealthSnapshotId);
                    table.CheckConstraint("CK_Robotics_RobotHealthSnapshot_BatteryPercent", "[BatteryPercent] IS NULL OR ([BatteryPercent] BETWEEN 0 AND 100)");
                    table.CheckConstraint("CK_Robotics_RobotHealthSnapshot_CpuLoadPercent", "[CpuLoadPercent] IS NULL OR ([CpuLoadPercent] BETWEEN 0 AND 100)");
                    table.ForeignKey(
                        name: "FK_Robotics_RobotHealthSnapshot_BatteryStatus",
                        column: x => x.BatteryStatusId,
                        principalSchema: "Robotics",
                        principalTable: "BatteryStatus",
                        principalColumn: "BatteryStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_RobotHealthSnapshot_CommunicationStatus",
                        column: x => x.CommunicationStatusId,
                        principalSchema: "Robotics",
                        principalTable: "CommunicationStatus",
                        principalColumn: "CommunicationStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Robotics_RobotHealthSnapshot_Robot",
                        column: x => x.RobotId,
                        principalSchema: "Robotics",
                        principalTable: "Robot",
                        principalColumn: "RobotId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_BatteryStatus_Code",
                schema: "Robotics",
                table: "BatteryStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_CommunicationStatus_Code",
                schema: "Robotics",
                table: "CommunicationStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mission_MissionPriorityId",
                schema: "Robotics",
                table: "Mission",
                column: "MissionPriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_MissionStatusId",
                schema: "Robotics",
                table: "Mission",
                column: "MissionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_MissionTypeId",
                schema: "Robotics",
                table: "Mission",
                column: "MissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Mission_UnitId",
                schema: "Robotics",
                table: "Mission",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_Mission_Code",
                schema: "Robotics",
                table: "Mission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionEvent_MissionId",
                schema: "Robotics",
                table: "MissionEvent",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionEvent_RobotId",
                schema: "Robotics",
                table: "MissionEvent",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_MissionPriority_Code",
                schema: "Robotics",
                table: "MissionPriority",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionReadinessAssessment_MissionId",
                schema: "Robotics",
                table: "MissionReadinessAssessment",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionReadinessAssessment_ReadinessStatusId",
                schema: "Robotics",
                table: "MissionReadinessAssessment",
                column: "ReadinessStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionReadinessItem_MissionReadinessAssessmentId",
                schema: "Robotics",
                table: "MissionReadinessItem",
                column: "MissionReadinessAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MissionReadinessItem_ReadinessStatusId",
                schema: "Robotics",
                table: "MissionReadinessItem",
                column: "ReadinessStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_MissionStatus_Code",
                schema: "Robotics",
                table: "MissionStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_MissionType_Code",
                schema: "Robotics",
                table: "MissionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_ReadinessStatus_Code",
                schema: "Robotics",
                table: "ReadinessStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Robot_HomeUnitId",
                schema: "Robotics",
                table: "Robot",
                column: "HomeUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Robot_RobotModelId",
                schema: "Robotics",
                table: "Robot",
                column: "RobotModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Robot_RobotStatusId",
                schema: "Robotics",
                table: "Robot",
                column: "RobotStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_Robot_Code",
                schema: "Robotics",
                table: "Robot",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RobotHealthSnapshot_BatteryStatusId",
                schema: "Robotics",
                table: "RobotHealthSnapshot",
                column: "BatteryStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotHealthSnapshot_CommunicationStatusId",
                schema: "Robotics",
                table: "RobotHealthSnapshot",
                column: "CommunicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotHealthSnapshot_RobotId",
                schema: "Robotics",
                table: "RobotHealthSnapshot",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotModel_RobotTypeId",
                schema: "Robotics",
                table: "RobotModel",
                column: "RobotTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_RobotModel_Code",
                schema: "Robotics",
                table: "RobotModel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_RobotStatus_Code",
                schema: "Robotics",
                table: "RobotStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Robotics_RobotType_Code",
                schema: "Robotics",
                table: "RobotType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionEvent",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "MissionReadinessItem",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "RobotHealthSnapshot",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "MissionReadinessAssessment",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "BatteryStatus",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "CommunicationStatus",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "Robot",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "Mission",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "ReadinessStatus",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "RobotModel",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "RobotStatus",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "MissionPriority",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "MissionStatus",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "MissionType",
                schema: "Robotics");

            migrationBuilder.DropTable(
                name: "RobotType",
                schema: "Robotics");
        }
    }
}
