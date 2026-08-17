using Nexus1.BuildingBlocks.Application;

namespace Nexus1.EventManagement.Application;

public sealed class GetOpenIncidentActionsQueryHandler(IOpenIncidentActionsFinder finder)
    : IQueryHandler<GetOpenIncidentActionsQuery, IReadOnlyList<OpenIncidentActionDto>>
{
    public async Task<Result<IReadOnlyList<OpenIncidentActionDto>>> Handle(GetOpenIncidentActionsQuery query, CancellationToken cancellationToken)
    {
        var actions = await finder.GetOpenIncidentActionsAsync(cancellationToken);
        return Result<IReadOnlyList<OpenIncidentActionDto>>.Success(actions);
    }
}
