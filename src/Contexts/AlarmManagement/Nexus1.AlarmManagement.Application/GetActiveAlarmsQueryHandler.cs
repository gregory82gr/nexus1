using Nexus1.BuildingBlocks.Application;

namespace Nexus1.AlarmManagement.Application;

public sealed class GetActiveAlarmsQueryHandler(IAlarmEventFinder eventFinder)
    : IQueryHandler<GetActiveAlarmsQuery, IReadOnlyList<ActiveAlarmSummaryDto>>
{
    public async Task<Result<IReadOnlyList<ActiveAlarmSummaryDto>>> Handle(
        GetActiveAlarmsQuery query, CancellationToken cancellationToken)
    {
        var activeAlarms = await eventFinder.GetAllActiveAsync(cancellationToken);

        IReadOnlyList<ActiveAlarmSummaryDto> dtos = activeAlarms
            .Select(e => new ActiveAlarmSummaryDto(e.Id.Value, e.UnitId.Value, e.Message, e.Severity.ToString(), e.RaisedAtUtc))
            .ToList();

        return Result<IReadOnlyList<ActiveAlarmSummaryDto>>.Success(dtos);
    }
}
