namespace Nexus1.EventManagement.Application;

/// <summary>
/// Backs Incident's real, atlas-named invariant (ADR-022): at most one
/// Incident per OperationalEvent. OpenIncidentCommandHandler checks this
/// before insert so a duplicate open attempt surfaces a clear conflict,
/// rather than letting the database's own unique-index violation
/// (IncidentConfiguration.cs) be the only signal.
/// </summary>
public interface IIncidentExistenceFinder
{
    Task<bool> ExistsForOperationalEventAsync(long operationalEventId, CancellationToken cancellationToken);
}
