using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroundingStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audit",
                schema: "RootCause",
                columns: table => new
                {
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrevHash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    Hash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Audit", x => x.Seq);
                });

            migrationBuilder.CreateTable(
                name: "Component",
                schema: "RootCause",
                columns: table => new
                {
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    HealthScore = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AlarmCount = table.Column<int>(type: "int", nullable: false),
                    IllustrativeWeight = table.Column<double>(type: "float", nullable: true),
                    IllustrativeRole = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Component", x => x.ComponentId);
                });

            migrationBuilder.CreateTable(
                name: "Corpus",
                schema: "RootCause",
                columns: table => new
                {
                    ChunkId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocId = table.Column<int>(type: "int", nullable: false),
                    TrustTier = table.Column<byte>(type: "tinyint", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLabel = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Corpus", x => x.ChunkId);
                });

            migrationBuilder.CreateTable(
                name: "DiagnosisRun",
                schema: "RootCause",
                columns: table => new
                {
                    DiagnosisRunId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Verdict = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AbstainReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CorpusVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_DiagnosisRun", x => x.DiagnosisRunId);
                });

            migrationBuilder.CreateTable(
                name: "Historian",
                schema: "RootCause",
                columns: table => new
                {
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Quality = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Historian", x => new { x.ChannelId, x.TimestampUtc });
                });

            migrationBuilder.CreateTable(
                name: "Edge",
                schema: "RootCause",
                columns: table => new
                {
                    EdgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromComponentId = table.Column<int>(type: "int", nullable: false),
                    ToComponentId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    DelayMinSeconds = table.Column<int>(type: "int", nullable: false),
                    DelayMaxSeconds = table.Column<int>(type: "int", nullable: false),
                    SourceRef = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Edge", x => x.EdgeId);
                    table.ForeignKey(
                        name: "FK_RootCause_Edge_FromComponentId",
                        column: x => x.FromComponentId,
                        principalSchema: "RootCause",
                        principalTable: "Component",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RootCause_Edge_ToComponentId",
                        column: x => x.ToComponentId,
                        principalSchema: "RootCause",
                        principalTable: "Component",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Candidate",
                schema: "RootCause",
                columns: table => new
                {
                    DiagnosisRunId = table.Column<long>(type: "bigint", nullable: false),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Coverage = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootCause_Candidate", x => new { x.DiagnosisRunId, x.ComponentId });
                    table.ForeignKey(
                        name: "FK_RootCause_Candidate_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "RootCause",
                        principalTable: "Component",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RootCause_Candidate_DiagnosisRunId",
                        column: x => x.DiagnosisRunId,
                        principalSchema: "RootCause",
                        principalTable: "DiagnosisRun",
                        principalColumn: "DiagnosisRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidate_ComponentId",
                schema: "RootCause",
                table: "Candidate",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "UQ_RootCause_Component_Tag",
                schema: "RootCause",
                table: "Component",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RootCause_DiagnosisRun_IncidentId",
                schema: "RootCause",
                table: "DiagnosisRun",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Edge_ToComponentId",
                schema: "RootCause",
                table: "Edge",
                column: "ToComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_RootCause_Edge_FromComponentId",
                schema: "RootCause",
                table: "Edge",
                column: "FromComponentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "Candidate",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "Corpus",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "Edge",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "Historian",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "DiagnosisRun",
                schema: "RootCause");

            migrationBuilder.DropTable(
                name: "Component",
                schema: "RootCause");
        }
    }
}
