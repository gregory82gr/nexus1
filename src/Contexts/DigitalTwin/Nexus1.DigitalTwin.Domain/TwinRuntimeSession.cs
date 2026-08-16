using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.DigitalTwin.Domain;

/// <summary>
/// Runtime instance of a twin model version, including mode, host and time
/// window (atlas C.6.2). Required by TwinSnapshot — without it, a snapshot
/// has no valid parent. Real invariant: EndedAtUtc, when set, must be later
/// than StartedAtUtc (CK_DigitalTwin_TwinRuntimeSession_TimeRange).
/// Open/close lifecycle mirrors SignalQualityEvent's pattern: Create opens
/// the session (EndedAtUtc == null); End closes it.
///
/// StartedByUserId is a Security.ApplicationUser passport int — no enforced
/// FK (ADR-020, SecurityDb is a separate physical database).
/// </summary>
public sealed class TwinRuntimeSession : Entity<TwinRuntimeSessionId>, IAggregateRoot
{
    private TwinRuntimeSession(
        TwinRuntimeSessionId id, TwinModelVersionId twinModelVersionId, int? startedByUserId, string sessionCode,
        string runtimeMode, string? hostName, DateTime startedAtUtc, DateTime? endedAtUtc, bool isReadOnly,
        DateTime createdAtUtc)
        : base(id)
    {
        TwinModelVersionId = twinModelVersionId;
        StartedByUserId = startedByUserId;
        SessionCode = sessionCode;
        RuntimeMode = runtimeMode;
        HostName = hostName;
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        IsReadOnly = isReadOnly;
        CreatedAtUtc = createdAtUtc;
    }

    public TwinModelVersionId TwinModelVersionId { get; }

    /// <summary>Security.ApplicationUser passport id — no enforced FK (ADR-020).</summary>
    public int? StartedByUserId { get; }

    public string SessionCode { get; }

    public string RuntimeMode { get; }

    public string? HostName { get; }

    public DateTime StartedAtUtc { get; }

    public DateTime? EndedAtUtc { get; private set; }

    public bool IsReadOnly { get; }

    public DateTime CreatedAtUtc { get; }

    /// <summary>Opens a new runtime session. EndedAtUtc starts unset — use <see cref="End"/> to close it out.</summary>
    public static TwinRuntimeSession Create(
        TwinRuntimeSessionId id, TwinModelVersionId twinModelVersionId, string sessionCode, string runtimeMode,
        DateTime startedAtUtc, DateTime createdAtUtc, int? startedByUserId = null, string? hostName = null,
        bool isReadOnly = true)
    {
        if (string.IsNullOrWhiteSpace(sessionCode))
        {
            throw new ArgumentException("TwinRuntimeSession session code must not be empty.", nameof(sessionCode));
        }

        if (string.IsNullOrWhiteSpace(runtimeMode))
        {
            throw new ArgumentException("TwinRuntimeSession runtime mode must not be empty.", nameof(runtimeMode));
        }

        return new TwinRuntimeSession(
            id, twinModelVersionId, startedByUserId, sessionCode, runtimeMode, hostName, startedAtUtc, null,
            isReadOnly, createdAtUtc);
    }

    /// <summary>Closes an open session — re-validates CK_DigitalTwin_TwinRuntimeSession_TimeRange (EndedAtUtc must be later than StartedAtUtc).</summary>
    public void End(DateTime endedAtUtc)
    {
        if (endedAtUtc <= StartedAtUtc)
        {
            throw new ArgumentException(
                "EndedAtUtc must be later than StartedAtUtc (CK_DigitalTwin_TwinRuntimeSession_TimeRange).",
                nameof(endedAtUtc));
        }

        EndedAtUtc = endedAtUtc;
    }
}
