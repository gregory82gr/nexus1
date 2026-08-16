using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Security.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSecuritySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Security");

            migrationBuilder.CreateTable(
                name: "PermissionCategory",
                schema: "Security",
                columns: table => new
                {
                    PermissionCategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_PermissionCategory", x => x.PermissionCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "RoleType",
                schema: "Security",
                columns: table => new
                {
                    RoleTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_RoleType", x => x.RoleTypeId);
                });

            migrationBuilder.CreateTable(
                name: "UserStatus",
                schema: "Security",
                columns: table => new
                {
                    UserStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_UserStatus", x => x.UserStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                schema: "Security",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    PermissionCategoryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    ResourceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsSafetyRelevant = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_Permission", x => x.PermissionId);
                    table.ForeignKey(
                        name: "FK_Security_Permission_PermissionCategory",
                        column: x => x.PermissionCategoryId,
                        principalSchema: "Security",
                        principalTable: "PermissionCategory",
                        principalColumn: "PermissionCategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationRole",
                schema: "Security",
                columns: table => new
                {
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    RoleTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentRoleId = table.Column<int>(type: "int", nullable: true),
                    IsBuiltIn = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_ApplicationRole", x => x.ApplicationRoleId);
                    table.ForeignKey(
                        name: "FK_Security_ApplicationRole_ParentRole",
                        column: x => x.ParentRoleId,
                        principalSchema: "Security",
                        principalTable: "ApplicationRole",
                        principalColumn: "ApplicationRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_ApplicationRole_RoleType",
                        column: x => x.RoleTypeId,
                        principalSchema: "Security",
                        principalTable: "RoleType",
                        principalColumn: "RoleTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                schema: "Security",
                columns: table => new
                {
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    UserStatusId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsServiceAccount = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GivenName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FamilyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_ApplicationUser", x => x.ApplicationUserId);
                    table.CheckConstraint("CK_Security_ApplicationUser_AccessFailedCount", "[AccessFailedCount] >= 0");
                    table.ForeignKey(
                        name: "FK_Security_ApplicationUser_UserStatus",
                        column: x => x.UserStatusId,
                        principalSchema: "Security",
                        principalTable: "UserStatus",
                        principalColumn: "UserStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                schema: "Security",
                columns: table => new
                {
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrantedByUserId = table.Column<int>(type: "int", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_RolePermission", x => new { x.ApplicationRoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_Security_RolePermission_ApplicationRole",
                        column: x => x.ApplicationRoleId,
                        principalSchema: "Security",
                        principalTable: "ApplicationRole",
                        principalColumn: "ApplicationRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_RolePermission_GrantedByUser",
                        column: x => x.GrantedByUserId,
                        principalSchema: "Security",
                        principalTable: "ApplicationUser",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_RolePermission_Permission",
                        column: x => x.PermissionId,
                        principalSchema: "Security",
                        principalTable: "Permission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPreference",
                schema: "Security",
                columns: table => new
                {
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    TimeZoneId = table.Column<int>(type: "int", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NumberFormat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReceiveEmailAlerts = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveInAppAlerts = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_UserPreference", x => x.ApplicationUserId);
                    table.ForeignKey(
                        name: "FK_Security_UserPreference_ApplicationUser",
                        column: x => x.ApplicationUserId,
                        principalSchema: "Security",
                        principalTable: "ApplicationUser",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                schema: "Security",
                columns: table => new
                {
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    ApplicationRoleId = table.Column<int>(type: "int", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Security_UserRole", x => new { x.ApplicationUserId, x.ApplicationRoleId });
                    table.ForeignKey(
                        name: "FK_Security_UserRole_ApplicationRole",
                        column: x => x.ApplicationRoleId,
                        principalSchema: "Security",
                        principalTable: "ApplicationRole",
                        principalColumn: "ApplicationRoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_UserRole_ApplicationUser",
                        column: x => x.ApplicationUserId,
                        principalSchema: "Security",
                        principalTable: "ApplicationUser",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Security_UserRole_AssignedByUser",
                        column: x => x.AssignedByUserId,
                        principalSchema: "Security",
                        principalTable: "ApplicationUser",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRole_ParentRoleId",
                schema: "Security",
                table: "ApplicationRole",
                column: "ParentRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRole_RoleTypeId",
                schema: "Security",
                table: "ApplicationRole",
                column: "RoleTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Security_ApplicationRole_NormalizedName",
                schema: "Security",
                table: "ApplicationRole",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_UserStatusId",
                schema: "Security",
                table: "ApplicationUser",
                column: "UserStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Security_ApplicationUser_NormalizedUserName",
                schema: "Security",
                table: "ApplicationUser",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_PermissionCategoryId",
                schema: "Security",
                table: "Permission",
                column: "PermissionCategoryId");

            migrationBuilder.CreateIndex(
                name: "UQ_Security_Permission_Code",
                schema: "Security",
                table: "Permission",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Security_PermissionCategory_Code",
                schema: "Security",
                table: "PermissionCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_GrantedByUserId",
                schema: "Security",
                table: "RolePermission",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                schema: "Security",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "UQ_Security_RoleType_Code",
                schema: "Security",
                table: "RoleType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_ApplicationRoleId",
                schema: "Security",
                table: "UserRole",
                column: "ApplicationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_AssignedByUserId",
                schema: "Security",
                table: "UserRole",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Security_UserStatus_Code",
                schema: "Security",
                table: "UserStatus",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermission",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "UserPreference",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "UserRole",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "Permission",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "ApplicationRole",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "ApplicationUser",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "PermissionCategory",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "RoleType",
                schema: "Security");

            migrationBuilder.DropTable(
                name: "UserStatus",
                schema: "Security");
        }
    }
}
