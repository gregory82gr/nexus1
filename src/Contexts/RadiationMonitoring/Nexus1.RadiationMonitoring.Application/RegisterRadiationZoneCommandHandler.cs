using Nexus1.BuildingBlocks.Application;
using Microsoft.Extensions.DependencyInjection;
using Nexus1.RadiationMonitoring.Domain;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class RegisterRadiationZoneCommandHandler(
    IRepository<RadiationZone, RadiationZoneId> radiationZoneRepository, [FromKeyedServices("RadiationMonitoring")] IUnitOfWork unitOfWork, IIdGenerator idGenerator)
    : ICommandHandler<RegisterRadiationZoneCommand, int>
{
    public async Task<Result<int>> Handle(RegisterRadiationZoneCommand command, CancellationToken cancellationToken)
    {
        RadiationZone zone;
        try
        {
            zone = RadiationZone.Create(
                new RadiationZoneId(idGenerator.NextInt()), new RadiationZoneTypeId(command.RadiationZoneTypeId),
                new RadiationZoneStatusId(command.RadiationZoneStatusId),
                new RadiationAreaClassificationId(command.RadiationAreaClassificationId), command.Code, command.Name,
                command.UnitId, command.EquipmentLocationId, command.Description, command.IsEntryControlled,
                command.RequiresDosimeter, command.PostedAtUtc);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }

        await radiationZoneRepository.AddAsync(zone, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(zone.Id.Value);
    }
}
