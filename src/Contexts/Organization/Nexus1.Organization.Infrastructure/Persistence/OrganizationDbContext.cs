using Microsoft.EntityFrameworkCore;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Infrastructure.Persistence;

/// <summary>
/// Own physical database (OrganizationDb, ADR-017) — Person carries real
/// PII (GivenName, FamilyName, DisplayName, WorkEmail, WorkPhone), a
/// data-sensitivity reason to isolate independent of deployment topology,
/// the same class of reason ADR-016 used for Security.
/// </summary>
public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : DbContext(options)
{
    public DbSet<LegalEntityType> LegalEntityTypes => Set<LegalEntityType>();

    public DbSet<SiteType> SiteTypes => Set<SiteType>();

    public DbSet<PlantType> PlantTypes => Set<PlantType>();

    public DbSet<DepartmentType> DepartmentTypes => Set<DepartmentType>();

    public DbSet<TeamType> TeamTypes => Set<TeamType>();

    public DbSet<PersonType> PersonTypes => Set<PersonType>();

    public DbSet<EmploymentStatus> EmploymentStatuses => Set<EmploymentStatus>();

    public DbSet<QualificationStatus> QualificationStatuses => Set<QualificationStatus>();

    public DbSet<LegalEntity> LegalEntities => Set<LegalEntity>();

    public DbSet<Site> Sites => Set<Site>();

    public DbSet<Plant> Plants => Set<Plant>();

    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Position> Positions => Set<Position>();

    public DbSet<Person> People => Set<Person>();

    public DbSet<DepartmentAssignment> DepartmentAssignments => Set<DepartmentAssignment>();

    public DbSet<TeamMembership> TeamMemberships => Set<TeamMembership>();

    public DbSet<Qualification> Qualifications => Set<Qualification>();

    public DbSet<PersonQualification> PersonQualifications => Set<PersonQualification>();

    public DbSet<PersonnelRequirement> PersonnelRequirements => Set<PersonnelRequirement>();

    public DbSet<StaffingScenario> StaffingScenarios => Set<StaffingScenario>();

    public DbSet<StaffingScenarioRequirement> StaffingScenarioRequirements => Set<StaffingScenarioRequirement>();

    public DbSet<StaffingScenarioResult> StaffingScenarioResults => Set<StaffingScenarioResult>();

    public DbSet<StaffingScenarioGap> StaffingScenarioGaps => Set<StaffingScenarioGap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
    }
}
