using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.EmergencyPreparedness.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEmergencyPreparednessSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EmergencyPreparedness");

            migrationBuilder.CreateTable(
                name: "AssemblyPoint",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    AssemblyPointId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    RadiationZoneId = table.Column<int>(type: "int", nullable: true),
                    MaxOccupancy = table.Column<int>(type: "int", nullable: true),
                    IsIndoor = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_AssemblyPoint", x => x.AssemblyPointId);
                    table.CheckConstraint("CK_EmergencyPreparedness_AssemblyPoint_MaxOccupancy", "[MaxOccupancy] IS NULL OR [MaxOccupancy] >= 0");
                    table.ForeignKey(
                        name: "FK_AssemblyPoint_RadiationZone",
                        column: x => x.RadiationZoneId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationZone",
                        principalColumn: "RadiationZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseStatus",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ExerciseStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ExerciseStatus", x => x.ExerciseStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseType",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ExerciseTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ExerciseType", x => x.ExerciseTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ObservationSeverity",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ObservationSeverityId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ObservationSeverity", x => x.ObservationSeverityId);
                });

            migrationBuilder.CreateTable(
                name: "PlanStatus",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    PlanStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_PlanStatus", x => x.PlanStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ReadinessStatus",
                schema: "EmergencyPreparedness",
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ReadinessStatus", x => x.ReadinessStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceStatus",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ResourceStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ResourceStatus", x => x.ResourceStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceType",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ResourceTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_ResourceType", x => x.ResourceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RouteStatus",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    RouteStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_RouteStatus", x => x.RouteStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Exercise",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExerciseTypeId = table.Column<int>(type: "int", nullable: false),
                    ExerciseStatusId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    ScheduledStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CoordinatorUserId = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_Exercise", x => x.ExerciseId);
                    table.CheckConstraint("CK_EmergencyPreparedness_Exercise_ActualDateRange", "[ActualEndUtc] IS NULL OR [ActualStartUtc] IS NULL OR [ActualEndUtc] >= [ActualStartUtc]");
                    table.CheckConstraint("CK_EmergencyPreparedness_Exercise_ScheduledDateRange", "[ScheduledEndUtc] >= [ScheduledStartUtc]");
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_Exercise_ExerciseStatus",
                        column: x => x.ExerciseStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ExerciseStatus",
                        principalColumn: "ExerciseStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_Exercise_ExerciseType",
                        column: x => x.ExerciseTypeId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ExerciseType",
                        principalColumn: "ExerciseTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyPlan",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    EmergencyPlanId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlanStatusId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    CurrentRevisionNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_EmergencyPreparedness_EmergencyPlan", x => x.EmergencyPlanId);
                    table.CheckConstraint("CK_EmergencyPreparedness_EmergencyPlan_EffectiveDateRange", "[EffectiveToUtc] IS NULL OR [EffectiveFromUtc] IS NULL OR [EffectiveToUtc] >= [EffectiveFromUtc]");
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EmergencyPlan_PlanStatus",
                        column: x => x.PlanStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "PlanStatus",
                        principalColumn: "PlanStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyResource",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    EmergencyResourceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResourceTypeId = table.Column<int>(type: "int", nullable: false),
                    ResourceStatusId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    OwnerTeamId = table.Column<int>(type: "int", nullable: true),
                    QuantityOnHand = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: true),
                    LocationText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_EmergencyResource", x => x.EmergencyResourceId);
                    table.CheckConstraint("CK_EmergencyPreparedness_EmergencyResource_QuantityOnHand", "[QuantityOnHand] IS NULL OR [QuantityOnHand] >= 0");
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EmergencyResource_ResourceStatus",
                        column: x => x.ResourceStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ResourceStatus",
                        principalColumn: "ResourceStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EmergencyResource_ResourceType",
                        column: x => x.ResourceTypeId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ResourceType",
                        principalColumn: "ResourceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyResource_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvacuationRoute",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    EvacuationRouteId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    AssemblyPointId = table.Column<int>(type: "int", nullable: false),
                    RouteStatusId = table.Column<int>(type: "int", nullable: false),
                    FromLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    RouteGeometryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_EvacuationRoute", x => x.EvacuationRouteId);
                    table.CheckConstraint("CK_EmergencyPreparedness_EvacuationRoute_EstimatedMinutes", "[EstimatedMinutes] IS NULL OR [EstimatedMinutes] >= 0");
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EvacuationRoute_AssemblyPoint",
                        column: x => x.AssemblyPointId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "AssemblyPoint",
                        principalColumn: "AssemblyPointId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EvacuationRoute_RouteStatus",
                        column: x => x.RouteStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "RouteStatus",
                        principalColumn: "RouteStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseObservation",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ExerciseObservationId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    ObservationSeverityId = table.Column<int>(type: "int", nullable: false),
                    ObservedByUserId = table.Column<int>(type: "int", nullable: false),
                    ObservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FindingText = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    CorrectiveActionRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_ExerciseObservation", x => x.ExerciseObservationId);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_ExerciseObservation_Exercise",
                        column: x => x.ExerciseId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "Exercise",
                        principalColumn: "ExerciseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_ExerciseObservation_ObservationSeverity",
                        column: x => x.ObservationSeverityId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ObservationSeverity",
                        principalColumn: "ObservationSeverityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyPlanRevision",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    EmergencyPlanRevisionId = table.Column<int>(type: "int", nullable: false),
                    EmergencyPlanId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlanStatusId = table.Column<int>(type: "int", nullable: false),
                    PreparedByUserId = table.Column<int>(type: "int", nullable: false),
                    PreparedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_EmergencyPlanRevision", x => x.EmergencyPlanRevisionId);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EmergencyPlanRevision_EmergencyPlan",
                        column: x => x.EmergencyPlanId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "EmergencyPlan",
                        principalColumn: "EmergencyPlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EmergencyPlanRevision_PlanStatus",
                        column: x => x.PlanStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "PlanStatus",
                        principalColumn: "PlanStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResourceReadinessCheck",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    ResourceReadinessCheckId = table.Column<long>(type: "bigint", nullable: false),
                    EmergencyResourceId = table.Column<int>(type: "int", nullable: false),
                    ReadinessStatusId = table.Column<int>(type: "int", nullable: false),
                    CheckedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckedByUserId = table.Column<int>(type: "int", nullable: false),
                    ConditionSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NextCheckDueUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_ResourceReadinessCheck", x => x.ResourceReadinessCheckId);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_ResourceReadinessCheck_EmergencyResource",
                        column: x => x.EmergencyResourceId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "EmergencyResource",
                        principalColumn: "EmergencyResourceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_ResourceReadinessCheck_ReadinessStatus",
                        column: x => x.ReadinessStatusId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "ReadinessStatus",
                        principalColumn: "ReadinessStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EvacuationRouteZone",
                schema: "EmergencyPreparedness",
                columns: table => new
                {
                    EvacuationRouteZoneId = table.Column<int>(type: "int", nullable: false),
                    EvacuationRouteId = table.Column<int>(type: "int", nullable: false),
                    RadiationZoneId = table.Column<int>(type: "int", nullable: false),
                    IsAvoidIfAlarmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyPreparedness_EvacuationRouteZone", x => x.EvacuationRouteZoneId);
                    table.ForeignKey(
                        name: "FK_EmergencyPreparedness_EvacuationRouteZone_EvacuationRoute",
                        column: x => x.EvacuationRouteId,
                        principalSchema: "EmergencyPreparedness",
                        principalTable: "EvacuationRoute",
                        principalColumn: "EvacuationRouteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvacuationRouteZone_RadiationZone",
                        column: x => x.RadiationZoneId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationZone",
                        principalColumn: "RadiationZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyPoint_RadiationZoneId",
                schema: "EmergencyPreparedness",
                table: "AssemblyPoint",
                column: "RadiationZoneId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_AssemblyPoint_Code",
                schema: "EmergencyPreparedness",
                table: "AssemblyPoint",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyPlan_PlanStatusId",
                schema: "EmergencyPreparedness",
                table: "EmergencyPlan",
                column: "PlanStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_EmergencyPlan_Code",
                schema: "EmergencyPreparedness",
                table: "EmergencyPlan",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyPlanRevision_PlanStatusId",
                schema: "EmergencyPreparedness",
                table: "EmergencyPlanRevision",
                column: "PlanStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_EmergencyPlanRevision_Plan_RevisionNumber",
                schema: "EmergencyPreparedness",
                table: "EmergencyPlanRevision",
                columns: new[] { "EmergencyPlanId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyResource_EngineeringUnitId",
                schema: "EmergencyPreparedness",
                table: "EmergencyResource",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyResource_ResourceStatusId",
                schema: "EmergencyPreparedness",
                table: "EmergencyResource",
                column: "ResourceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyResource_ResourceTypeId",
                schema: "EmergencyPreparedness",
                table: "EmergencyResource",
                column: "ResourceTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_EmergencyResource_Code",
                schema: "EmergencyPreparedness",
                table: "EmergencyResource",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRoute_AssemblyPointId",
                schema: "EmergencyPreparedness",
                table: "EvacuationRoute",
                column: "AssemblyPointId");

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRoute_RouteStatusId",
                schema: "EmergencyPreparedness",
                table: "EvacuationRoute",
                column: "RouteStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_EvacuationRoute_Code",
                schema: "EmergencyPreparedness",
                table: "EvacuationRoute",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRouteZone_RadiationZoneId",
                schema: "EmergencyPreparedness",
                table: "EvacuationRouteZone",
                column: "RadiationZoneId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_EvacuationRouteZone_Route_Zone",
                schema: "EmergencyPreparedness",
                table: "EvacuationRouteZone",
                columns: new[] { "EvacuationRouteId", "RadiationZoneId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_ExerciseStatusId",
                schema: "EmergencyPreparedness",
                table: "Exercise",
                column: "ExerciseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_ExerciseTypeId",
                schema: "EmergencyPreparedness",
                table: "Exercise",
                column: "ExerciseTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_Exercise_Code",
                schema: "EmergencyPreparedness",
                table: "Exercise",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseObservation_ExerciseId",
                schema: "EmergencyPreparedness",
                table: "ExerciseObservation",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseObservation_ObservationSeverityId",
                schema: "EmergencyPreparedness",
                table: "ExerciseObservation",
                column: "ObservationSeverityId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ExerciseStatus_Code",
                schema: "EmergencyPreparedness",
                table: "ExerciseStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ExerciseType_Code",
                schema: "EmergencyPreparedness",
                table: "ExerciseType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ObservationSeverity_Code",
                schema: "EmergencyPreparedness",
                table: "ObservationSeverity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_PlanStatus_Code",
                schema: "EmergencyPreparedness",
                table: "PlanStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ReadinessStatus_Code",
                schema: "EmergencyPreparedness",
                table: "ReadinessStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceReadinessCheck_EmergencyResourceId",
                schema: "EmergencyPreparedness",
                table: "ResourceReadinessCheck",
                column: "EmergencyResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceReadinessCheck_ReadinessStatusId",
                schema: "EmergencyPreparedness",
                table: "ResourceReadinessCheck",
                column: "ReadinessStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ResourceStatus_Code",
                schema: "EmergencyPreparedness",
                table: "ResourceStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_ResourceType_Code",
                schema: "EmergencyPreparedness",
                table: "ResourceType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EmergencyPreparedness_RouteStatus_Code",
                schema: "EmergencyPreparedness",
                table: "RouteStatus",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyPlanRevision",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "EvacuationRouteZone",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ExerciseObservation",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ResourceReadinessCheck",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "EmergencyPlan",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "EvacuationRoute",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "Exercise",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ObservationSeverity",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "EmergencyResource",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ReadinessStatus",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "PlanStatus",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "AssemblyPoint",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "RouteStatus",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ExerciseStatus",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ExerciseType",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ResourceStatus",
                schema: "EmergencyPreparedness");

            migrationBuilder.DropTable(
                name: "ResourceType",
                schema: "EmergencyPreparedness");
        }
    }
}
