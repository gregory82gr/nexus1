using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetOpenWorkOrdersByUnitQueryHandler(IOpenWorkOrdersFinder finder)
    : IQueryHandler<GetOpenWorkOrdersByUnitQuery, IReadOnlyList<OpenWorkOrderDto>>
{
    public async Task<Result<IReadOnlyList<OpenWorkOrderDto>>> Handle(GetOpenWorkOrdersByUnitQuery query, CancellationToken cancellationToken)
    {
        var workOrders = await finder.GetOpenWorkOrdersAsync(cancellationToken);
        return Result<IReadOnlyList<OpenWorkOrderDto>>.Success(workOrders);
    }
}
