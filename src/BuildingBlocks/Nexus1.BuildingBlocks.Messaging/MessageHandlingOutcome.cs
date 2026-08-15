namespace Nexus1.BuildingBlocks.Messaging;

/// <summary>
/// Shared across every consumer's message handler (ADR-009/ADR-010) — the
/// three real dispositions a classified failure policy needs: Ack covers
/// both success and a retry ticket being recorded (ownership moves to that
/// consumer's RetryDispatcher, so the original delivery is done either way);
/// NackRequeue is reserved for the genuinely ambiguous case the book calls
/// "stop without acknowledgement" (ch.29 Guarantee Ledger 29-A) — a
/// concurrent-duplicate resolution that's still unresolved, not every
/// failure; NackNoRequeue is the retry-budget-exhausted poison path, routing
/// to the broker's nexus.dead safety net.
/// </summary>
public enum MessageHandlingOutcome
{
    Ack,
    NackRequeue,
    NackNoRequeue,
}
