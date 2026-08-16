using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRootCauseAnalysisAlarmFloodStartedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AlarmFloodStartedAtUtc",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlarmFloodStartedAtUtc",
                schema: "RootCause",
                table: "RootCauseAnalysis");
        }
    }
}
