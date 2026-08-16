using Nexus1.BuildingBlocks.Domain;

namespace Nexus1.Instrumentation.Domain;

/// <summary>
/// Interval record explaining when a signal became stale, bad, substituted
/// or simulated (atlas C.5.4.7) — directly serving C.5.9's own "a value
/// without quality is not evidence" emphasis. Open/close lifecycle: Create
/// opens the event (EndedAtUtc == null); Close ends it and re-validates
/// CK_Instrumentation_SignalQualityEvent_Time.
/// </summary>
public sealed class SignalQualityEvent : Entity<SignalQualityEventId>, IAggregateRoot
{
    private SignalQualityEvent(
        SignalQualityEventId id, SignalId signalId, SignalQualityId signalQualityId, DateTime startedAtUtc,
        DateTime? endedAtUtc, string? reasonCode, string? notes, DateTime createdAtUtc)
        : base(id)
    {
        SignalId = signalId;
        SignalQualityId = signalQualityId;
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        ReasonCode = reasonCode;
        Notes = notes;
        CreatedAtUtc = createdAtUtc;
    }

    public SignalId SignalId { get; }

    public SignalQualityId SignalQualityId { get; }

    public DateTime StartedAtUtc { get; }

    public DateTime? EndedAtUtc { get; private set; }

    public string? ReasonCode { get; private set; }

    public string? Notes { get; }

    public DateTime CreatedAtUtc { get; }

    /// <summary>Opens a new quality event. EndedAtUtc starts unset — use <see cref="Close"/> to close it out.</summary>
    public static SignalQualityEvent Create(
        SignalQualityEventId id, SignalId signalId, SignalQualityId signalQualityId, DateTime startedAtUtc,
        DateTime createdAtUtc, string? reasonCode = null, string? notes = null)
    {
        return new SignalQualityEvent(id, signalId, signalQualityId, startedAtUtc, null, reasonCode, notes, createdAtUtc);
    }

    /// <summary>Closes an open event — re-validates CK_Instrumentation_SignalQualityEvent_Time (EndedAtUtc must be later than StartedAtUtc). A non-null reasonCode overwrites the reason recorded at open.</summary>
    public void Close(DateTime endedAtUtc, string? reasonCode = null)
    {
        if (endedAtUtc <= StartedAtUtc)
        {
            throw new ArgumentException("EndedAtUtc must be later than StartedAtUtc.", nameof(endedAtUtc));
        }

        EndedAtUtc = endedAtUtc;

        if (reasonCode is not null)
        {
            ReasonCode = reasonCode;
        }
    }
}
