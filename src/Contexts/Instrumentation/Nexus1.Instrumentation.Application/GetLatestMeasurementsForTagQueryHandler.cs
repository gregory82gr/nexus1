using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

public sealed class GetLatestMeasurementsForTagQueryHandler(ILatestMeasurementFinder finder)
    : IQueryHandler<GetLatestMeasurementsForTagQuery, IReadOnlyList<LatestMeasurementDto>>
{
    public async Task<Result<IReadOnlyList<LatestMeasurementDto>>> Handle(
        GetLatestMeasurementsForTagQuery query, CancellationToken cancellationToken)
    {
        var measurements = await finder.GetLatestByTagAsync(query.Tag, query.Count, cancellationToken);
        return Result<IReadOnlyList<LatestMeasurementDto>>.Success(measurements);
    }
}
