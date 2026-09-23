namespace Nexus1.RootCause.Application.Diagnosis;

/// <summary>
/// The identity of the one fixed incident this walking skeleton diagnoses,
/// EVT-2026-0418 (ADR-032/ADR-034) -- the single source of truth for the incident
/// context. It lives in the Application layer because the use case
/// (RunFixedIncidentDiagnosis) builds an <see cref="IncidentContext"/> from it and
/// cannot reference the Infrastructure seed (dependency law). GroundingSeed
/// delegates to these values, so the seeded rows and the runnable context can
/// never drift apart.
/// </summary>
public static class FixedIncident
{
    public const string IncidentId = "EVT-2026-0418";

    public const int UnitId = 1;

    /// <summary>Physical onset of the origin fault (historian anchor), chosen so the trip onset lands on the book's 17:12:14.</summary>
    public static readonly DateTime FloodStartUtc = new(2026, 4, 18, 17, 9, 52, DateTimeKind.Utc);

    /// <summary>
    /// The component ids that raised at least one alarm in the flood -- the
    /// fourteen-alarm set across nine nodes. FT-7 (id 10) raised none (it was
    /// correlation-flagged and cross-checked healthy), so it is absent. See
    /// GroundingSeed for the id-to-tag mapping (1=FV-104 … 9=4kV bus).
    /// </summary>
    public static readonly IReadOnlyList<int> AlarmedComponentIds = [1, 2, 3, 4, 5, 6, 7, 8, 9];

    /// <summary>The runnable context for the fixed incident.</summary>
    public static IncidentContext Context() => new(IncidentId, UnitId, AlarmedComponentIds, FloodStartUtc);
}
