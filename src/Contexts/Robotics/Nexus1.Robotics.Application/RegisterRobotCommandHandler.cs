using Nexus1.BuildingBlocks.Application;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Application;

public sealed class RegisterRobotCommandHandler(
    IRepository<Robot, RobotId> robotRepository, IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<RegisterRobotCommand, int>
{
    public async Task<Result<int>> Handle(RegisterRobotCommand command, CancellationToken cancellationToken)
    {
        Robot robot;
        try
        {
            robot = Robot.Create(
                new RobotId(idGenerator.NextInt()), new RobotModelId(command.RobotModelId),
                new RobotStatusId(command.RobotStatusId), command.Code, command.Name, command.HomeUnitId,
                command.SerialNumber, command.CommissionedAtUtc, command.RetiredAtUtc, command.LastSeenAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await robotRepository.AddAsync(robot, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(robot.Id.Value);
    }
}
