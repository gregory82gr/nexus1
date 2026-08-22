using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

public sealed class ResolvePersonOrganizationContextQueryHandlerTests : OrganizationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly StartDate = new(2026, 1, 1);

    private static ResolvePersonOrganizationContextQueryHandler CreateHandler(OrganizationDbContext dbContext) =>
        new(new EfPersonOrganizationContextFinder(dbContext));

    private async Task SeedPersonWithCurrentDepartmentAndTeamAsync()
    {
        await using var seedContext = CreateDbContext();

        var legalEntityType = LegalEntityType.Create(new LegalEntityTypeId(1), "OPERATOR", "Operator", NowUtc);
        var departmentType = DepartmentType.Create(new DepartmentTypeId(1), "OPS", "Operations", NowUtc);
        var teamType = TeamType.Create(new TeamTypeId(1), "SHIFT_CREW", "Shift Crew", NowUtc);
        var personType = PersonType.Create(new PersonTypeId(1), "EMPLOYEE", "Employee", NowUtc);
        await seedContext.LegalEntityTypes.AddAsync(legalEntityType);
        await seedContext.DepartmentTypes.AddAsync(departmentType);
        await seedContext.TeamTypes.AddAsync(teamType);
        await seedContext.PersonTypes.AddAsync(personType);

        var legalEntity = LegalEntity.Create(new LegalEntityId(1), legalEntityType.Id, "NEXUS1-OP", "Nexus1 Operator", NowUtc);
        await seedContext.LegalEntities.AddAsync(legalEntity);

        var department = Department.Create(new DepartmentId(1), legalEntity.Id, departmentType.Id, "OPS", "Operations", NowUtc);
        await seedContext.Departments.AddAsync(department);

        var team = Team.Create(new TeamId(1), department.Id, teamType.Id, "CREW-A", "Crew A", NowUtc);
        await seedContext.Teams.AddAsync(team);

        var person = Person.Create(
            new PersonId(1), personType.Id, "Ada", "Lovelace", "Ada Lovelace", NowUtc, applicationUserId: 77);
        await seedContext.People.AddAsync(person);

        await seedContext.DepartmentAssignments.AddAsync(
            DepartmentAssignment.Create(new DepartmentAssignmentId(1), person.Id, department.Id, StartDate, NowUtc, isPrimary: true));
        await seedContext.TeamMemberships.AddAsync(
            TeamMembership.Create(new TeamMembershipId(1), person.Id, team.Id, StartDate, NowUtc, isLead: true));

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Resolves_current_department_and_team_by_person_id()
    {
        await SeedPersonWithCurrentDepartmentAndTeamAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new ResolvePersonOrganizationContextQuery(PersonId: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada Lovelace", result.Value.DisplayName);
        Assert.Equal("Operations", result.Value.DepartmentName);
        Assert.Equal("Crew A", result.Value.TeamName);
    }

    [Fact]
    public async Task Resolves_by_application_user_passport_id()
    {
        await SeedPersonWithCurrentDepartmentAndTeamAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new ResolvePersonOrganizationContextQuery(ApplicationUserId: 77), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.PersonId);
    }

    [Fact]
    public async Task Neither_identifier_provided_fails()
    {
        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(new ResolvePersonOrganizationContextQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}
