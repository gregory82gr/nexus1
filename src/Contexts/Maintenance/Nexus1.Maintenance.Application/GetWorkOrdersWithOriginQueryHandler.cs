using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Maintenance.Application;

public sealed class GetWorkOrdersWithOriginQueryHandler(IWorkOrdersWithOriginFinder finder)
    : IQueryHandler<GetWorkOrdersWithOriginQuery, IReadOnlyList<WorkOrderWithOriginDto>>
{
    public async Task<Result<IReadOnlyList<WorkOrderWithOriginDto>>> Handle(GetWorkOrdersWithOriginQuery query, CancellationToken cancellationToken)
    {
        var workOrders = await finder.GetWorkOrdersWithOriginAsync(cancellationToken);
        return Result<IReadOnlyList<WorkOrderWithOriginDto>>.Success(workOrders);
    }
}
