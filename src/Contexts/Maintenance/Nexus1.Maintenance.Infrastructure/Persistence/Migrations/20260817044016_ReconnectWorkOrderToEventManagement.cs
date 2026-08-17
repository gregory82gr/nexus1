using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Maintenance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconnectWorkOrderToEventManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_OriginIncidentActionId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "OriginIncidentActionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_OriginOperationalEventId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "OriginOperationalEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_WorkOrder_OriginIncidentAction",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "OriginIncidentActionId",
                principalSchema: "EventManagement",
                principalTable: "IncidentAction",
                principalColumn: "IncidentActionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenance_WorkOrder_OriginOperationalEvent",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "OriginOperationalEventId",
                principalSchema: "EventManagement",
                principalTable: "OperationalEvent",
                principalColumn: "OperationalEventId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_WorkOrder_OriginIncidentAction",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenance_WorkOrder_OriginOperationalEvent",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_OriginIncidentActionId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_OriginOperationalEventId",
                schema: "Maintenance",
                table: "WorkOrder");
        }
    }
}
