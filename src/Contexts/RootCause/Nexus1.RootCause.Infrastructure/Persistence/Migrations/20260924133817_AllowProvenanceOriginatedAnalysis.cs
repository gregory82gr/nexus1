using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowProvenanceOriginatedAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AlarmFloodId",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AlarmFloodId",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
