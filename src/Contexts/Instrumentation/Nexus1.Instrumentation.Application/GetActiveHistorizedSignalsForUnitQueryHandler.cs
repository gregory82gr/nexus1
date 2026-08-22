using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetActiveHistorizedSignalsForUnitQueryHandler(IActiveHistorizedSignalFinder finder)
    : IQueryHandler<GetActiveHistorizedSignalsForUnitQuery, IReadOnlyList<ActiveHistorizedSignalDto>>
{
    public async Task<Result<IReadOnlyList<ActiveHistorizedSignalDto>>> Handle(
        GetActiveHistorizedSignalsForUnitQuery query, CancellationToken cancellationToken)
    {
        var signals = await finder.GetByUnitCodeAsync(query.UnitCode, cancellationToken);
        return Result<IReadOnlyList<ActiveHistorizedSignalDto>>.Success(signals);
    }
}
