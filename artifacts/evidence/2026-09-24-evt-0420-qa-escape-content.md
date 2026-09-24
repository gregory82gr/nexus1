# Evidence — EVT-2026-0420 content, the QA-escape (ADR-040)

**Date:** 2026-09-24
**ADR:** ADR-040 (reusing ADR-039's Inconclusive mechanism)
**Scope:** Record the QA-escape (CR-7 / WV-318) honestly — a provenance-originated
`RootCauseAnalysis` with no flood, terminally `Inconclusive` with a narrative reason,
never a structured verdict. The engine's reach genuinely ends here (both routes 404).
No 0418/0419 changes, no new incident beyond 0420, no Audit/Compliance changes.

## Reading (b), and the honest model

0420 is recorded as `Inconclusive` (not Closed-with-verdict): the book says *"the engine
cannot solve it… say plainly where the reach of the system ends,"* the real detection was
an out-of-band provenance audit this system does not model (no waiver/provenance entity),
and 0420 never entered the flood-triggered pipeline (no alarm, no flood). The human
resolution lived outside this system's surfaces, so the system records only its own
boundary. CR-7/WV-318 are book-narrative in the reason text, not fabricated records.

## Built

| Piece | Location |
|---|---|
| ADR-040 | docs/adr/ADR-040-evt-0420-qa-escape-content.md |
| Provenance-originated open (`AlarmFloodId` optional; `OpenForProvenance`) | RootCause.Domain/RootCauseAnalysis.cs |
| `RootCauseAnalysisOpened` + `RootCauseCaseOpenedV1` + `RootCauseCaseInconclusiveV1` widened to `long?` AlarmFloodId | Domain + Contracts |
| `OpenProvenanceAnalysisCommand` + handler (+ DI) | RootCause.Application |
| Nullable AlarmFloodId EF mapping + migration (`AllowProvenanceOriginatedAnalysis`) | RootCause.Infrastructure |
| Reporting: `RootCauseCaseSummary.AlarmFloodId` `long?` (+ ApplyOpened, config, migration `AllowNullableAlarmFloodId`) | Reporting |
| `CaseSummaryDto` + `AnalysisDto` + frontend `RootCauseCase.alarmFloodId` widened to nullable | Reporting.Application / RootCause.Application / console |
| 0420 provisioning (open-provenance + MarkInconclusive at fixed id 20260420, idempotent, 2 outbox events) | RootCause.Host/Provisioning.cs |

The honest "no-flood, not-a-sentinel" model necessarily widened `AlarmFloodId` to nullable
through the two open/inconclusive events → Reporting projection → DTO → frontend type — a
**backward-compatible widening** (flood cases still carry their id; `RootCauseVerdictIssuedV1`
unchanged, verdicts are always flood-originated). Somewhat more than a one-line change, but
every touched point is a real widening, not a workaround.

## Gates

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8**.
- `Nexus1.RootCause.UnitTests` → **39/39** (35 + 4 new `IncidentRegistryTests` cases
  locking 0420's non-registration).
- `Nexus1.RootCause.ComponentTests` (deterministic, `!~Ollama`) → **61/61** (+1 provenance
  open→inconclusive workflow test: null flood, Inconclusive, no verdict, no hypotheses,
  both outbox events).
- `Nexus1.Reporting.ComponentTests` → **19/19** (+1 provenance null-flood projection test:
  Status=Inconclusive, `AlarmFloodId` NULL, reason has WV-318, zero poison).
- Frontend `npx jest ai-diagnostics` → **6/6** (DTO type widening, existing tests intact).
- Two clean additive migrations (both widen `AlarmFloodId` to nullable — no data loss).

## Engine-boundary proof (live, the book's core point)

Host + BFF up, via the BFF:
```
0420 diagnoses POST -> 404      0418 graph GET -> 200   (registry intact)
0420 graph GET      -> 404      0419 graph GET -> 200   (registry intact)
                                EVT-9999 graph -> 404   (control)
```
0420 is deliberately **not** in `IncidentRegistry`, so both engine routes 404 — this *is*
the evidence that the engine's reach ends here, not a gap. 0418/0419 still resolve.

## Provisioning + DB state (live LocalDB)

`dotnet run --project Nexus1.RootCause.Host -- provision` →
`"Provisioned EVT-2026-0420 QA-escape case as Inconclusive (AnalysisId 20260420, unit 1)."`
Direct SELECT of `RootCause.RootCauseAnalysis` id 20260420:
```
id        | UnitId | Status       | flood | Verdict | reason
20260420  | 1      | Inconclusive | NULL  | NULL    | has-WV-318
```
Genuinely flood-less (`AlarmFloodId` NULL, not a sentinel), no verdict, reason narrates the
QA escape. Outbox holds exactly **1** `root-cause.root-cause-case-inconclusive.v1` event
for it (plus its case-opened), enqueued in the provisioning transaction.

## Component proof (real LocalDB, no mocks)

- **RootCause workflow test:** open via the provenance path → `AlarmFloodId` null;
  MarkInconclusive → Inconclusive, Verdict null, reason contains WV-318, no hypotheses;
  the outbox carries both `case-opened` and `case-inconclusive` events.
- **Reporting projection test (the 0420 shape):** a null-flood CaseOpened + null-flood
  Inconclusive project to `Status=Inconclusive`, `AlarmFloodId` NULL, reason has WV-318,
  Verdict null, **zero PoisonMessages** — null flows end-to-end through the read model.

## Real broker (RabbitMQ management API, the actual 0420 payload)

Published the real 0420 inconclusive payload (`analysisId:20260420, alarmFloodId:null`) to
`nexus.events` with key `root-cause.root-cause-case-inconclusive.v1`:
- **`routed: true`** — delivered to a bound queue.
- `reporting.integration-events.v1` binds `root-cause.#` → **receives it**.
- `audit.root-cause-verdicts.v1` and `compliance.root-cause-verdicts.v1` bind **only**
  `root-cause.root-cause-verdict-issued.v1` → **do not receive it** (safe no-op, re-confirmed
  with real 0420 data). The probe was purged from the reporting queue afterwards.

## No regression

0418 → FV-104 and 0419 → BKR-2A are unchanged (deterministic GroundingDiagnosis component
tests, all green; their graph routes return 200 live). The existing flood-originated open
path is unchanged (`AlarmFloodMessageHandlerTests` green). `RootCauseVerdictIssuedV1` is
untouched.

## Live render — the full chain, end to end (browser → BFF → Reporting projection)

Requested follow-up: an actual live render of the AI Diagnostics screen showing the 0420
Inconclusive case. This ran the **full chain end to end**, not the two-halves substitute:

1. Reset the 0420 outbox pair to unprocessed (undoing an earlier broker-probe purge).
2. RabbitMQ + **RootCause.Host** (its outbox relay re-published the 0420 CaseOpened +
   Inconclusive events) + **Nexus1.ModularRuntime** (its real Reporting consumer).
3. The consumer projected 0420 into ReportingDb — verified by direct SELECT:
   `20260420 | Inconclusive | AlarmFloodId NULL | reason has WV-318`; the outbox pair is now
   `processed`.
4. BFF (`BffContexts` incl. `Reporting`) + `ng serve`. `GET /api/v1/reporting/units/1`
   returns `{"caseId":20260420,"unitId":1,"alarmFloodId":null,"status":"Inconclusive","verdict":null,...}`.
5. Browser → `http://localhost:4200/ai` → the **Investigation Cases** panel renders the case:
   **"Inconclusive · opened 2026-04-20T09:00:00 · investigation in progress, no verdict yet."**

Screenshot: `artifacts/evidence/screenshots/evt-0420-ai-diagnostics-inconclusive.png`.
Graceful degradation confirmed programmatically — the three `.case-status` pills are
`[VerdictIssued(verdict), VerdictIssued(verdict), Inconclusive(neither open nor verdict)]`:
the new status renders as plain text with **no crash and no mislabel** (it does not borrow the
Open or VerdictIssued styling). The body line "investigation in progress, no verdict yet" is
the existing template's generic non-verdict fallback; a dedicated Inconclusive label/reason
rendering remains the deferred UI follow-on (ADR-040 scope) — the `Reason` is not in the
read DTO.

So the ADR-039-style "two halves" caveat no longer applies here: the outbox-relay → broker →
Reporting-consumer → ReportingDb → BFF → browser chain was exercised in full for this render.

## Honest boundaries / what was NOT run
- **Audit/Compliance participation stays deferred** to the named follow-on (ADR-039's reversal
  condition is now met); they remain a safe no-op (verified: they never receive the event).
- Migrations applied to the shared dev DBs this time (needed for provisioning): the ADR-039
  pair (previously deferred) plus the ADR-040 pair are now applied.
- Dev stack stopped (Host, BFF, RabbitMQ down); Ollama left running (not started by this slice).
