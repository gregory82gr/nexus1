namespace Nexus1.RootCause.Domain.Grounding;

/// <summary>
/// The tamper-evident audit chain (From Flood to Cause, App C.3 / Listing 9.5
/// H10): each conclusion sealed as Hash = SHA-256(PrevHash + Payload). The
/// first entry chains from a fixed genesis PrevHash. This is the same
/// hash-chain shape the Angular port's Ch.30 audit-log built client-side --
/// here it is the server-side grounding-store version. Named AuditEntry, not
/// Audit, to avoid colliding with the existing Audit bounded context.
/// </summary>
public sealed class AuditEntry
{
    public long Seq { get; init; }

    public DateTime TimestampUtc { get; init; }

    /// <summary>Sealed payload: inputs, context, verdict-or-abstention, citations.</summary>
    public required string Payload { get; init; }

    public required string PrevHash { get; init; }

    public required string Hash { get; init; }
}
