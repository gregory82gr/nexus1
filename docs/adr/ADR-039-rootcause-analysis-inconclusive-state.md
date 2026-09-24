# ADR-039: RootCauseAnalysis `Inconclusive` terminal state

## Status

Accepted. Extends ADR-005 (the RootCauseAnalysis aggregate, Open/Closed) and ADR-012
(Reporting's projection of RootCause integration events).

## Context

The human-owned `RootCauseAnalysis` lifecycle has exactly two states, `Open` and
`Closed` ([AnalysisStatus](../../src/Contexts/RootCause/Nexus1.RootCause.Domain/AnalysisStatus.cs)),
and `Close()` **requires** a non-empty verdict, at least one hypothesis with evidence,
and not-all-hypotheses-rejected ([RootCauseAnalysis.Close](../../src/Contexts/RootCause/Nexus1.RootCause.Domain/RootCauseAnalysis.cs)).

There is therefore **no way for an investigator to terminally record "investigated,
cannot conclude."** A case that cannot reach a supported verdict can only stay `Open`
forever. That is a real, pre-existing expressiveness gap in the human lifecycle,
**independent of any specific incident.** This ADR closes that gap.

This is explicitly **not** "EVT-2026-0420 preparation." 0420 (the QA-escape) — its seed
data, a provenance-only retrieval path in the DiagnosisRun engine, and whether Audit
and Compliance should consume an inconclusive outcome — all remain **deferred, named
gaps** (see the prior 0420 feasibility investigation). This slice builds only the
mechanism: a third terminal state on the human aggregate and its honest propagation to
the read model.

## Decision

### 1. `Inconclusive`, a third terminal state

Add `AnalysisStatus.Inconclusive`. Named for the *outcome* (no conclusion reached),
symmetric with Open/Closed, and consistent with the DiagnosisRun world's existing
`abstained` / "inconclusive is a first-class outcome, not an error" vocabulary.
(`ReferredForReview` was rejected: it presupposes a review-routing mechanism that does
not exist.)

New transition `MarkInconclusive(reason, decidedBy, decidedAtUtc)`:
- **Invariant: reason-only.** Requires a non-empty `reason`. Does **not** require any
  hypothesis or evidence — the whole point is to record "could not conclude," which
  includes "could not even form a supported hypothesis." Requiring evidence would
  recreate the exact trap that leaves such cases stuck `Open`. (Alternative considered:
  require ≥1 hypothesis; rejected as dishonest for a genuinely blind case.)
- **`Verdict` stays null** — the structural distinction from `Closed`. The aggregate
  records `Reason`/`DecidedBy`/`DecidedAtUtc` (mirroring `Verdict`/`ClosedBy`/`ClosedAtUtc`).
- **Terminal/immutable** via the existing `EnsureOpen()` guard — any state other than
  `Open` blocks every mutator, so `Inconclusive` is as immutable as `Closed`. The
  guard's message is generalized from "Closed cases cannot be changed" to a
  finalized-case wording accurate for both terminal states.

### 2. New domain + integration events

- Domain event `RootCauseAnalysisMarkedInconclusive(AnalysisId, Reason, DecidedAtUtc)`
  (mirrors `RootCauseAnalysisClosed`).
- Integration event `RootCauseCaseInconclusiveV1(AnalysisId, UnitId, AlarmFloodId,
  Reason, DecidedAtUtc)` in `Nexus1.Contracts.RootCause` — same "adapted, not frozen"
  reduction as `RootCauseVerdictIssuedV1`, deliberately with **no `Verdict` field**;
  it carries `Reason` instead, so "no verdict" is encoded structurally (this project's
  "Inconclusive is never a silent Pass").
- Published through the **existing transactional outbox**, via a new
  `MarkAnalysisInconclusiveCommand` + handler parallel to `CloseAnalysisCommandHandler`
  (load → `MarkInconclusive` → `outboxWriter.Enqueue` → one `SaveChangesAsync`).
  Coordinates per ADR-008: routing key `root-cause.root-cause-case-inconclusive.v1`,
  eventType `nexus1.root-cause.root-cause-case-inconclusive.v1`.
- **Deferred nicety (named, not silent):** no `alarm-to-inconclusive` workflow-duration
  metric this slice (would need a new reviewed `MetricLabelPolicy` vocabulary term).

### 3. Reporting projection extension (regression prevention, not just a feature)

Reporting binds the **wildcard `root-cause.#`** ([ReportingConsumerBackgroundService](../../src/Contexts/Reporting/Nexus1.Reporting.Infrastructure/Messaging/ReportingConsumerBackgroundService.cs)),
so `RootCauseCaseInconclusiveV1` **will be delivered** to Reporting's queue and — with
the handler's `default:` quarantining unknown event types as `unsupported-contract`
(NackNoRequeue → poison) — **would be poisoned on arrival** without handling. So the
following is regression prevention:
- `ReportingCaseStatus` gains `Inconclusive`.
- `RootCauseCaseSummary.ApplyInconclusive(reason, decidedAtUtc, appliedAtUtc, messageId)`,
  idempotency-guarded the same way as `ApplyVerdictIssued` (a case already finalized is
  a no-op). New columns `Reason` and a generalized `FinalizedAtUtc` (both `Closed` and
  `Inconclusive` are terminal, so a single "finalized when" timestamp reads more
  consistently than an `InconclusiveAtUtc`).
- An explicit new `case` in `ReportingProjectionMessageHandler` routing the new event
  type to `ApplyInconclusiveAsync` (mandatory — not a fall-through).
- A new parallel `PendingInconclusive` buffer mirroring `PendingVerdict` (an
  inconclusive event can arrive before its `CaseOpened`, exactly as a verdict can;
  `ApplyOpenedAsync` resolves either pending kind when the row appears). The working
  `PendingVerdict` path is left untouched.
- A Reporting migration for the new status/columns/table.

### 4. Audit and Compliance: unchanged, a stated reversible deferral

`AuditConsumerBackgroundService` and `ComplianceConsumerBackgroundService` bind the
**specific** verdict routing key `root-cause.root-cause-verdict-issued.v1`, not the
wildcard, so the new event is **never delivered** to them — a genuine safe no-op, with
no poison risk (unlike Reporting).

**Named gap (reversible, not permanent):** a case marked `Inconclusive` is therefore
**not audited and not compliance-reviewed** today — even though a QA-escape-type case
is precisely a records/compliance matter. Whether Audit and/or Compliance should
consume inconclusive outcomes is a deferred decision. **Reversal condition:** revisit
when a real QA-escape case (0420) is built, or when policy decides inconclusive cases
must be audited/reviewed — at which point their bindings + a reducer would be added.

### 5. Frontend: defensive only

The AI Diagnostics screen reads Reporting's projection (`GetCaseSummariesForUnitQuery`),
which serializes `Status.ToString()`, so `"Inconclusive"` flows through unchanged. The
screen renders `{{ c.status }}` verbatim and applies `.open`/`.verdict` classes only by
explicit equality — so an `Inconclusive` status **already degrades gracefully** (renders
the text, no crash, no mislabelled pill). This slice adds only a **defensive test**
asserting that; no `.inconclusive` styling or label feature work (deferred).

## Consequences

- Investigators (and the auto-open path, later) can record a terminal "cannot conclude"
  outcome that flows honestly to the read model, instead of a case stuck `Open`.
- Reporting correctly projects and displays the third status; the wildcard-delivery
  poison regression is prevented.
- Audit/Compliance intentionally do not react yet (named gap).
- No change to the DiagnosisRun/grounding world (0418/0419), no 0420 content, no
  pipeline changes.

## Rejected alternatives

- **`ReferredForReview`** — presupposes unbuilt review routing; `Inconclusive` states
  the outcome honestly.
- **Requiring evidence/hypotheses to mark inconclusive** — recreates the stuck-Open trap.
- **Reusing `RootCauseVerdictIssuedV1` with an empty verdict** — would fabricate a
  verdict; violates "Inconclusive is never a silent Pass."
- **Generalizing `PendingVerdict`** instead of a parallel `PendingInconclusive` —
  rejected to leave the working verdict path untouched (duplicate-until-proven).
- **`InconclusiveAtUtc` column** — `FinalizedAtUtc` reads more consistently across
  terminal states.

## Reversal condition

If Audit/Compliance participation is decided (§4), or if a later slice needs the
`Inconclusive` status surfaced with real UI styling, extend then. The state itself is
additive and terminal; removing it later would be a breaking contract change, so it is
introduced only because the human-lifecycle gap is real now.

## Evidence required

- Component (LocalDB): open a generic case → `MarkAnalysisInconclusive` → `Status =
  Inconclusive`, `Verdict = null`, `Reason` set; a subsequent `AddHypothesis`/`Close`
  fails with the generalized finalized-case error; the domain event fires; the outbox
  carries `RootCauseCaseInconclusiveV1` in the same transaction.
- Reporting (LocalDB): in-order (opened → inconclusive) projects `Inconclusive` +
  `Reason`; out-of-order (inconclusive → opened) buffers in `PendingInconclusive` and
  resolves; the existing verdict path and the unsupported-contract quarantine still hold.
- Real broker → Reporting: an inconclusive event delivered over the wildcard binding is
  projected (not poisoned); zero DLQ; inbox receipt recorded.
- Audit/Compliance binding inspection confirms they never receive it.
- Frontend defensive test: `Inconclusive` renders as text with no `.open`/`.verdict`
  class and no crash.

See `artifacts/evidence/2026-09-24-rootcause-inconclusive-state.md`.
