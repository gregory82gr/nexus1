namespace Nexus1.Maintenance.Application;

public interface IActiveDegradationCasesFinder
{
    Task<IReadOnlyList<ActiveDegradationCaseDto>> GetActiveDegradationCasesAsync(CancellationToken cancellationToken);
}
