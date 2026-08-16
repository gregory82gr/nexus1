using Microsoft.EntityFrameworkCore;
using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Application;
using Nexus1.Organization.Domain;
using Nexus1.Organization.Infrastructure.Persistence;

namespace Nexus1.Organization.ComponentTests;

public sealed class AssignPersonToTeamCommandHandlerTests : OrganizationComponentTestDatabase
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private static AssignPersonToTeamCommandHandler CreateHandler(OrganizationDbContext dbContext) => new(
        new EfRepository<Person, PersonId>(dbContext),
        new EfRepository<Team, TeamId>(dbContext),
        new EfRepository<TeamMembership, TeamMembershipId>(dbContext),
        UnitOfWork(dbContext),
        new SequentialIdGenerator(),
        new FixedDateTimeProvider(NowUtc));

    private async Task SeedPersonAndTeamAsync()
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

        await seedContext.Teams.AddAsync(Team.Create(new TeamId(1), department.Id, teamType.Id, "CREW-A", "Crew A", NowUtc));

        await seedContext.People.AddAsync(Person.Create(new PersonId(1), personType.Id, "Ada", "Lovelace", "Ada Lovelace", NowUtc));

        await seedContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Assigning_an_existing_person_to_an_existing_team_persists_it()
    {
        await SeedPersonAndTeamAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new AssignPersonToTeamCommand(1, 1, new DateOnly(2026, 1, 1), IsLead: true), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verifyContext = CreateDbContext();
        var membership = await verifyContext.TeamMemberships.SingleAsync();
        Assert.Equal(new PersonId(1), membership.PersonId);
        Assert.Equal(new TeamId(1), membership.TeamId);
        Assert.True(membership.IsLead);
    }

    [Fact]
    public async Task Assigning_to_a_nonexistent_team_fails_without_writing_anything()
    {
        await SeedPersonAndTeamAsync();

        await using var dbContext = CreateDbContext();
        var result = await CreateHandler(dbContext).Handle(
            new AssignPersonToTeamCommand(1, 999, new DateOnly(2026, 1, 1)), CancellationToken.None);

        Assert.True(result.IsFailure);

        await using var verifyContext = CreateDbContext();
        Assert.Equal(0, await verifyContext.TeamMemberships.CountAsync());
    }
}
