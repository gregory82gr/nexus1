# Evidence — Ch.29 Root Cause screens + Angular consumer for the BFF route

**Date:** 2026-09-23
**Book chapter:** From File to Framework, Ch.29 (Root Cause) — the one Angular chapter
deferred until the RAG backend existed.
**Scope:** Frontend-only. Two new screens (Root Cause, Incident Analysis) consume the
existing BFF diagnosis route (ADR-034/ADR-035) through the console's single gateway.

## Built

| Piece | Location |
|---|---|
| `RootCauseDiagnosisApi` (typed client → `POST /api/v1/root-cause/incidents/{id}/diagnoses`) | `core/api/root-cause-diagnosis-api.ts` |
| `RootCauseService` (single fetch-and-cache owner; both screens read it) | `features/root-cause/root-cause.service.ts` |
| Root Cause screen (verdict, ranked candidates, real citations, audit hash, illustrative labels, graph-pending note) | `features/root-cause/root-cause.{ts,html,scss}` |
| Incident Analysis screen (top-cause card reading the service; no CAND; no engineer-confirmation; no timeline) | `features/incident/incident-summary.{ts,html,scss}` |
| Routes `incident` + `rcgraph` now point at the real components (were PlaceholderComponent) | `app.routes.ts` |

## Honest-scope decisions (per the approved plan)

- **Graph visualization deferred, named not faked** — the route returns the ranked
  verdict + citations, not the fault-tree topology. The screen renders the real ranking
  and a "GRAPH VISUALIZATION — PENDING A BACKEND ENDPOINT" note instead of a
  hand-authored graph.
- **Event Reconstruction timeline omitted** — no backend source; not fabricated.
- **Fixed-incident only** — EVT-2026-0418; a 404 renders an honest "No diagnosis
  available for this incident" empty state, no incident-browsing UI.
- **Ch.29's correction inherited by construction** — these screens are new, so there was
  never a local `CAND` hypothesis array or "confirmed by an engineer" claim to retire.
- **Honest async states** — real loading (naming the up-to-~2-min first-run wait),
  first-class abstention ("inconclusive — no verdict", never an error), and 502/503 error
  states; never a fabricated verdict.

## Gate results

- `npx jest` (full Angular suite) → **280 passed, 55 suites, 0 failed** (13 new tests
  included; no regression).
- New specs: `root-cause.spec.ts` (verdict/candidates/citations/auditHash render;
  loading; abstention-not-error; 404 empty; 503 error; no `<svg>` graph + pending note)
  and `incident-summary.spec.ts` (cross-screen agreement; no-`CAND`; **Ch.33 spec-of-specs**
  proving the no-`CAND` guard is load-bearing; no engineer-confirmation; honest states) —
  **13 passed**.
- `npx ng build --configuration development` → clean, **no NG8107 warning** (the
  `topCause` computed is typed `DiagnosisCandidate | null` so the template `?.` is valid).
- Grep proofs (feature code, excluding spec guard-fixtures): **no `const CAND`**, **no
  "confirmed by an engineer"** in templates/rendered code.

## Live render against the real stack (the key evidence)

Real browser (Browser pane) → `http://localhost:4200` (Angular) → BFF `:5103` → Host
`:5102` → Ollama `nexus-dslm` → RootCauseDb. Full chain, not a fixture.

**Root Cause screen (`/rcgraph`)** rendered the real result:
```
VERDICT — ORIGIN CAUSE: FV-104
Ranked candidates:  FV-104 origin 66% / 79%,  PB-2A proximate 20% / 50%,
                    RCP-1B parallel 7% / 50%,  BUS-2A contributing 5% / 7%,
                    FT-7 ruled-out 2% / 0%
Grounding citations: "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.1-2"
                     "From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.2 (delay windows)"
Sealed audit hash:   0e6cf02a7ae7dcbccfb192713ff87fa5fe2579be89954080a0a42dc94455141d
Graph visualization — pending a backend endpoint (honest note; no drawn graph)
```

**It really hit the backend (not a fixture):** immediately after the render, RootCauseDb's
newest audit entry was `seq=9 hash=0e6cf02a7ae7dcbccfb192713ff87fa5fe2579be89954080a0a42dc94455141d`
— **exactly the hash the console displayed**. The browser triggered a new, real,
audit-sealed diagnosis run through the whole chain.

**Live cross-screen agreement:** clicking the **Incident Analysis** nav link (staying in
the SPA) rendered **TOP CAUSE: FV-104 — origin — 66%** *instantly* — no second backend
call, because it read the same cached `RootCauseService` result. The same top cause as the
graph's #1, from the one place the answer is produced (Ch.29's rule). Its header states
"there is exactly one place the answer is produced, so the two screens cannot disagree."

Both screens were captured to disk via Playwright (against the running console,
full-page), matching the existing `artifacts/evidence/screenshots/` PNG convention:
- `artifacts/evidence/screenshots/root-cause.png` — verdict FV-104, the ranked table,
  real citations, the sealed audit hash, and the graph-pending note.
- `artifacts/evidence/screenshots/incident-analysis.png` — TOP CAUSE FV-104 — origin —
  66%, with the "one place the answer is produced, so the two screens cannot disagree"
  header.
Each capture triggered its own real, audit-sealed diagnosis run through the full chain
(the Root Cause capture's own run sealed audit hash c62f092f3c307b8ecab08debd5ef388492d4e2850b25d4a593bcbe4db66efdaa).

## Ch.33 deferred cross-screen regression test

The component-level cross-screen agreement test (both screens read the one service) and a
**spec-of-specs** (a reintroduced `const CAND` fixture must match the guard regex, proving
it is load-bearing — Ch.33's "seen to fail" ritual) landed here. The **full live E2E
un-deferral** (Ch.33 `operator-session` cross-screen step needing the whole stack up,
20–100s per run) **remains a named follow-up**, not wired here.

## Scope boundary

No graph-topology drawing, no Event Reconstruction timeline, no second incident, no
backend/route change, no other Angular screen touched. Just: the console's real screens
consume the real pipeline over the network and can never disagree about the verdict.

## Operational note

The stack was already running from a prior session (Host, BFF, ng serve, Ollama,
RabbitMQ). The model went cold between sessions; the first render-triggering call was ~109s
(cold, under the 150s Host timeout — the ADR-033-addendum fix), then warm.
