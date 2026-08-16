using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

public sealed class TraceModelVariableToSignalQueryHandler(IModelVariableSignalTraceFinder finder)
    : IQueryHandler<TraceModelVariableToSignalQuery, IReadOnlyList<ModelVariableSignalTraceDto>>
{
    public async Task<Result<IReadOnlyList<ModelVariableSignalTraceDto>>> Handle(
        TraceModelVariableToSignalQuery query, CancellationToken cancellationToken)
    {
        var trace = await finder.GetByTwinCodeAsync(query.TwinCode, cancellationToken);
        return Result<IReadOnlyList<ModelVariableSignalTraceDto>>.Success(trace);
    }
}
