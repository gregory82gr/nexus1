using Nexus1.AlarmManagement.Domain;
using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class GetActiveAlarmsForUnitQueryHandler(IAlarmEventFinder eventFinder)
    : IQueryHandler<GetActiveAlarmsForUnitQuery, IReadOnlyList<ActiveAlarmDto>>
{
    public async Task<Result<IReadOnlyList<ActiveAlarmDto>>> Handle(
        GetActiveAlarmsForUnitQuery query, CancellationToken cancellationToken)
    {
        var activeAlarms = await eventFinder.GetActiveForUnitAsync(new UnitId(query.UnitId), cancellationToken);

        IReadOnlyList<ActiveAlarmDto> dtos = activeAlarms
            .Select(e => new ActiveAlarmDto(e.Id.Value, e.Message, e.Severity.ToString(), e.RaisedAtUtc))
            .ToList();

        return Result<IReadOnlyList<ActiveAlarmDto>>.Success(dtos);
    }
}
