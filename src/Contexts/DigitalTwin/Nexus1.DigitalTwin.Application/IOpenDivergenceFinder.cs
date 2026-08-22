namespace Nexus1.DigitalTwin.Application;

public interface IOpenDivergenceFinder
{
    Task<IReadOnlyList<OpenDivergenceDto>> GetOpenDivergencesAsync(CancellationToken cancellationToken);
}
