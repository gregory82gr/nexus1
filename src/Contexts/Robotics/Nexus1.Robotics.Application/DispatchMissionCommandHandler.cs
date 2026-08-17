using Nexus1.BuildingBlocks.Application;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Application;

public sealed class DispatchMissionCommandHandler(
    IRepository<Mission, MissionId> missionRepository, IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<DispatchMissionCommand, long>
{
    public async Task<Result<long>> Handle(DispatchMissionCommand command, CancellationToken cancellationToken)
    {
        Mission mission;
        try
        {
            mission = Mission.Create(
                new MissionId(idGenerator.NextLong()), command.UnitId, new MissionTypeId(command.MissionTypeId),
                new MissionStatusId(command.MissionStatusId), new MissionPriorityId(command.MissionPriorityId),
                command.Code, command.Title, command.RequestedAtUtc, command.Objective, command.PlannedStartUtc,
                command.PlannedEndUtc, command.ActualStartUtc, command.ActualEndUtc, command.RequestedByUserId,
                command.ApprovedByUserId);
        }
        catch (ArgumentException ex)
        {
            return Result<long>.Failure(ex.Message);
        }

        await missionRepository.AddAsync(mission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(mission.Id.Value);
    }
}
