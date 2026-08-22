using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Application;

public sealed class AssignPersonToTeamCommandHandler(
    IRepository<Person, PersonId> personRepository,
    IRepository<Team, TeamId> teamRepository,
    IRepository<TeamMembership, TeamMembershipId> membershipRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AssignPersonToTeamCommand, int>
{
    public async Task<Result<int>> Handle(AssignPersonToTeamCommand command, CancellationToken cancellationToken)
    {
        var personId = new PersonId(command.PersonId);
        var teamId = new TeamId(command.TeamId);

        if (await personRepository.GetByIdAsync(personId, cancellationToken) is null)
        {
            return Result<int>.Failure($"Person {command.PersonId} does not exist.");
        }

        if (await teamRepository.GetByIdAsync(teamId, cancellationToken) is null)
        {
            return Result<int>.Failure($"Team {command.TeamId} does not exist.");
        }

        TeamMembership membership;
        try
        {
            membership = TeamMembership.Create(
                new TeamMembershipId(idGenerator.NextInt()), personId, teamId, command.StartDate,
                dateTimeProvider.UtcNow, command.PositionId is { } positionId ? new PositionId(positionId) : null,
                isLead: command.IsLead);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await membershipRepository.AddAsync(membership, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(membership.Id.Value);
    }
}
