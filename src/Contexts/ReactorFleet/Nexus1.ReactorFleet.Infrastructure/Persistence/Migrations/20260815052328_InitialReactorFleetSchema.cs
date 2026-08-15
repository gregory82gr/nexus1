using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.ReactorFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReactorFleetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ReactorFleet");

            migrationBuilder.CreateTable(
                name: "Unit",
                schema: "ReactorFleet",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactorFleet_Unit", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "UnitPowerSnapshot",
                schema: "ReactorFleet",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PowerPercent = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactorFleet_UnitPowerSnapshot", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_ReactorFleet_Unit_Code",
                schema: "ReactorFleet",
                table: "Unit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactorFleet_UnitPowerSnapshot_UnitId_RecordedAtUtc",
                schema: "ReactorFleet",
                table: "UnitPowerSnapshot",
                columns: new[] { "UnitId", "RecordedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unit",
                schema: "ReactorFleet");

            migrationBuilder.DropTable(
                name: "UnitPowerSnapshot",
                schema: "ReactorFleet");
        }
    }
}
