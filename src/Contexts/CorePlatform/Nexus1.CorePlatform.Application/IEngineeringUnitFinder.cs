using Nexus1.CorePlatform.Domain;

namespace Nexus1.CorePlatform.Application;

public interface IEngineeringUnitFinder
{
    /// <summary>Matches the atlas's own C.1.8 verification query: active units ordered by QuantityType, DisplayOrder, Symbol.</summary>
    Task<IReadOnlyList<EngineeringUnit>> GetActiveAsync(CancellationToken cancellationToken);
}
