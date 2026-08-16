using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Instrumentation.Application;

/// <summary>Atlas C.5.8 query 4, verbatim: tag -> current SignalMapping (EffectiveToUtc IS NULL) -> AcquisitionPoint -> AcquisitionConnection -> DataAcquisitionNode.</summary>
public sealed record GetAcquisitionPathForTagQuery(string Tag) : IQuery<IReadOnlyList<AcquisitionPathDto>>;
