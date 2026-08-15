namespace Nexus1.AlarmManagement.Domain;

/// <summary>
/// Payload shape invented per ADR-004 — the book names this event but never
/// defines its fields anywhere, even in its own handler-parameter usage.
/// </summary>
public sealed record AlarmFloodDetected(AlarmFloodId AlarmFloodId, UnitId UnitId, DateTime StartedAtUtc);
