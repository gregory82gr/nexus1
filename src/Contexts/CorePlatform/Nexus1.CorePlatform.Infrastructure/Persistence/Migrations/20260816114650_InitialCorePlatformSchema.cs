using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCorePlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "CorePlatform");

            migrationBuilder.CreateTable(
                name: "AppSetting",
                schema: "CorePlatform",
                columns: table => new
                {
                    AppSettingId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_AppSetting", x => x.AppSettingId);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                schema: "CorePlatform",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Iso2Code = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Iso3Code = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    NumericCode = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OfficialName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Country", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "EngineeringUnit",
                schema: "CorePlatform",
                columns: table => new
                {
                    EngineeringUnitId = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuantityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SiConversionFactor = table.Column<decimal>(type: "decimal(28,12)", nullable: true),
                    SiConversionOffset = table.Column<decimal>(type: "decimal(28,12)", nullable: true),
                    IsDimensionless = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_EngineeringUnit", x => x.EngineeringUnitId);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlag",
                schema: "CorePlatform",
                columns: table => new
                {
                    FeatureFlagId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EnvironmentName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EnabledFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_FeatureFlag", x => x.FeatureFlagId);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                schema: "CorePlatform",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRightToLeft = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Language", x => x.LanguageId);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfiguration",
                schema: "CorePlatform",
                columns: table => new
                {
                    SystemConfigurationId = table.Column<int>(type: "int", nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigurationKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchemaVersion = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_SystemConfiguration", x => x.SystemConfigurationId);
                });

            migrationBuilder.CreateTable(
                name: "TimeZone",
                schema: "CorePlatform",
                columns: table => new
                {
                    TimeZoneId = table.Column<int>(type: "int", nullable: false),
                    IanaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WindowsName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentUtcOffsetMinutes = table.Column<short>(type: "smallint", nullable: false),
                    ObservesDst = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_TimeZone", x => x.TimeZoneId);
                });

            migrationBuilder.CreateTable(
                name: "Version",
                schema: "CorePlatform",
                columns: table => new
                {
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ComponentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuildSignature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GitCommit = table.Column<string>(type: "nchar(40)", fixedLength: true, maxLength: 40, nullable: true),
                    SchemaMigration = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReleaseDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangelogSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Version", x => x.VersionId);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                schema: "CorePlatform",
                columns: table => new
                {
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RegionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Region", x => x.RegionId);
                    table.ForeignKey(
                        name: "FK_CorePlatform_Region_Country",
                        column: x => x.CountryId,
                        principalSchema: "CorePlatform",
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Localization",
                schema: "CorePlatform",
                columns: table => new
                {
                    LocalizationId = table.Column<int>(type: "int", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMachineTranslated = table.Column<bool>(type: "bit", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Localization", x => x.LocalizationId);
                    table.ForeignKey(
                        name: "FK_CorePlatform_Localization_Language",
                        column: x => x.LanguageId,
                        principalSchema: "CorePlatform",
                        principalTable: "Language",
                        principalColumn: "LanguageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Calendar",
                schema: "CorePlatform",
                columns: table => new
                {
                    CalendarId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TimeZoneId = table.Column<int>(type: "int", nullable: false),
                    CalendarType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WorkingDaysMask = table.Column<byte>(type: "tinyint", nullable: false),
                    WorkingDayStart = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    WorkingDayEnd = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    Is24x7 = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorePlatform_Calendar", x => x.CalendarId);
                    table.ForeignKey(
                        name: "FK_CorePlatform_Calendar_TimeZone",
                        column: x => x.TimeZoneId,
                        principalSchema: "CorePlatform",
                        principalTable: "TimeZone",
                        principalColumn: "TimeZoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_AppSetting_Key",
                schema: "CorePlatform",
                table: "AppSetting",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorePlatform_Calendar_TimeZoneId",
                schema: "CorePlatform",
                table: "Calendar",
                column: "TimeZoneId");

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Calendar_Code",
                schema: "CorePlatform",
                table: "Calendar",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Country_Iso2Code",
                schema: "CorePlatform",
                table: "Country",
                column: "Iso2Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Country_Iso3Code",
                schema: "CorePlatform",
                table: "Country",
                column: "Iso3Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorePlatform_EngineeringUnit_QuantityType",
                schema: "CorePlatform",
                table: "EngineeringUnit",
                column: "QuantityType");

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_EngineeringUnit_Symbol",
                schema: "CorePlatform",
                table: "EngineeringUnit",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_FeatureFlag_Code_Environment",
                schema: "CorePlatform",
                table: "FeatureFlag",
                columns: new[] { "Code", "EnvironmentName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Language_Code",
                schema: "CorePlatform",
                table: "Language",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorePlatform_Localization_LanguageId",
                schema: "CorePlatform",
                table: "Localization",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_CorePlatform_Localization_ResourceKey",
                schema: "CorePlatform",
                table: "Localization",
                column: "ResourceKey");

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Localization_ResourceKey_Language",
                schema: "CorePlatform",
                table: "Localization",
                columns: new[] { "ResourceKey", "LanguageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorePlatform_Region_CountryId",
                schema: "CorePlatform",
                table: "Region",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Region_Country_Code",
                schema: "CorePlatform",
                table: "Region",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_SystemConfiguration_Module_Key_Version",
                schema: "CorePlatform",
                table: "SystemConfiguration",
                columns: new[] { "ModuleName", "ConfigurationKey", "SchemaVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_TimeZone_IanaName",
                schema: "CorePlatform",
                table: "TimeZone",
                column: "IanaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_CorePlatform_Version_Component_Version",
                schema: "CorePlatform",
                table: "Version",
                columns: new[] { "ComponentName", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CorePlatform_Version_Current_Component",
                schema: "CorePlatform",
                table: "Version",
                column: "ComponentName",
                unique: true,
                filter: "[IsCurrent] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSetting",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Calendar",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "EngineeringUnit",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "FeatureFlag",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Localization",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Region",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "SystemConfiguration",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Version",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "TimeZone",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Language",
                schema: "CorePlatform");

            migrationBuilder.DropTable(
                name: "Country",
                schema: "CorePlatform");
        }
    }
}
