using Nexus1.BuildingBlocks.Application;
using Nexus1.Organization.Domain;

namespace Nexus1.Organization.Application;

public sealed class AssignPersonToDepartmentCommandHandler(
    IRepository<Person, PersonId> personRepository,
    IRepository<Department, DepartmentId> departmentRepository,
    IRepository<DepartmentAssignment, DepartmentAssignmentId> assignmentRepository,
    IUnitOfWork unitOfWork,
    IIdGenerator idGenerator,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<AssignPersonToDepartmentCommand, int>
{
    public async Task<Result<int>> Handle(AssignPersonToDepartmentCommand command, CancellationToken cancellationToken)
    {
        var personId = new PersonId(command.PersonId);
        var departmentId = new DepartmentId(command.DepartmentId);

        if (await personRepository.GetByIdAsync(personId, cancellationToken) is null)
        {
            return Result<int>.Failure($"Person {command.PersonId} does not exist.");
        }

        if (await departmentRepository.GetByIdAsync(departmentId, cancellationToken) is null)
        {
            return Result<int>.Failure($"Department {command.DepartmentId} does not exist.");
        }

        DepartmentAssignment assignment;
        try
        {
            assignment = DepartmentAssignment.Create(
                new DepartmentAssignmentId(idGenerator.NextInt()), personId, departmentId, command.StartDate,
                dateTimeProvider.UtcNow, command.PositionId is { } positionId ? new PositionId(positionId) : null,
                isPrimary: command.IsPrimary);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await assignmentRepository.AddAsync(assignment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(assignment.Id.Value);
    }
}
