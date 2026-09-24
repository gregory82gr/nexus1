# ADR-040: EVT-2026-0420 content — the QA-escape recorded as Inconclusive

## Status

Accepted. Content slice on top of ADR-039 (the Inconclusive terminal state) and ADR-005
(the RootCauseAnalysis aggregate). Reuses ADR-039's mechanism; adds no new event type.

## Context

`From Flood to Cause` Ch.10's third case study, EVT-2026-0420: a non-conforming control
rod **CR-7** entered service through a mis-dispositioned quality waiver **WV-318**. It
*"raised no alarm… left no telemetry signature for months… Two corners of the grounding
triangle were simply dark. It was exposed only when the provenance records were examined…
The engine that thrives on floods is blind to a fault that makes no noise, and the honest
response is not to pretend otherwise but to say plainly where the reach of the system
ends."*

Two structural facts about this repo make 0420 different from 0418/0419 (confirmed by the
prior feasibility investigation):
- 0420 has **no flood, no graph, no telemetry**, so it produces nothing the DiagnosisRun
  engine (walker/corroborator/retriever) can act on.
- There is **no waiver / provenance / disposition / non-conformance entity** anywhere in
  the domain — the system does not model the out-of-band audit that actually found it.

## Decision

### Reading (b): 0420 records as Inconclusive, never a structured Verdict

0420 is recorded in the **human `RootCauseAnalysis` world** as terminally
**`Inconclusive`** (ADR-039), with a **narrative reason** naming CR-7 / WV-318 / no-alarm
/ no-telemetry / exposed-only-by-provenance-audit. It is **not** recorded as
Closed-with-a-Verdict.

Why (b) over "AI inconclusive vs human Closed-with-verdict":
- The book's lesson is a boundary statement — "say plainly where the reach of the system
  ends." A structured `Verdict` would assert the system's pipeline solved the case.
- The *real* detection was an **out-of-band provenance/QA audit**, which this system does
  not model (no waiver/provenance entity). A structured verdict would point at nothing real.
- 0420 never entered the flood-triggered investigation pipeline (no alarm, no flood), so a
  Closed human case would fabricate the trigger the book says was absent.

The honest record is therefore: `Inconclusive` + a reason that states what happened and
where the reach ended. The human resolution existed **outside** this system's modeled
surfaces, so the system does not synthesize it as a structured conclusion.

### Provenance-originated open — a small domain change, not a sentinel

`RootCauseAnalysis` was flood-anchored: `Open` required a non-null `AlarmFloodId`. 0420
has no flood. Rather than a fake/sentinel id, this adds an honest **provenance-originated
open path**: `AlarmFloodId` becomes **optional** on the aggregate
(`OpenForProvenance(id, unitId, openedBy, openedAtUtc)` opens a case with no flood), and
the `RootCauseAnalysisOpened` domain event and the two integration events that carry it
(`RootCauseCaseOpenedV1`, `RootCauseCaseInconclusiveV1`) widen `AlarmFloodId` from `long`
to **`long?`** — a backward-compatible widening (flood cases still carry their id; a
provenance case carries null). Reporting's `RootCauseCaseSummary.AlarmFloodId` and the
`CaseSummaryDto`/frontend type widen to nullable to match. **No new event type**;
`RootCauseVerdictIssuedV1` is unchanged (a verdict is always flood-originated).

This is the honest consequence of "cases can originate from a provenance audit, not only
floods," chosen over a sentinel id per the standing honest-modeling preference. It is
somewhat more than a one-line change because `AlarmFloodId` is carried on the wire and in
the read model — but every touched point is a real widening, not a workaround.

### 0420 is content-only for the engine — the 404s are the evidence

0420 is **not** added to `IncidentRegistry`. `POST …/incidents/EVT-2026-0420/diagnoses`
and `GET …/EVT-2026-0420/graph` therefore **stay 404** — this *is* the honest evidence
that the engine's reach ends here, not a gap to close. No DiagnosisRun / Component / Edge
/ Historian / Corpus rows are seeded for 0420, and the walk-gate-before-retrieval runner
structure is untouched (unnecessary for 0420, which never touches the engine).

### Provisioning, visible in the console

Like the 0418/0419 grounding fixture, 0420 is created by an explicit dev **provisioning**
step (open via the provenance path + `MarkInconclusive`, at a fixed AnalysisId,
idempotent), so it is real and visible in the console via Reporting's projection.

### Audit/Compliance: the named follow-on

ADR-039 deferred Audit/Compliance participation with the reversal condition "when a real
QA-escape case is built." That condition is now **formally met by this slice**, but
extending Audit and Compliance to consume `RootCauseCaseInconclusiveV1` (new bindings,
reducers, a review/audit-record shape for a reason-not-verdict outcome, tests, evidence)
is real cross-context work in two more contexts. To keep this slice tight and honor the
one-checkpoint-at-a-time discipline, that extension is the **explicit next follow-on
slice**, not bundled here. Audit and Compliance remain untouched and, because they bind
only the verdict routing key, never receive the inconclusive event (a safe no-op).

## Consequences

- 0420 exists as an honest `Inconclusive` case, visible in the console's investigation
  history, with the engine boundary observable as the two 404s.
- The system can now open cases that did not originate from a flood.
- 0418/0419 and the DiagnosisRun world are untouched.
- Audit/Compliance still do not see inconclusive outcomes — the named follow-on.

## Scope boundary

No changes to 0418/0419; no new incident beyond 0420; no frontend UI beyond the
already-graceful Inconclusive rendering (type widening only); no Audit/Compliance changes;
no DiagnosisRun/retrieval restructure; no fabricated waiver/provenance structured records
(CR-7/WV-318 are book-narrative in the reason text only).

## Rejected alternatives

- **Sentinel/fake AlarmFloodId** — rejected; the honest model allows a genuinely flood-less case.
- **Closed-with-a-Verdict for 0420** — rejected (reading b above); pretends the pipeline solved it.
- **Seeding a DiagnosisRun/graph for 0420** — rejected; the engine has nothing to walk; the 404s are the truth.
- **Bundling Audit/Compliance participation here** — rejected; a scoped follow-on.
- **A new inconclusive-for-provenance event type** — rejected; reuse ADR-039's event, widen AlarmFloodId.

## Reversal condition

If a real provenance/QA-disposition sector is later modeled, 0420 could carry structured
provenance records (and possibly a human verdict from that process). Until then,
Inconclusive + narrative reason is the honest record. The Audit/Compliance follow-on is
already named.

## Evidence required

- Engine boundary: both routes 404 for EVT-2026-0420.
- Component (LocalDB): the provisioned case is Inconclusive, Verdict null, reason narrates
  CR-7/WV-318/provenance-only, no hypotheses/evidence; the outbox carries CaseOpened
  (null flood) and RootCauseCaseInconclusiveV1 for it.
- Reporting projection: 0420 projects as Inconclusive + Reason, per-unit query.
- Real broker (management API) with the actual 0420 payload: routes to the reporting
  queue, zero DLQ; Audit/Compliance queues still don't receive it.
- No regression: 0418 → FV-104, 0419 → BKR-2A unchanged; existing flood-originated open
  path unchanged.

See `artifacts/evidence/2026-09-24-evt-0420-qa-escape-content.md`.
