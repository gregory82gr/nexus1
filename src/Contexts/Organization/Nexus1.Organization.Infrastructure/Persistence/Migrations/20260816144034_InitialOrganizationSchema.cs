using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.Organization.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOrganizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Organization");

            migrationBuilder.CreateTable(
                name: "DepartmentType",
                schema: "Organization",
                columns: table => new
                {
                    DepartmentTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_DepartmentType", x => x.DepartmentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentStatus",
                schema: "Organization",
                columns: table => new
                {
                    EmploymentStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_EmploymentStatus", x => x.EmploymentStatusId);
                });

            migrationBuilder.CreateTable(
                name: "LegalEntityType",
                schema: "Organization",
                columns: table => new
                {
                    LegalEntityTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_LegalEntityType", x => x.LegalEntityTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PersonType",
                schema: "Organization",
                columns: table => new
                {
                    PersonTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_PersonType", x => x.PersonTypeId);
                });

            migrationBuilder.CreateTable(
                name: "PlantType",
                schema: "Organization",
                columns: table => new
                {
                    PlantTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_PlantType", x => x.PlantTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Qualification",
                schema: "Organization",
                columns: table => new
                {
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Issuer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValidityMonths = table.Column<int>(type: "int", nullable: true),
                    IsSafetyCritical = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Qualification", x => x.QualificationId);
                    table.CheckConstraint("CK_Organization_Qualification_ValidityMonths", "[ValidityMonths] IS NULL OR [ValidityMonths] > 0");
                });

            migrationBuilder.CreateTable(
                name: "QualificationStatus",
                schema: "Organization",
                columns: table => new
                {
                    QualificationStatusId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_QualificationStatus", x => x.QualificationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "SiteType",
                schema: "Organization",
                columns: table => new
                {
                    SiteTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_SiteType", x => x.SiteTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TeamType",
                schema: "Organization",
                columns: table => new
                {
                    TeamTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_TeamType", x => x.TeamTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LegalEntity",
                schema: "Organization",
                columns: table => new
                {
                    LegalEntityId = table.Column<int>(type: "int", nullable: false),
                    LegalEntityTypeId = table.Column<int>(type: "int", nullable: false),
                    ParentLegalEntityId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsOperator = table.Column<bool>(type: "bit", nullable: false),
                    IsVendor = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_LegalEntity", x => x.LegalEntityId);
                    table.ForeignKey(
                        name: "FK_Organization_LegalEntity_LegalEntityType",
                        column: x => x.LegalEntityTypeId,
                        principalSchema: "Organization",
                        principalTable: "LegalEntityType",
                        principalColumn: "LegalEntityTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_LegalEntity_ParentLegalEntity",
                        column: x => x.ParentLegalEntityId,
                        principalSchema: "Organization",
                        principalTable: "LegalEntity",
                        principalColumn: "LegalEntityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                schema: "Organization",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    LegalEntityId = table.Column<int>(type: "int", nullable: false),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                    DepartmentTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CostCentreCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Department", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Organization_Department_DepartmentType",
                        column: x => x.DepartmentTypeId,
                        principalSchema: "Organization",
                        principalTable: "DepartmentType",
                        principalColumn: "DepartmentTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Department_LegalEntity",
                        column: x => x.LegalEntityId,
                        principalSchema: "Organization",
                        principalTable: "LegalEntity",
                        principalColumn: "LegalEntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Department_ParentDepartment",
                        column: x => x.ParentDepartmentId,
                        principalSchema: "Organization",
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                schema: "Organization",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PersonTypeId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true),
                    LegalEntityId = table.Column<int>(type: "int", nullable: true),
                    PersonnelNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    GivenName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Person", x => x.PersonId);
                    table.ForeignKey(
                        name: "FK_Organization_Person_LegalEntity",
                        column: x => x.LegalEntityId,
                        principalSchema: "Organization",
                        principalTable: "LegalEntity",
                        principalColumn: "LegalEntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Person_PersonType",
                        column: x => x.PersonTypeId,
                        principalSchema: "Organization",
                        principalTable: "PersonType",
                        principalColumn: "PersonTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Site",
                schema: "Organization",
                columns: table => new
                {
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    LegalEntityId = table.Column<int>(type: "int", nullable: false),
                    SiteTypeId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    TimeZoneId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Site", x => x.SiteId);
                    table.CheckConstraint("CK_Organization_Site_Latitude", "[Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Organization_Site_Longitude", "[Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Organization_Site_LegalEntity",
                        column: x => x.LegalEntityId,
                        principalSchema: "Organization",
                        principalTable: "LegalEntity",
                        principalColumn: "LegalEntityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Site_SiteType",
                        column: x => x.SiteTypeId,
                        principalSchema: "Organization",
                        principalTable: "SiteType",
                        principalColumn: "SiteTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Position",
                schema: "Organization",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsSafetyCritical = table.Column<bool>(type: "bit", nullable: false),
                    RequiresShiftWork = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Position", x => x.PositionId);
                    table.ForeignKey(
                        name: "FK_Organization_Position_Department",
                        column: x => x.DepartmentId,
                        principalSchema: "Organization",
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                schema: "Organization",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    TeamTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsShiftTeam = table.Column<bool>(type: "bit", nullable: false),
                    IsEmergencyTeam = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Team", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Organization_Team_Department",
                        column: x => x.DepartmentId,
                        principalSchema: "Organization",
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Team_TeamType",
                        column: x => x.TeamTypeId,
                        principalSchema: "Organization",
                        principalTable: "TeamType",
                        principalColumn: "TeamTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonQualification",
                schema: "Organization",
                columns: table => new
                {
                    PersonQualificationId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    QualificationId = table.Column<int>(type: "int", nullable: false),
                    QualificationStatusId = table.Column<int>(type: "int", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_PersonQualification", x => x.PersonQualificationId);
                    table.CheckConstraint("CK_Organization_PersonQualification_Expiry", "[ExpiresAtUtc] IS NULL OR [IssuedAtUtc] IS NULL OR [ExpiresAtUtc] > [IssuedAtUtc]");
                    table.ForeignKey(
                        name: "FK_Organization_PersonQualification_Person",
                        column: x => x.PersonId,
                        principalSchema: "Organization",
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonQualification_Qualification",
                        column: x => x.QualificationId,
                        principalSchema: "Organization",
                        principalTable: "Qualification",
                        principalColumn: "QualificationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonQualification_QualificationStatus",
                        column: x => x.QualificationStatusId,
                        principalSchema: "Organization",
                        principalTable: "QualificationStatus",
                        principalColumn: "QualificationStatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Building",
                schema: "Organization",
                columns: table => new
                {
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuildingUsage = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FloorCount = table.Column<int>(type: "int", nullable: true),
                    IsControlledArea = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Building", x => x.BuildingId);
                    table.CheckConstraint("CK_Organization_Building_FloorCount", "[FloorCount] IS NULL OR [FloorCount] >= 0");
                    table.ForeignKey(
                        name: "FK_Organization_Building_Site",
                        column: x => x.SiteId,
                        principalSchema: "Organization",
                        principalTable: "Site",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Plant",
                schema: "Organization",
                columns: table => new
                {
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OperationalStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Plant", x => x.PlantId);
                    table.ForeignKey(
                        name: "FK_Organization_Plant_PlantType",
                        column: x => x.PlantTypeId,
                        principalSchema: "Organization",
                        principalTable: "PlantType",
                        principalColumn: "PlantTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_Plant_Site",
                        column: x => x.SiteId,
                        principalSchema: "Organization",
                        principalTable: "Site",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffingScenario",
                schema: "Organization",
                columns: table => new
                {
                    StaffingScenarioId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    ScenarioCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_StaffingScenario", x => x.StaffingScenarioId);
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenario_Site",
                        column: x => x.SiteId,
                        principalSchema: "Organization",
                        principalTable: "Site",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAssignment",
                schema: "Organization",
                columns: table => new
                {
                    DepartmentAssignmentId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_DepartmentAssignment", x => x.DepartmentAssignmentId);
                    table.CheckConstraint("CK_Organization_DepartmentAssignment_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.ForeignKey(
                        name: "FK_Organization_DepartmentAssignment_Department",
                        column: x => x.DepartmentId,
                        principalSchema: "Organization",
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_DepartmentAssignment_Person",
                        column: x => x.PersonId,
                        principalSchema: "Organization",
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_DepartmentAssignment_Position",
                        column: x => x.PositionId,
                        principalSchema: "Organization",
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembership",
                schema: "Organization",
                columns: table => new
                {
                    TeamMembershipId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsLead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_TeamMembership", x => x.TeamMembershipId);
                    table.CheckConstraint("CK_Organization_TeamMembership_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.ForeignKey(
                        name: "FK_Organization_TeamMembership_Person",
                        column: x => x.PersonId,
                        principalSchema: "Organization",
                        principalTable: "Person",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_TeamMembership_Position",
                        column: x => x.PositionId,
                        principalSchema: "Organization",
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_TeamMembership_Team",
                        column: x => x.TeamId,
                        principalSchema: "Organization",
                        principalTable: "Team",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelRequirement",
                schema: "Organization",
                columns: table => new
                {
                    PersonnelRequirementId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    PlantId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    MinRequiredCount = table.Column<int>(type: "int", nullable: false),
                    RequiredQualificationId = table.Column<int>(type: "int", nullable: true),
                    IsSafetyCritical = table.Column<bool>(type: "bit", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_PersonnelRequirement", x => x.PersonnelRequirementId);
                    table.CheckConstraint("CK_Organization_PersonnelRequirement_MinRequiredCount", "[MinRequiredCount] >= 0");
                    table.CheckConstraint("CK_Organization_PersonnelRequirement_Validity", "[ValidToUtc] IS NULL OR [ValidToUtc] > [ValidFromUtc]");
                    table.ForeignKey(
                        name: "FK_Organization_PersonnelRequirement_Department",
                        column: x => x.DepartmentId,
                        principalSchema: "Organization",
                        principalTable: "Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonnelRequirement_Plant",
                        column: x => x.PlantId,
                        principalSchema: "Organization",
                        principalTable: "Plant",
                        principalColumn: "PlantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonnelRequirement_Position",
                        column: x => x.PositionId,
                        principalSchema: "Organization",
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonnelRequirement_RequiredQualification",
                        column: x => x.RequiredQualificationId,
                        principalSchema: "Organization",
                        principalTable: "Qualification",
                        principalColumn: "QualificationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_PersonnelRequirement_Site",
                        column: x => x.SiteId,
                        principalSchema: "Organization",
                        principalTable: "Site",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffingScenarioRequirement",
                schema: "Organization",
                columns: table => new
                {
                    StaffingScenarioRequirementId = table.Column<int>(type: "int", nullable: false),
                    StaffingScenarioId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    RequiredQualificationId = table.Column<int>(type: "int", nullable: true),
                    RequiredCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_StaffingScenarioRequirement", x => x.StaffingScenarioRequirementId);
                    table.CheckConstraint("CK_Organization_StaffingScenarioRequirement_RequiredCount", "[RequiredCount] >= 0");
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioRequirement_Position",
                        column: x => x.PositionId,
                        principalSchema: "Organization",
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioRequirement_RequiredQualification",
                        column: x => x.RequiredQualificationId,
                        principalSchema: "Organization",
                        principalTable: "Qualification",
                        principalColumn: "QualificationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioRequirement_StaffingScenario",
                        column: x => x.StaffingScenarioId,
                        principalSchema: "Organization",
                        principalTable: "StaffingScenario",
                        principalColumn: "StaffingScenarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffingScenarioResult",
                schema: "Organization",
                columns: table => new
                {
                    StaffingScenarioResultId = table.Column<int>(type: "int", nullable: false),
                    StaffingScenarioId = table.Column<int>(type: "int", nullable: false),
                    EvaluatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvaluatedByUserId = table.Column<int>(type: "int", nullable: true),
                    OverallStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_StaffingScenarioResult", x => x.StaffingScenarioResultId);
                    table.CheckConstraint("CK_Organization_StaffingScenarioResult_OverallStatus", "[OverallStatus] IN ('Pass','Warning','Fail','NotEvaluated')");
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioResult_StaffingScenario",
                        column: x => x.StaffingScenarioId,
                        principalSchema: "Organization",
                        principalTable: "StaffingScenario",
                        principalColumn: "StaffingScenarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffingScenarioGap",
                schema: "Organization",
                columns: table => new
                {
                    StaffingScenarioGapId = table.Column<int>(type: "int", nullable: false),
                    StaffingScenarioResultId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    RequiredCount = table.Column<int>(type: "int", nullable: false),
                    AvailableCount = table.Column<int>(type: "int", nullable: false),
                    GapCount = table.Column<int>(type: "int", nullable: false, computedColumnSql: "(CASE WHEN [RequiredCount] > [AvailableCount] THEN [RequiredCount] - [AvailableCount] ELSE 0 END)", stored: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_StaffingScenarioGap", x => x.StaffingScenarioGapId);
                    table.CheckConstraint("CK_Organization_StaffingScenarioGap_Counts", "[RequiredCount] >= 0 AND [AvailableCount] >= 0");
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioGap_Position",
                        column: x => x.PositionId,
                        principalSchema: "Organization",
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_StaffingScenarioGap_StaffingScenarioResult",
                        column: x => x.StaffingScenarioResultId,
                        principalSchema: "Organization",
                        principalTable: "StaffingScenarioResult",
                        principalColumn: "StaffingScenarioResultId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Building_Site_Code",
                schema: "Organization",
                table: "Building",
                columns: new[] { "SiteId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_DepartmentTypeId",
                schema: "Organization",
                table: "Department",
                column: "DepartmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_ParentDepartmentId",
                schema: "Organization",
                table: "Department",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Department_LegalEntity_Code",
                schema: "Organization",
                table: "Department",
                columns: new[] { "LegalEntityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssignment_DepartmentId",
                schema: "Organization",
                table: "DepartmentAssignment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssignment_PersonId",
                schema: "Organization",
                table: "DepartmentAssignment",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssignment_PositionId",
                schema: "Organization",
                table: "DepartmentAssignment",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_DepartmentType_Code",
                schema: "Organization",
                table: "DepartmentType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_EmploymentStatus_Code",
                schema: "Organization",
                table: "EmploymentStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntity_ParentLegalEntityId",
                schema: "Organization",
                table: "LegalEntity",
                column: "ParentLegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_LegalEntity_CountryId",
                schema: "Organization",
                table: "LegalEntity",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_LegalEntity_LegalEntityTypeId",
                schema: "Organization",
                table: "LegalEntity",
                column: "LegalEntityTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_LegalEntity_Code",
                schema: "Organization",
                table: "LegalEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_LegalEntityType_Code",
                schema: "Organization",
                table: "LegalEntityType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Person_LegalEntityId",
                schema: "Organization",
                table: "Person",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PersonTypeId",
                schema: "Organization",
                table: "Person",
                column: "PersonTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Person_ApplicationUserId",
                schema: "Organization",
                table: "Person",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Person_PersonnelNumber",
                schema: "Organization",
                table: "Person",
                column: "PersonnelNumber",
                unique: true,
                filter: "[PersonnelNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelRequirement_DepartmentId",
                schema: "Organization",
                table: "PersonnelRequirement",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelRequirement_PlantId",
                schema: "Organization",
                table: "PersonnelRequirement",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelRequirement_PositionId",
                schema: "Organization",
                table: "PersonnelRequirement",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelRequirement_RequiredQualificationId",
                schema: "Organization",
                table: "PersonnelRequirement",
                column: "RequiredQualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelRequirement_SiteId",
                schema: "Organization",
                table: "PersonnelRequirement",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonQualification_PersonId",
                schema: "Organization",
                table: "PersonQualification",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonQualification_QualificationId",
                schema: "Organization",
                table: "PersonQualification",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonQualification_QualificationStatusId",
                schema: "Organization",
                table: "PersonQualification",
                column: "QualificationStatusId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_PersonType_Code",
                schema: "Organization",
                table: "PersonType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Plant_SiteId",
                schema: "Organization",
                table: "Plant",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Plant_PlantTypeId",
                schema: "Organization",
                table: "Plant",
                column: "PlantTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Plant_Code",
                schema: "Organization",
                table: "Plant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_PlantType_Code",
                schema: "Organization",
                table: "PlantType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Position_DepartmentId",
                schema: "Organization",
                table: "Position",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Position_Code",
                schema: "Organization",
                table: "Position",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Qualification_Code",
                schema: "Organization",
                table: "Qualification",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_QualificationStatus_Code",
                schema: "Organization",
                table: "QualificationStatus",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Site_CountryId_RegionId",
                schema: "Organization",
                table: "Site",
                columns: new[] { "CountryId", "RegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Site_LegalEntityId",
                schema: "Organization",
                table: "Site",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Site_SiteTypeId",
                schema: "Organization",
                table: "Site",
                column: "SiteTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Site_Code",
                schema: "Organization",
                table: "Site",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_SiteType_Code",
                schema: "Organization",
                table: "SiteType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_StaffingScenario_Site_Code",
                schema: "Organization",
                table: "StaffingScenario",
                columns: new[] { "SiteId", "ScenarioCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioGap_PositionId",
                schema: "Organization",
                table: "StaffingScenarioGap",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioGap_StaffingScenarioResultId",
                schema: "Organization",
                table: "StaffingScenarioGap",
                column: "StaffingScenarioResultId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioRequirement_PositionId",
                schema: "Organization",
                table: "StaffingScenarioRequirement",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioRequirement_RequiredQualificationId",
                schema: "Organization",
                table: "StaffingScenarioRequirement",
                column: "RequiredQualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioRequirement_StaffingScenarioId",
                schema: "Organization",
                table: "StaffingScenarioRequirement",
                column: "StaffingScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingScenarioResult_StaffingScenarioId",
                schema: "Organization",
                table: "StaffingScenarioResult",
                column: "StaffingScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_TeamTypeId",
                schema: "Organization",
                table: "Team",
                column: "TeamTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_Team_Department_Code",
                schema: "Organization",
                table: "Team",
                columns: new[] { "DepartmentId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembership_PersonId",
                schema: "Organization",
                table: "TeamMembership",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembership_PositionId",
                schema: "Organization",
                table: "TeamMembership",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembership_TeamId",
                schema: "Organization",
                table: "TeamMembership",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "UQ_Organization_TeamType_Code",
                schema: "Organization",
                table: "TeamType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Building",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "DepartmentAssignment",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "EmploymentStatus",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "PersonnelRequirement",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "PersonQualification",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "StaffingScenarioGap",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "StaffingScenarioRequirement",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "TeamMembership",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Plant",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "QualificationStatus",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "StaffingScenarioResult",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Qualification",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Person",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Position",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Team",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "PlantType",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "StaffingScenario",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "PersonType",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Department",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "TeamType",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Site",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "DepartmentType",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "LegalEntity",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "SiteType",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "LegalEntityType",
                schema: "Organization");
        }
    }
}
