using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RadiationMonitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRadiationMonitoringSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RadiationMonitoring");

            migrationBuilder.CreateTable(
                name: "AlertStatus",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    AlertStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_AlertStatus", x => x.AlertStatusId);
                });

            migrationBuilder.CreateTable(
                name: "DoseType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DoseTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_DoseType", x => x.DoseTypeId);
                });

            migrationBuilder.CreateTable(
                name: "DosimeterStatus",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DosimeterStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_DosimeterStatus", x => x.DosimeterStatusId);
                });

            migrationBuilder.CreateTable(
                name: "DosimeterType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DosimeterTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_DosimeterType", x => x.DosimeterTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LimitType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    LimitTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_LimitType", x => x.LimitTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementQuality",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    MeasurementQualityId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_MeasurementQuality", x => x.MeasurementQualityId);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    MeasurementTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_MeasurementType", x => x.MeasurementTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MonitorStatus",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    MonitorStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_MonitorStatus", x => x.MonitorStatusId);
                });

            migrationBuilder.CreateTable(
                name: "MonitorType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    MonitorTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_MonitorType", x => x.MonitorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RadiationAreaClassification",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationAreaClassificationId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationAreaClassification", x => x.RadiationAreaClassificationId);
                });

            migrationBuilder.CreateTable(
                name: "RadiationZoneStatus",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationZoneStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationZoneStatus", x => x.RadiationZoneStatusId);
                });

            migrationBuilder.CreateTable(
                name: "RadiationZoneType",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationZoneTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationZoneType", x => x.RadiationZoneTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Dosimeter",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DosimeterId = table.Column<int>(type: "int", nullable: false),
                    DosimeterTypeId = table.Column<int>(type: "int", nullable: false),
                    DosimeterStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CalibrationDueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_Dosimeter", x => x.DosimeterId);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_Dosimeter_DosimeterStatus",
                        column: x => x.DosimeterStatusId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "DosimeterStatus",
                        principalColumn: "DosimeterStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_Dosimeter_DosimeterType",
                        column: x => x.DosimeterTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "DosimeterType",
                        principalColumn: "DosimeterTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoseLimit",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DoseLimitId = table.Column<int>(type: "int", nullable: false),
                    DoseTypeId = table.Column<int>(type: "int", nullable: false),
                    LimitTypeId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LimitValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    PeriodDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_DoseLimit", x => x.DoseLimitId);
                    table.CheckConstraint("CK_RadiationMonitoring_DoseLimit_LimitValue", "[LimitValue] >= 0");
                    table.CheckConstraint("CK_RadiationMonitoring_DoseLimit_PeriodDays", "[PeriodDays] > 0");
                    table.ForeignKey(
                        name: "FK_DoseLimit_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_DoseLimit_DoseType",
                        column: x => x.DoseTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "DoseType",
                        principalColumn: "DoseTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_DoseLimit_LimitType",
                        column: x => x.LimitTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "LimitType",
                        principalColumn: "LimitTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadiationZone",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationZoneId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    EquipmentLocationId = table.Column<int>(type: "int", nullable: true),
                    RadiationZoneTypeId = table.Column<int>(type: "int", nullable: false),
                    RadiationZoneStatusId = table.Column<int>(type: "int", nullable: false),
                    RadiationAreaClassificationId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsEntryControlled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RequiresDosimeter = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PostedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationZone", x => x.RadiationZoneId);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationZone_RadiationAreaClassification",
                        column: x => x.RadiationAreaClassificationId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationAreaClassification",
                        principalColumn: "RadiationAreaClassificationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationZone_RadiationZoneStatus",
                        column: x => x.RadiationZoneStatusId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationZoneStatus",
                        principalColumn: "RadiationZoneStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationZone_RadiationZoneType",
                        column: x => x.RadiationZoneTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationZoneType",
                        principalColumn: "RadiationZoneTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationZone_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonDosimeterAssignment",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    PersonDosimeterAssignmentId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    DosimeterId = table.Column<int>(type: "int", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignmentPurpose = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_PersonDosimeterAssignment", x => x.PersonDosimeterAssignmentId);
                    table.CheckConstraint("CK_RadiationMonitoring_PersonDosimeterAssignment_ReturnedAfterAssigned", "[ReturnedAtUtc] IS NULL OR [ReturnedAtUtc] > [AssignedAtUtc]");
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_PersonDosimeterAssignment_Dosimeter",
                        column: x => x.DosimeterId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "Dosimeter",
                        principalColumn: "DosimeterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadiationMonitor",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationMonitorId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    EquipmentId = table.Column<int>(type: "int", nullable: true),
                    RadiationZoneId = table.Column<int>(type: "int", nullable: true),
                    MonitorTypeId = table.Column<int>(type: "int", nullable: false),
                    MonitorStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    InstalledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CalibrationDueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationMonitor", x => x.RadiationMonitorId);
                    table.ForeignKey(
                        name: "FK_RadiationMonitor_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationMonitor_MonitorStatus",
                        column: x => x.MonitorStatusId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "MonitorStatus",
                        principalColumn: "MonitorStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationMonitor_MonitorType",
                        column: x => x.MonitorTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "MonitorType",
                        principalColumn: "MonitorTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationMonitor_RadiationZone",
                        column: x => x.RadiationZoneId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationZone",
                        principalColumn: "RadiationZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonDoseReading",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    PersonDoseReadingId = table.Column<long>(type: "bigint", nullable: false),
                    PersonDosimeterAssignmentId = table.Column<int>(type: "int", nullable: false),
                    DoseTypeId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    MeasurementQualityId = table.Column<int>(type: "int", nullable: false),
                    ReadingAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoseValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_PersonDoseReading", x => x.PersonDoseReadingId);
                    table.CheckConstraint("CK_RadiationMonitoring_PersonDoseReading_DoseValue", "[DoseValue] >= 0");
                    table.ForeignKey(
                        name: "FK_PersonDoseReading_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_PersonDoseReading_DoseType",
                        column: x => x.DoseTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "DoseType",
                        principalColumn: "DoseTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_PersonDoseReading_MeasurementQuality",
                        column: x => x.MeasurementQualityId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "MeasurementQuality",
                        principalColumn: "MeasurementQualityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_PersonDoseReading_PersonDosimeterAssignment",
                        column: x => x.PersonDosimeterAssignmentId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "PersonDosimeterAssignment",
                        principalColumn: "PersonDosimeterAssignmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadiationReading",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    RadiationReadingId = table.Column<long>(type: "bigint", nullable: false),
                    RadiationMonitorId = table.Column<int>(type: "int", nullable: false),
                    MeasurementTypeId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    MeasurementQualityId = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsAlarmRelevant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SourceTimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_RadiationReading", x => x.RadiationReadingId);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationReading_MeasurementQuality",
                        column: x => x.MeasurementQualityId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "MeasurementQuality",
                        principalColumn: "MeasurementQualityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationReading_MeasurementType",
                        column: x => x.MeasurementTypeId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "MeasurementType",
                        principalColumn: "MeasurementTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_RadiationReading_RadiationMonitor",
                        column: x => x.RadiationMonitorId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "RadiationMonitor",
                        principalColumn: "RadiationMonitorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationReading_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoseAlert",
                schema: "RadiationMonitoring",
                columns: table => new
                {
                    DoseAlertId = table.Column<long>(type: "bigint", nullable: false),
                    DoseLimitId = table.Column<int>(type: "int", nullable: false),
                    PersonDoseReadingId = table.Column<long>(type: "bigint", nullable: true),
                    AlertStatusId = table.Column<int>(type: "int", nullable: false),
                    AcknowledgedByUserId = table.Column<int>(type: "int", nullable: true),
                    AlertAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiationMonitoring_DoseAlert", x => x.DoseAlertId);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_DoseAlert_AlertStatus",
                        column: x => x.AlertStatusId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "AlertStatus",
                        principalColumn: "AlertStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_DoseAlert_DoseLimit",
                        column: x => x.DoseLimitId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "DoseLimit",
                        principalColumn: "DoseLimitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiationMonitoring_DoseAlert_PersonDoseReading",
                        column: x => x.PersonDoseReadingId,
                        principalSchema: "RadiationMonitoring",
                        principalTable: "PersonDoseReading",
                        principalColumn: "PersonDoseReadingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_AlertStatus_Code",
                schema: "RadiationMonitoring",
                table: "AlertStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoseAlert_AlertStatusId",
                schema: "RadiationMonitoring",
                table: "DoseAlert",
                column: "AlertStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseAlert_DoseLimitId",
                schema: "RadiationMonitoring",
                table: "DoseAlert",
                column: "DoseLimitId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseAlert_PersonDoseReadingId",
                schema: "RadiationMonitoring",
                table: "DoseAlert",
                column: "PersonDoseReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseLimit_DoseTypeId",
                schema: "RadiationMonitoring",
                table: "DoseLimit",
                column: "DoseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseLimit_EngineeringUnitId",
                schema: "RadiationMonitoring",
                table: "DoseLimit",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DoseLimit_LimitTypeId",
                schema: "RadiationMonitoring",
                table: "DoseLimit",
                column: "LimitTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_DoseLimit_Code",
                schema: "RadiationMonitoring",
                table: "DoseLimit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_DoseType_Code",
                schema: "RadiationMonitoring",
                table: "DoseType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dosimeter_DosimeterStatusId",
                schema: "RadiationMonitoring",
                table: "Dosimeter",
                column: "DosimeterStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Dosimeter_DosimeterTypeId",
                schema: "RadiationMonitoring",
                table: "Dosimeter",
                column: "DosimeterTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_Dosimeter_Code",
                schema: "RadiationMonitoring",
                table: "Dosimeter",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_DosimeterStatus_Code",
                schema: "RadiationMonitoring",
                table: "DosimeterStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_DosimeterType_Code",
                schema: "RadiationMonitoring",
                table: "DosimeterType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_LimitType_Code",
                schema: "RadiationMonitoring",
                table: "LimitType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_MeasurementQuality_Code",
                schema: "RadiationMonitoring",
                table: "MeasurementQuality",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_MeasurementType_Code",
                schema: "RadiationMonitoring",
                table: "MeasurementType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_MonitorStatus_Code",
                schema: "RadiationMonitoring",
                table: "MonitorStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_MonitorType_Code",
                schema: "RadiationMonitoring",
                table: "MonitorType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonDoseReading_DoseTypeId",
                schema: "RadiationMonitoring",
                table: "PersonDoseReading",
                column: "DoseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDoseReading_EngineeringUnitId",
                schema: "RadiationMonitoring",
                table: "PersonDoseReading",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDoseReading_MeasurementQualityId",
                schema: "RadiationMonitoring",
                table: "PersonDoseReading",
                column: "MeasurementQualityId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDoseReading_PersonDosimeterAssignmentId",
                schema: "RadiationMonitoring",
                table: "PersonDoseReading",
                column: "PersonDosimeterAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonDosimeterAssignment_DosimeterId",
                schema: "RadiationMonitoring",
                table: "PersonDosimeterAssignment",
                column: "DosimeterId");

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_RadiationAreaClassification_Code",
                schema: "RadiationMonitoring",
                table: "RadiationAreaClassification",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadiationMonitor_MonitorStatusId",
                schema: "RadiationMonitoring",
                table: "RadiationMonitor",
                column: "MonitorStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationMonitor_MonitorTypeId",
                schema: "RadiationMonitoring",
                table: "RadiationMonitor",
                column: "MonitorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationMonitor_RadiationZoneId",
                schema: "RadiationMonitoring",
                table: "RadiationMonitor",
                column: "RadiationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationMonitor_UnitId",
                schema: "RadiationMonitoring",
                table: "RadiationMonitor",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_RadiationMonitor_Code",
                schema: "RadiationMonitoring",
                table: "RadiationMonitor",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadiationReading_EngineeringUnitId",
                schema: "RadiationMonitoring",
                table: "RadiationReading",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationReading_MeasurementQualityId",
                schema: "RadiationMonitoring",
                table: "RadiationReading",
                column: "MeasurementQualityId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationReading_MeasurementTypeId",
                schema: "RadiationMonitoring",
                table: "RadiationReading",
                column: "MeasurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationReading_RadiationMonitorId",
                schema: "RadiationMonitoring",
                table: "RadiationReading",
                column: "RadiationMonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationZone_RadiationAreaClassificationId",
                schema: "RadiationMonitoring",
                table: "RadiationZone",
                column: "RadiationAreaClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationZone_RadiationZoneStatusId",
                schema: "RadiationMonitoring",
                table: "RadiationZone",
                column: "RadiationZoneStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationZone_RadiationZoneTypeId",
                schema: "RadiationMonitoring",
                table: "RadiationZone",
                column: "RadiationZoneTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiationZone_UnitId",
                schema: "RadiationMonitoring",
                table: "RadiationZone",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_RadiationZone_Code",
                schema: "RadiationMonitoring",
                table: "RadiationZone",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_RadiationZoneStatus_Code",
                schema: "RadiationMonitoring",
                table: "RadiationZoneStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RadiationMonitoring_RadiationZoneType_Code",
                schema: "RadiationMonitoring",
                table: "RadiationZoneType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoseAlert",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationReading",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "AlertStatus",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "DoseLimit",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "PersonDoseReading",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "MeasurementType",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationMonitor",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "LimitType",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "DoseType",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "MeasurementQuality",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "PersonDosimeterAssignment",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "MonitorStatus",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "MonitorType",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationZone",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "Dosimeter",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationAreaClassification",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationZoneStatus",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "RadiationZoneType",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "DosimeterStatus",
                schema: "RadiationMonitoring");

            migrationBuilder.DropTable(
                name: "DosimeterType",
                schema: "RadiationMonitoring");
        }
    }
}
