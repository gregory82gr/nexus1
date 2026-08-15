using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.AlarmManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAlarmManagementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AlarmManagement");

            migrationBuilder.CreateTable(
                name: "AlarmDefinition",
                schema: "AlarmManagement",
                columns: table => new
                {
                    AlarmDefinitionId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmManagement_AlarmDefinition", x => x.AlarmDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "AlarmEvent",
                schema: "AlarmManagement",
                columns: table => new
                {
                    AlarmEventId = table.Column<long>(type: "bigint", nullable: false),
                    AlarmDefinitionId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RaisedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AcknowledgedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmManagement_AlarmEvent", x => x.AlarmEventId);
                });

            migrationBuilder.CreateTable(
                name: "AlarmFlood",
                schema: "AlarmManagement",
                columns: table => new
                {
                    AlarmFloodId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmManagement_AlarmFlood", x => x.AlarmFloodId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_AlarmManagement_AlarmDefinition_UnitId_Code",
                schema: "AlarmManagement",
                table: "AlarmDefinition",
                columns: new[] { "UnitId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlarmManagement_AlarmEvent_AlarmDefinitionId_RaisedAtUtc",
                schema: "AlarmManagement",
                table: "AlarmEvent",
                columns: new[] { "AlarmDefinitionId", "RaisedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmManagement_AlarmEvent_UnitId_RaisedAtUtc",
                schema: "AlarmManagement",
                table: "AlarmEvent",
                columns: new[] { "UnitId", "RaisedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AlarmManagement_AlarmFlood_UnitId_StartedAtUtc",
                schema: "AlarmManagement",
                table: "AlarmFlood",
                columns: new[] { "UnitId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmDefinition",
                schema: "AlarmManagement");

            migrationBuilder.DropTable(
                name: "AlarmEvent",
                schema: "AlarmManagement");

            migrationBuilder.DropTable(
                name: "AlarmFlood",
                schema: "AlarmManagement");
        }
    }
}
