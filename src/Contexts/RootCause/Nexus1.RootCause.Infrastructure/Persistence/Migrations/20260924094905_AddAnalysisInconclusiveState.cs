using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisInconclusiveState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAtUtc",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecidedBy",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InconclusiveReason",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecidedAtUtc",
                schema: "RootCause",
                table: "RootCauseAnalysis");

            migrationBuilder.DropColumn(
                name: "DecidedBy",
                schema: "RootCause",
                table: "RootCauseAnalysis");

            migrationBuilder.DropColumn(
                name: "InconclusiveReason",
                schema: "RootCause",
                table: "RootCauseAnalysis");
        }
    }
}
