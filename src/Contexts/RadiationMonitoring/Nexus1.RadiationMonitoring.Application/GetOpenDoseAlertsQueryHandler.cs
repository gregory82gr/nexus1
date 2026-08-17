using Nexus1.BuildingBlocks.Application;

namespace Nexus1.RadiationMonitoring.Application;

public sealed class GetOpenDoseAlertsQueryHandler(IOpenDoseAlertsFinder finder)
    : IQueryHandler<GetOpenDoseAlertsQuery, IReadOnlyList<OpenDoseAlertDto>>
{
    public async Task<Result<IReadOnlyList<OpenDoseAlertDto>>> Handle(GetOpenDoseAlertsQuery query, CancellationToken cancellationToken)
    {
        var alerts = await finder.GetOpenDoseAlertsAsync(cancellationToken);
        return Result<IReadOnlyList<OpenDoseAlertDto>>.Success(alerts);
    }
}
