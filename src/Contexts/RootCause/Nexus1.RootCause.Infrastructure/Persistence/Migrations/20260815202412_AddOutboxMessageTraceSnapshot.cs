using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.RootCause.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxMessageTraceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpanId",
                schema: "messaging",
                table: "OutboxMessage",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TraceFlags",
                schema: "messaging",
                table: "OutboxMessage",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceId",
                schema: "messaging",
                table: "OutboxMessage",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraceState",
                schema: "messaging",
                table: "OutboxMessage",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpanId",
                schema: "messaging",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceFlags",
                schema: "messaging",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceId",
                schema: "messaging",
                table: "OutboxMessage");

            migrationBuilder.DropColumn(
                name: "TraceState",
                schema: "messaging",
                table: "OutboxMessage");
        }
    }
}
