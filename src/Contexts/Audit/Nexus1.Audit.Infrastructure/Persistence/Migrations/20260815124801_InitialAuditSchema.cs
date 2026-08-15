using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Audit");

            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.CreateTable(
                name: "AuditEvidenceRecord",
                schema: "Audit",
                columns: table => new
                {
                    AuditEvidenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceAnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    EnvelopeBytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    EnvelopeSha256 = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CausationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_AuditEvidenceRecord", x => x.AuditEvidenceId);
                });

            migrationBuilder.CreateTable(
                name: "InboxReceipt",
                schema: "messaging",
                columns: table => new
                {
                    ConsumerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Producer = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messaging_InboxReceipt", x => new { x.ConsumerName, x.MessageId });
                    table.CheckConstraint("CK_messaging_InboxReceipt_CompletedAtUtc", "[CompletedAtUtc] >= [ReceivedAtUtc]");
                });

            migrationBuilder.CreateTable(
                name: "PoisonMessage",
                schema: "messaging",
                columns: table => new
                {
                    PoisonMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnvelopeSha256 = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    TerminalReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RetryAttempts = table.Column<int>(type: "int", nullable: false),
                    FirstFailedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuarantinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messaging_PoisonMessage", x => x.PoisonMessageId);
                });

            migrationBuilder.CreateTable(
                name: "RetryTicket",
                schema: "messaging",
                columns: table => new
                {
                    RetryTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Attempt = table.Column<int>(type: "int", nullable: false),
                    PolicyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FailureCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FirstFailedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Producer = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    OriginalRoutingKey = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    EnvelopeBytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    EnvelopeSha256 = table.Column<byte[]>(type: "binary(32)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messaging_RetryTicket", x => x.RetryTicketId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Audit_AuditEvidenceRecord_SourceAnalysisId",
                schema: "Audit",
                table: "AuditEvidenceRecord",
                column: "SourceAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_messaging_PoisonMessage_Identity",
                schema: "messaging",
                table: "PoisonMessage",
                columns: new[] { "ConsumerName", "MessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_messaging_RetryTicket_Due",
                schema: "messaging",
                table: "RetryTicket",
                columns: new[] { "PublishedAtUtc", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UQ_messaging_RetryTicket_Attempt",
                schema: "messaging",
                table: "RetryTicket",
                columns: new[] { "ConsumerName", "MessageId", "Attempt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvidenceRecord",
                schema: "Audit");

            migrationBuilder.DropTable(
                name: "InboxReceipt",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "PoisonMessage",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "RetryTicket",
                schema: "messaging");
        }
    }
}
