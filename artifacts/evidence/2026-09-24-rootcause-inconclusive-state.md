# Evidence — RootCauseAnalysis `Inconclusive` terminal state (ADR-039)

**Date:** 2026-09-24
**ADR:** ADR-039
**Scope:** Close the human-lifecycle gap where a case could only stay `Open` or
`Close()` **with a verdict** — add a third terminal state, `Inconclusive`
("investigated, cannot conclude"), and propagate it honestly to Reporting's read model.
No 0420 data, no provenance corpus, no pipeline changes, no Audit/Compliance changes.

## Built

| Piece | Location |
|---|---|
| ADR-039 | docs/adr/ADR-039-rootcause-analysis-inconclusive-state.md |
| `AnalysisStatus.Inconclusive` + `MarkInconclusive(reason, decidedBy, decidedAtUtc)`; guard message generalized | RootCause.Domain/RootCauseAnalysis.cs, AnalysisStatus.cs |
| Domain event `RootCauseAnalysisMarkedInconclusive` | RootCause.Domain |
| Integration event `RootCauseCaseInconclusiveV1` (no Verdict; carries Reason) | Nexus1.Contracts.RootCause |
| `MarkAnalysisInconclusiveCommand` + handler (outbox, one transaction) + DI | RootCause.Application |
| Aggregate EF mapping + migration (InconclusiveReason/DecidedBy/DecidedAtUtc) | RootCause.Infrastructure |
| `ReportingCaseStatus.Inconclusive`; `ApplyInconclusive` + `Reason`/`FinalizedAtUtc` | Reporting.Domain |
| Projection handler: mandatory allowlist case + `ApplyInconclusiveAsync` + pending resolution | Reporting.Infrastructure/Messaging |
| `PendingInconclusive` (+ config, DbSet) mirroring PendingVerdict; migration | Reporting.Infrastructure |
| `SpanNames.ReportingApplyInconclusive` (accurate trace, not a reused verdict span) | BuildingBlocks.Observability |
| Frontend defensive test (graceful degradation on the new status) | console ai-diagnostics.spec.ts |

## Gates

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8**.
- `Nexus1.RootCause.UnitTests` → **35/35** (guard-message assertions updated to the
  generalized "A finalized case cannot be changed.").
- `Nexus1.RootCause.ComponentTests` (deterministic, `!~Ollama`) → **60/60** (58 prior +
  2 new inconclusive workflow tests).
- `Nexus1.Reporting.ComponentTests` → **18/18** (16 prior + 2 new inconclusive projection tests).
- `Nexus1.Reporting.UnitTests` → **4/4**.
- Frontend `npx jest ai-diagnostics` → **6/6** (incl. the new Inconclusive defensive test).
- Two clean additive migrations generated (RootCause: 3 nullable columns; Reporting: 2
  nullable columns + `PendingInconclusive` table). Not destructive.

## Component evidence (real LocalDB, no mocks)

**Aggregate / command (FullAnalysisWorkflowTests):**
- Open a generic case → `MarkAnalysisInconclusive` **with no hypothesis/evidence** →
  `Status = Inconclusive`, `Verdict = null`, `InconclusiveReason` + `DecidedBy` set
  (the reason-only invariant).
- The **outbox carries** `RootCauseCaseInconclusiveV1` (routing key
  `root-cause.root-cause-case-inconclusive.v1`, eventType
  `nexus1.root-cause.root-cause-case-inconclusive.v1`) in the same transaction as the
  status change.
- A subsequent `AddHypothesis` fails with the generalized **"A finalized case cannot be
  changed."** (terminal/immutable).
- `MarkInconclusive` with a blank reason → failure "An inconclusive case must record a reason."

**Reporting projection (ReportingProjectionMessageHandlerTests):**
- In-order (opened → inconclusive): both **Ack** (not quarantined), projection
  `Status = Inconclusive` + `Reason`, `Verdict` null, **PoisonMessages = 0**.
- Out-of-order (inconclusive → opened): the inconclusive event buffers in
  **`PendingInconclusive`**, then resolves to `Inconclusive` once the case opens; buffer
  emptied.
- Regression intact: the existing verdict path and the unsupported-contract quarantine
  tests (which use `root-cause-case-closed.v1`, still not in the allowlist) still pass.

**Why this is the definitive poison-prevention proof:** Reporting binds the wildcard
`root-cause.#`, so the new event reaches its queue; the handler's `default:` would
poison an unhandled type. The tests show the handler **Acks and projects** the new event
with **zero PoisonMessages** — i.e. the mandatory allowlist entry prevents the
regression.

## Real broker evidence (RabbitMQ management API)

RabbitMQ 4.3.4 up; exchange `nexus.events` (topic):
- Publish routing key **`root-cause.root-cause-case-inconclusive.v1`** to `nexus.events`
  → **`routed: true`** (matched a `root-cause.#`-bound queue).
- Control: publish a non-matching key (`audit.x.v1`) → **`routed: false`** (routing is
  real, not always-true).
- The **real durable `reporting.integration-events.v1` queue** exists and is bound
  **`root-cause.#`** — so the running Reporting consumer's own queue receives the new
  event.
- **Audit/Compliance safe no-op, proven live:** `audit.root-cause-verdicts.v1` and
  `compliance.root-cause-verdicts.v1` bind **only**
  `root-cause.root-cause-verdict-issued.v1` — **not** the wildcard — so the new event is
  never delivered to them (no poison, no processing). This confirms the code-level
  binding inspection at the broker level.

## Honest boundaries / what was NOT run

- **The full command→outbox→relay→broker→consumer→DB chain was not run end to end in one
  process.** There is no HTTP trigger for the mark-inconclusive command, and the
  Reporting consumer runs only inside `Nexus1.ModularRuntime`, which requires ~6
  provisioned databases — a disproportionate bring-up for this slice. Instead the two
  halves are each proven directly: **the broker routes the new key to the real
  `root-cause.#`-bound reporting queue** (management API), and **the handler projects it
  with zero poison** (component test over real LocalDB). Together these cover the
  regression-critical claim.
- The two new migrations are generated but **not applied to the shared dev DBs** in this
  slice (component tests use throwaway auto-migrated DBs); they apply on the next stack
  start / `dotnet ef database update`.
- **Deferred, named (not silent):** no `alarm-to-inconclusive` workflow-duration metric
  and no dedicated tracing span on the RootCause command handler (both touch the reviewed
  observability vocabulary); Audit/Compliance do **not** consume the new event, so a
  QA-escape-type case is **not yet audited or compliance-reviewed** (reversible ADR-039
  deferral); no 0420 seed data, no provenance corpus, no pipeline restructure; the
  frontend shows the new status as plain text (no dedicated styling/label).
- Dev stack stopped (RabbitMQ down); Ollama left running (not started by this slice).

## Frontend defensive check

The AI Diagnostics screen reads Reporting's projection (`Status.ToString()` → the
literal `"Inconclusive"`) and renders `{{ c.status }}` verbatim, applying `.open`/
`.verdict` classes only by explicit equality. The new spec test flushes an
`Inconclusive` case and asserts it renders the text "Inconclusive", carries neither the
`.open` nor `.verdict` class (no mislabel), and does not crash — graceful degradation,
no feature work.
