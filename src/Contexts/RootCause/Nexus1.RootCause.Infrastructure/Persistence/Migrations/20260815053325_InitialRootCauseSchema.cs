using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRootCauseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RootCause");

            migrationBuilder.CreateTable(
                name: "RootCauseAnalysis",
                schema: "RootCause",
                columns: table => new
                {
                    RootCauseAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AlarmFloodId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OpenedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Verdict = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_RootCauseAnalysis", x => x.RootCauseAnalysisId);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisHypothesis",
                schema: "RootCause",
                columns: table => new
                {
                    AnalysisHypothesisId = table.Column<int>(type: "int", nullable: false),
                    HypothesisStatement = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RootCauseAnalysisId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_AnalysisHypothesis", x => x.AnalysisHypothesisId);
                    table.ForeignKey(
                        name: "FK_RootCause_AnalysisHypothesis_RootCauseAnalysisId",
                        column: x => x.RootCauseAnalysisId,
                        principalSchema: "RootCause",
                        principalTable: "RootCauseAnalysis",
                        principalColumn: "RootCauseAnalysisId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HypothesisEvidence",
                schema: "RootCause",
                columns: table => new
                {
                    HypothesisEvidenceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalysisHypothesisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_HypothesisEvidence", x => x.HypothesisEvidenceId);
                    table.ForeignKey(
                        name: "FK_RootCause_HypothesisEvidence_AnalysisHypothesisId",
                        column: x => x.AnalysisHypothesisId,
                        principalSchema: "RootCause",
                        principalTable: "AnalysisHypothesis",
                        principalColumn: "AnalysisHypothesisId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisHypothesis_RootCauseAnalysisId",
                schema: "RootCause",
                table: "AnalysisHypothesis",
                column: "RootCauseAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_HypothesisEvidence_AnalysisHypothesisId",
                schema: "RootCause",
                table: "HypothesisEvidence",
                column: "AnalysisHypothesisId");

            migrationBuilder.CreateIndex(
                name: "IX_RootCause_RootCauseAnalysis_AlarmFloodId",
                schema: "RootCause",
                table: "RootCauseAnalysis",
                column: "AlarmFloodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HypothesisEvidence",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "AnalysisHypothesis",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "RootCauseAnalysis",
                schema: "RootCause");
        }
    }
}
