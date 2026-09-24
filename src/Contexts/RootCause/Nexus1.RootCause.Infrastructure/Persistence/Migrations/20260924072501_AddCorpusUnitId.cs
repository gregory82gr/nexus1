using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorpusUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the corpus scoping key (ADR-037). Add it nullable first, backfill the
            // ONLY corpus that exists before this migration -- EVT-2026-0418's, on unit 1
            // -- then make it NOT NULL. This leaves no lingering default constraint (the
            // model maps UnitId as a plain required int); the seed sets it explicitly for
            // every new incident's chunks.
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "RootCause",
                table: "Corpus",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE [RootCause].[Corpus] SET [UnitId] = 1 WHERE [UnitId] IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                schema: "RootCause",
                table: "Corpus",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootCause_Corpus_UnitId",
                schema: "RootCause",
                table: "Corpus",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RootCause_Corpus_UnitId",
                schema: "RootCause",
                table: "Corpus");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "RootCause",
                table: "Corpus");
        }
    }
}
