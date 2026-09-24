using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Reporting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInconclusiveProjectionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizedAtUtc",
                schema: "Reporting",
                table: "RootCauseCaseSummary",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                schema: "Reporting",
                table: "RootCauseCaseSummary",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PendingInconclusive",
                schema: "Reporting",
                columns: table => new
                {
                    AnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DecidedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reporting_PendingInconclusive", x => x.AnalysisId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingInconclusive",
                schema: "Reporting");

            migrationBuilder.DropColumn(
                name: "FinalizedAtUtc",
                schema: "Reporting",
                table: "RootCauseCaseSummary");

            migrationBuilder.DropColumn(
                name: "Reason",
                schema: "Reporting",
                table: "RootCauseCaseSummary");
        }
    }
}
