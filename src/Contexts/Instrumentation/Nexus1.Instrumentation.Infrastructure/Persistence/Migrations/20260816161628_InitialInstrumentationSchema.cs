using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Instrumentation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialInstrumentationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Instrumentation");

            migrationBuilder.CreateTable(
                name: "ChannelStatus",
                schema: "Instrumentation",
                columns: table => new
                {
                    ChannelStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_ChannelStatus", x => x.ChannelStatusId);
                });

            migrationBuilder.CreateTable(
                name: "HistorianRetentionClass",
                schema: "Instrumentation",
                columns: table => new
                {
                    HistorianRetentionClassId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_HistorianRetentionClass", x => x.HistorianRetentionClassId);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementSource",
                schema: "Instrumentation",
                columns: table => new
                {
                    MeasurementSourceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_MeasurementSource", x => x.MeasurementSourceId);
                });

            migrationBuilder.CreateTable(
                name: "SamplingMode",
                schema: "Instrumentation",
                columns: table => new
                {
                    SamplingModeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SamplingMode", x => x.SamplingModeId);
                });

            migrationBuilder.CreateTable(
                name: "SignalCategory",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalCategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalCategory", x => x.SignalCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "SignalQuality",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalQualityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalQuality", x => x.SignalQualityId);
                });

            migrationBuilder.CreateTable(
                name: "SignalRole",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalRoleId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalRole", x => x.SignalRoleId);
                });

            migrationBuilder.CreateTable(
                name: "SignalType",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalType", x => x.SignalTypeId);
                });

            migrationBuilder.CreateTable(
                name: "DataAcquisitionNode",
                schema: "Instrumentation",
                columns: table => new
                {
                    DataAcquisitionNodeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ChannelStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    HostName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    NetworkZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_DataAcquisitionNode", x => x.DataAcquisitionNodeId);
                    table.ForeignKey(
                        name: "FK_Instrumentation_DataAcquisitionNode_ChannelStatus",
                        column: x => x.ChannelStatusId,
                        principalSchema: "Instrumentation",
                        principalTable: "ChannelStatus",
                        principalColumn: "ChannelStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_DataAcquisitionNode_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Signal",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    EquipmentId = table.Column<int>(type: "int", nullable: true),
                    PlantSystemId = table.Column<int>(type: "int", nullable: true),
                    SignalTypeId = table.Column<int>(type: "int", nullable: false),
                    SignalCategoryId = table.Column<int>(type: "int", nullable: false),
                    SignalRoleId = table.Column<int>(type: "int", nullable: false),
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    SamplingModeId = table.Column<int>(type: "int", nullable: false),
                    HistorianRetentionClassId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScanRateHz = table.Column<decimal>(type: "decimal(12,6)", nullable: true),
                    PrecisionDigits = table.Column<int>(type: "int", nullable: true),
                    NormalMin = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    NormalMax = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsSafetyRelated = table.Column<bool>(type: "bit", nullable: false),
                    IsHistorized = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_Signal", x => x.SignalId);
                    table.CheckConstraint("CK_Instrumentation_Signal_NormalRange", "[NormalMin] IS NULL OR [NormalMax] IS NULL OR [NormalMax] > [NormalMin]");
                    table.CheckConstraint("CK_Instrumentation_Signal_ScanRate", "[ScanRateHz] IS NULL OR [ScanRateHz] > 0");
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_EngineeringUnit",
                        column: x => x.EngineeringUnitId,
                        principalSchema: "CorePlatform",
                        principalTable: "EngineeringUnit",
                        principalColumn: "EngineeringUnitId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_HistorianRetentionClass",
                        column: x => x.HistorianRetentionClassId,
                        principalSchema: "Instrumentation",
                        principalTable: "HistorianRetentionClass",
                        principalColumn: "HistorianRetentionClassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_SamplingMode",
                        column: x => x.SamplingModeId,
                        principalSchema: "Instrumentation",
                        principalTable: "SamplingMode",
                        principalColumn: "SamplingModeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_SignalCategory",
                        column: x => x.SignalCategoryId,
                        principalSchema: "Instrumentation",
                        principalTable: "SignalCategory",
                        principalColumn: "SignalCategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_SignalRole",
                        column: x => x.SignalRoleId,
                        principalSchema: "Instrumentation",
                        principalTable: "SignalRole",
                        principalColumn: "SignalRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_SignalType",
                        column: x => x.SignalTypeId,
                        principalSchema: "Instrumentation",
                        principalTable: "SignalType",
                        principalColumn: "SignalTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Signal_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcquisitionConnection",
                schema: "Instrumentation",
                columns: table => new
                {
                    AcquisitionConnectionId = table.Column<int>(type: "int", nullable: false),
                    DataAcquisitionNodeId = table.Column<int>(type: "int", nullable: false),
                    ChannelStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PollIntervalMs = table.Column<int>(type: "int", nullable: true),
                    IsReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_AcquisitionConnection", x => x.AcquisitionConnectionId);
                    table.CheckConstraint("CK_Instrumentation_AcquisitionConnection_PollInterval", "[PollIntervalMs] IS NULL OR [PollIntervalMs] > 0");
                    table.ForeignKey(
                        name: "FK_Instrumentation_AcquisitionConnection_ChannelStatus",
                        column: x => x.ChannelStatusId,
                        principalSchema: "Instrumentation",
                        principalTable: "ChannelStatus",
                        principalColumn: "ChannelStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_AcquisitionConnection_DataAcquisitionNode",
                        column: x => x.DataAcquisitionNodeId,
                        principalSchema: "Instrumentation",
                        principalTable: "DataAcquisitionNode",
                        principalColumn: "DataAcquisitionNodeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Measurement",
                schema: "Instrumentation",
                columns: table => new
                {
                    MeasurementId = table.Column<long>(type: "bigint", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    SignalQualityId = table.Column<int>(type: "int", nullable: false),
                    MeasurementSourceId = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    IsEstimated = table.Column<bool>(type: "bit", nullable: false),
                    InsertedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_Measurement", x => x.MeasurementId)
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_Instrumentation_Measurement_OneValue", "[NumericValue] IS NOT NULL OR [TextValue] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Instrumentation_Measurement_MeasurementSource",
                        column: x => x.MeasurementSourceId,
                        principalSchema: "Instrumentation",
                        principalTable: "MeasurementSource",
                        principalColumn: "MeasurementSourceId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Measurement_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_Measurement_SignalQuality",
                        column: x => x.SignalQualityId,
                        principalSchema: "Instrumentation",
                        principalTable: "SignalQuality",
                        principalColumn: "SignalQualityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SignalQualityEvent",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalQualityEventId = table.Column<long>(type: "bigint", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    SignalQualityId = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReasonCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalQualityEvent", x => x.SignalQualityEventId);
                    table.CheckConstraint("CK_Instrumentation_SignalQualityEvent_Time", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]");
                    table.ForeignKey(
                        name: "FK_Instrumentation_SignalQualityEvent_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_SignalQualityEvent_SignalQuality",
                        column: x => x.SignalQualityId,
                        principalSchema: "Instrumentation",
                        principalTable: "SignalQuality",
                        principalColumn: "SignalQualityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcquisitionPoint",
                schema: "Instrumentation",
                columns: table => new
                {
                    AcquisitionPointId = table.Column<int>(type: "int", nullable: false),
                    AcquisitionConnectionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RawAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RawDataType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ScaleFactor = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    OffsetValue = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_AcquisitionPoint", x => x.AcquisitionPointId);
                    table.ForeignKey(
                        name: "FK_Instrumentation_AcquisitionPoint_AcquisitionConnection",
                        column: x => x.AcquisitionConnectionId,
                        principalSchema: "Instrumentation",
                        principalTable: "AcquisitionConnection",
                        principalColumn: "AcquisitionConnectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SignalMapping",
                schema: "Instrumentation",
                columns: table => new
                {
                    SignalMappingId = table.Column<int>(type: "int", nullable: false),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    AcquisitionPointId = table.Column<int>(type: "int", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumentation_SignalMapping", x => x.SignalMappingId);
                    table.CheckConstraint("CK_Instrumentation_SignalMapping_Effective", "[EffectiveToUtc] IS NULL OR [EffectiveToUtc] > [EffectiveFromUtc]");
                    table.ForeignKey(
                        name: "FK_Instrumentation_SignalMapping_AcquisitionPoint",
                        column: x => x.AcquisitionPointId,
                        principalSchema: "Instrumentation",
                        principalTable: "AcquisitionPoint",
                        principalColumn: "AcquisitionPointId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Instrumentation_SignalMapping_Signal",
                        column: x => x.SignalId,
                        principalSchema: "Instrumentation",
                        principalTable: "Signal",
                        principalColumn: "SignalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcquisitionConnection_ChannelStatusId",
                schema: "Instrumentation",
                table: "AcquisitionConnection",
                column: "ChannelStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_AcquisitionConnection_Node_Code",
                schema: "Instrumentation",
                table: "AcquisitionConnection",
                columns: new[] { "DataAcquisitionNodeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_AcquisitionPoint_Connection_Code",
                schema: "Instrumentation",
                table: "AcquisitionPoint",
                columns: new[] { "AcquisitionConnectionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_ChannelStatus_Code",
                schema: "Instrumentation",
                table: "ChannelStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataAcquisitionNode_ChannelStatusId",
                schema: "Instrumentation",
                table: "DataAcquisitionNode",
                column: "ChannelStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_DataAcquisitionNode_Unit_Code",
                schema: "Instrumentation",
                table: "DataAcquisitionNode",
                columns: new[] { "UnitId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_HistorianRetentionClass_Code",
                schema: "Instrumentation",
                table: "HistorianRetentionClass",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrumentation_Measurement_SignalId_TimestampUtc",
                schema: "Instrumentation",
                table: "Measurement",
                columns: new[] { "SignalId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Measurement_MeasurementSourceId",
                schema: "Instrumentation",
                table: "Measurement",
                column: "MeasurementSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurement_SignalQualityId",
                schema: "Instrumentation",
                table: "Measurement",
                column: "SignalQualityId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_MeasurementSource_Code",
                schema: "Instrumentation",
                table: "MeasurementSource",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SamplingMode_Code",
                schema: "Instrumentation",
                table: "SamplingMode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signal_EngineeringUnitId",
                schema: "Instrumentation",
                table: "Signal",
                column: "EngineeringUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_HistorianRetentionClassId",
                schema: "Instrumentation",
                table: "Signal",
                column: "HistorianRetentionClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_SamplingModeId",
                schema: "Instrumentation",
                table: "Signal",
                column: "SamplingModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_SignalCategoryId",
                schema: "Instrumentation",
                table: "Signal",
                column: "SignalCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_SignalRoleId",
                schema: "Instrumentation",
                table: "Signal",
                column: "SignalRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_SignalTypeId",
                schema: "Instrumentation",
                table: "Signal",
                column: "SignalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_UnitId",
                schema: "Instrumentation",
                table: "Signal",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_Signal_Tag",
                schema: "Instrumentation",
                table: "Signal",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SignalCategory_Code",
                schema: "Instrumentation",
                table: "SignalCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignalMapping_AcquisitionPointId",
                schema: "Instrumentation",
                table: "SignalMapping",
                column: "AcquisitionPointId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SignalMapping_Signal_Point_From",
                schema: "Instrumentation",
                table: "SignalMapping",
                columns: new[] { "SignalId", "AcquisitionPointId", "EffectiveFromUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SignalQuality_Code",
                schema: "Instrumentation",
                table: "SignalQuality",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrumentation_SignalQualityEvent_SignalId_EndedAtUtc",
                schema: "Instrumentation",
                table: "SignalQualityEvent",
                columns: new[] { "SignalId", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SignalQualityEvent_SignalQualityId",
                schema: "Instrumentation",
                table: "SignalQualityEvent",
                column: "SignalQualityId");

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SignalRole_Code",
                schema: "Instrumentation",
                table: "SignalRole",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Instrumentation_SignalType_Code",
                schema: "Instrumentation",
                table: "SignalType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measurement",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalMapping",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalQualityEvent",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "MeasurementSource",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "AcquisitionPoint",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "Signal",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalQuality",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "AcquisitionConnection",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "HistorianRetentionClass",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SamplingMode",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalCategory",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalRole",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "SignalType",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "DataAcquisitionNode",
                schema: "Instrumentation");

            migrationBuilder.DropTable(
                name: "ChannelStatus",
                schema: "Instrumentation");
        }
    }
}
