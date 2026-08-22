using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.EventManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEventManagementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "EventManagement");

            migrationBuilder.CreateTable(
                name: "EventSeverity",
                schema: "EventManagement",
                columns: table => new
                {
                    EventSeverityId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventSeverity", x => x.EventSeverityId);
                });

            migrationBuilder.CreateTable(
                name: "EventSourceType",
                schema: "EventManagement",
                columns: table => new
                {
                    EventSourceTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventSourceType", x => x.EventSourceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EventStatus",
                schema: "EventManagement",
                columns: table => new
                {
                    EventStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventStatus", x => x.EventStatusId);
                });

            migrationBuilder.CreateTable(
                name: "EventTimelineEntryType",
                schema: "EventManagement",
                columns: table => new
                {
                    EventTimelineEntryTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventTimelineEntryType", x => x.EventTimelineEntryTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EventType",
                schema: "EventManagement",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventType", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "IncidentActionStatus",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentActionStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_IncidentActionStatus", x => x.IncidentActionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "IncidentActionType",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentActionTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_IncidentActionType", x => x.IncidentActionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "IncidentStatus",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_IncidentStatus", x => x.IncidentStatusId);
                });

            migrationBuilder.CreateTable(
                name: "IncidentType",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_IncidentType", x => x.IncidentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OperationalEvent",
                schema: "EventManagement",
                columns: table => new
                {
                    OperationalEventId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    EventStatusId = table.Column<int>(type: "int", nullable: false),
                    EventSeverityId = table.Column<int>(type: "int", nullable: false),
                    EventSourceTypeId = table.Column<int>(type: "int", nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DetectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: true),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    OwningPersonId = table.Column<int>(type: "int", nullable: true),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDrill = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_OperationalEvent", x => x.OperationalEventId);
                    table.ForeignKey(
                        name: "FK_EventManagement_OperationalEvent_EventSeverity",
                        column: x => x.EventSeverityId,
                        principalSchema: "EventManagement",
                        principalTable: "EventSeverity",
                        principalColumn: "EventSeverityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_OperationalEvent_EventSourceType",
                        column: x => x.EventSourceTypeId,
                        principalSchema: "EventManagement",
                        principalTable: "EventSourceType",
                        principalColumn: "EventSourceTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_OperationalEvent_EventStatus",
                        column: x => x.EventStatusId,
                        principalSchema: "EventManagement",
                        principalTable: "EventStatus",
                        principalColumn: "EventStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_OperationalEvent_EventType",
                        column: x => x.EventTypeId,
                        principalSchema: "EventManagement",
                        principalTable: "EventType",
                        principalColumn: "EventTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_OperationalEvent_Unit",
                        column: x => x.UnitId,
                        principalSchema: "ReactorFleet",
                        principalTable: "Unit",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventAlarmLink",
                schema: "EventManagement",
                columns: table => new
                {
                    EventAlarmLinkId = table.Column<long>(type: "bigint", nullable: false),
                    OperationalEventId = table.Column<long>(type: "bigint", nullable: false),
                    AlarmEventId = table.Column<long>(type: "bigint", nullable: false),
                    LinkRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventAlarmLink", x => x.EventAlarmLinkId);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventAlarmLink_AlarmEvent",
                        column: x => x.AlarmEventId,
                        principalSchema: "AlarmManagement",
                        principalTable: "AlarmEvent",
                        principalColumn: "AlarmEventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventAlarmLink_OperationalEvent",
                        column: x => x.OperationalEventId,
                        principalSchema: "EventManagement",
                        principalTable: "OperationalEvent",
                        principalColumn: "OperationalEventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventFloodLink",
                schema: "EventManagement",
                columns: table => new
                {
                    EventFloodLinkId = table.Column<long>(type: "bigint", nullable: false),
                    OperationalEventId = table.Column<long>(type: "bigint", nullable: false),
                    AlarmFloodId = table.Column<long>(type: "bigint", nullable: false),
                    LinkRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventFloodLink", x => x.EventFloodLinkId);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventFloodLink_AlarmFlood",
                        column: x => x.AlarmFloodId,
                        principalSchema: "AlarmManagement",
                        principalTable: "AlarmFlood",
                        principalColumn: "AlarmFloodId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventFloodLink_OperationalEvent",
                        column: x => x.OperationalEventId,
                        principalSchema: "EventManagement",
                        principalTable: "OperationalEvent",
                        principalColumn: "OperationalEventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventTimelineEntry",
                schema: "EventManagement",
                columns: table => new
                {
                    EventTimelineEntryId = table.Column<long>(type: "bigint", nullable: false),
                    OperationalEventId = table.Column<long>(type: "bigint", nullable: false),
                    EventTimelineEntryTypeId = table.Column<int>(type: "int", nullable: false),
                    EntryAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EnteredByUserId = table.Column<int>(type: "int", nullable: true),
                    SourceReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_EventTimelineEntry", x => x.EventTimelineEntryId);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventTimelineEntry_OperationalEvent",
                        column: x => x.OperationalEventId,
                        principalSchema: "EventManagement",
                        principalTable: "OperationalEvent",
                        principalColumn: "OperationalEventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_EventTimelineEntry_Type",
                        column: x => x.EventTimelineEntryTypeId,
                        principalSchema: "EventManagement",
                        principalTable: "EventTimelineEntryType",
                        principalColumn: "EventTimelineEntryTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incident",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentId = table.Column<long>(type: "bigint", nullable: false),
                    OperationalEventId = table.Column<long>(type: "bigint", nullable: false),
                    IncidentTypeId = table.Column<int>(type: "int", nullable: false),
                    IncidentStatusId = table.Column<int>(type: "int", nullable: false),
                    IncidentNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    InvestigationSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeadInvestigatorUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_Incident", x => x.IncidentId);
                    table.ForeignKey(
                        name: "FK_EventManagement_Incident_IncidentStatus",
                        column: x => x.IncidentStatusId,
                        principalSchema: "EventManagement",
                        principalTable: "IncidentStatus",
                        principalColumn: "IncidentStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_Incident_IncidentType",
                        column: x => x.IncidentTypeId,
                        principalSchema: "EventManagement",
                        principalTable: "IncidentType",
                        principalColumn: "IncidentTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_Incident_OperationalEvent",
                        column: x => x.OperationalEventId,
                        principalSchema: "EventManagement",
                        principalTable: "OperationalEvent",
                        principalColumn: "OperationalEventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentAction",
                schema: "EventManagement",
                columns: table => new
                {
                    IncidentActionId = table.Column<long>(type: "bigint", nullable: false),
                    IncidentId = table.Column<long>(type: "bigint", nullable: false),
                    IncidentActionTypeId = table.Column<int>(type: "int", nullable: false),
                    IncidentActionStatusId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValueSql: "N'system'"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventManagement_IncidentAction", x => x.IncidentActionId);
                    table.ForeignKey(
                        name: "FK_EventManagement_IncidentAction_Incident",
                        column: x => x.IncidentId,
                        principalSchema: "EventManagement",
                        principalTable: "Incident",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_IncidentAction_Status",
                        column: x => x.IncidentActionStatusId,
                        principalSchema: "EventManagement",
                        principalTable: "IncidentActionStatus",
                        principalColumn: "IncidentActionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventManagement_IncidentAction_Type",
                        column: x => x.IncidentActionTypeId,
                        principalSchema: "EventManagement",
                        principalTable: "IncidentActionType",
                        principalColumn: "IncidentActionTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAlarmLink_AlarmEventId",
                schema: "EventManagement",
                table: "EventAlarmLink",
                column: "AlarmEventId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventAlarmLink",
                schema: "EventManagement",
                table: "EventAlarmLink",
                columns: new[] { "OperationalEventId", "AlarmEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventFloodLink_AlarmFloodId",
                schema: "EventManagement",
                table: "EventFloodLink",
                column: "AlarmFloodId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventFloodLink",
                schema: "EventManagement",
                table: "EventFloodLink",
                columns: new[] { "OperationalEventId", "AlarmFloodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventSeverity_Code",
                schema: "EventManagement",
                table: "EventSeverity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventSourceType_Code",
                schema: "EventManagement",
                table: "EventSourceType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventStatus_Code",
                schema: "EventManagement",
                table: "EventStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTimelineEntry_EventTimelineEntryTypeId",
                schema: "EventManagement",
                table: "EventTimelineEntry",
                column: "EventTimelineEntryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTimelineEntry_OperationalEventId",
                schema: "EventManagement",
                table: "EventTimelineEntry",
                column: "OperationalEventId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventTimelineEntryType_Code",
                schema: "EventManagement",
                table: "EventTimelineEntryType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_EventType_Code",
                schema: "EventManagement",
                table: "EventType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incident_IncidentStatusId",
                schema: "EventManagement",
                table: "Incident",
                column: "IncidentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Incident_IncidentTypeId",
                schema: "EventManagement",
                table: "Incident",
                column: "IncidentTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_Incident_IncidentNumber",
                schema: "EventManagement",
                table: "Incident",
                column: "IncidentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_Incident_OperationalEvent",
                schema: "EventManagement",
                table: "Incident",
                column: "OperationalEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAction_IncidentActionStatusId",
                schema: "EventManagement",
                table: "IncidentAction",
                column: "IncidentActionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAction_IncidentActionTypeId",
                schema: "EventManagement",
                table: "IncidentAction",
                column: "IncidentActionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAction_IncidentId",
                schema: "EventManagement",
                table: "IncidentAction",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_IncidentActionStatus_Code",
                schema: "EventManagement",
                table: "IncidentActionStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_IncidentActionType_Code",
                schema: "EventManagement",
                table: "IncidentActionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_IncidentStatus_Code",
                schema: "EventManagement",
                table: "IncidentStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_IncidentType_Code",
                schema: "EventManagement",
                table: "IncidentType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperationalEvent_EventSeverityId",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "EventSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalEvent_EventSourceTypeId",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "EventSourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalEvent_EventStatusId",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "EventStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalEvent_EventTypeId",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalEvent_UnitId",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "UQ_EventManagement_OperationalEvent_EventCode",
                schema: "EventManagement",
                table: "OperationalEvent",
                column: "EventCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventAlarmLink",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventFloodLink",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventTimelineEntry",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "IncidentAction",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventTimelineEntryType",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "Incident",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "IncidentActionStatus",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "IncidentActionType",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "IncidentStatus",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "IncidentType",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "OperationalEvent",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventSeverity",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventSourceType",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventStatus",
                schema: "EventManagement");

            migrationBuilder.DropTable(
                name: "EventType",
                schema: "EventManagement");
        }
    }
}
