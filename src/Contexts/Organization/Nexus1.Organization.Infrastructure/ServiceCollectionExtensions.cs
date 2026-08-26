using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>connectionString points at OrganizationDb, Organization's own physical database (ADR-017) — not shared with any other context.</summary>
    public static IServiceCollection AddOrganizationInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrganizationDbContext>(options => options.UseSqlServer(
            connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Organization")));

        services.AddScoped<IRepository<LegalEntityType, LegalEntityTypeId>, EfRepository<LegalEntityType, LegalEntityTypeId>>();
        services.AddScoped<IRepository<SiteType, SiteTypeId>, EfRepository<SiteType, SiteTypeId>>();
        services.AddScoped<IRepository<PlantType, PlantTypeId>, EfRepository<PlantType, PlantTypeId>>();
        services.AddScoped<IRepository<DepartmentType, DepartmentTypeId>, EfRepository<DepartmentType, DepartmentTypeId>>();
        services.AddScoped<IRepository<TeamType, TeamTypeId>, EfRepository<TeamType, TeamTypeId>>();
        services.AddScoped<IRepository<PersonType, PersonTypeId>, EfRepository<PersonType, PersonTypeId>>();
        services.AddScoped<IRepository<EmploymentStatus, EmploymentStatusId>, EfRepository<EmploymentStatus, EmploymentStatusId>>();
        services.AddScoped<IRepository<QualificationStatus, QualificationStatusId>, EfRepository<QualificationStatus, QualificationStatusId>>();

        services.AddScoped<IRepository<LegalEntity, LegalEntityId>, EfRepository<LegalEntity, LegalEntityId>>();
        services.AddScoped<IRepository<Site, SiteId>, EfRepository<Site, SiteId>>();
        services.AddScoped<IRepository<Plant, PlantId>, EfRepository<Plant, PlantId>>();
        services.AddScoped<IRepository<Building, BuildingId>, EfRepository<Building, BuildingId>>();

        services.AddScoped<IRepository<Department, DepartmentId>, EfRepository<Department, DepartmentId>>();
        services.AddScoped<IRepository<Team, TeamId>, EfRepository<Team, TeamId>>();
        services.AddScoped<IRepository<Position, PositionId>, EfRepository<Position, PositionId>>();

        services.AddScoped<IRepository<Person, PersonId>, EfRepository<Person, PersonId>>();

        services.AddScoped<IRepository<DepartmentAssignment, DepartmentAssignmentId>, EfRepository<DepartmentAssignment, DepartmentAssignmentId>>();
        services.AddScoped<IRepository<TeamMembership, TeamMembershipId>, EfRepository<TeamMembership, TeamMembershipId>>();

        services.AddScoped<IRepository<Qualification, QualificationId>, EfRepository<Qualification, QualificationId>>();
        services.AddScoped<IRepository<PersonQualification, PersonQualificationId>, EfRepository<PersonQualification, PersonQualificationId>>();

        services.AddScoped<IRepository<PersonnelRequirement, PersonnelRequirementId>, EfRepository<PersonnelRequirement, PersonnelRequirementId>>();
        services.AddScoped<IRepository<StaffingScenario, StaffingScenarioId>, EfRepository<StaffingScenario, StaffingScenarioId>>();
        services.AddScoped<IRepository<StaffingScenarioRequirement, StaffingScenarioRequirementId>, EfRepository<StaffingScenarioRequirement, StaffingScenarioRequirementId>>();
        services.AddScoped<IRepository<StaffingScenarioResult, StaffingScenarioResultId>, EfRepository<StaffingScenarioResult, StaffingScenarioResultId>>();
        services.AddScoped<IRepository<StaffingScenarioGap, StaffingScenarioGapId>, EfRepository<StaffingScenarioGap, StaffingScenarioGapId>>();

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("Organization");

        services.AddScoped<ISitePlantHierarchyFinder, EfSitePlantHierarchyFinder>();
        services.AddScoped<IPersonOrganizationContextFinder, EfPersonOrganizationContextFinder>();
        services.AddScoped<IStaffingGapFinder, EfStaffingGapFinder>();
        services.AddScoped<IDepartmentRosterFinder, EfDepartmentRosterFinder>();

        return services;
    }
}
