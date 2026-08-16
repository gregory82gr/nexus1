namespace Nexus1.Instrumentation.Application;

public interface ILatestMeasurementFinder
{
    /// <summary>Atlas C.5.8 query 2: WHERE s.Tag = @tag, ORDER BY m.TimestampUtc DESC, TOP (count).</summary>
    Task<IReadOnlyList<LatestMeasurementDto>> GetLatestByTagAsync(string tag, int count, CancellationToken cancellationToken);
}
