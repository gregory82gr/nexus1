# Evidence: Angular console, Ch. 24 — AI Diagnostics

## Scope

One real screen, `ai` route, no new BFF route (the real case-history
endpoint was already live, hosted by Reporting):

1. `AiDiagnosticsComponent` (`features/ai-diagnostics/`) — the book's own
   DSLM advisory panel, unchanged; a real, distinctly-styled Predictive
   Diagnostics equivalent sourced from RootCause's real investigation-case
   history.
2. A data-cleanliness pass on `Reporting.RootCauseCaseSummary`, done
   proactively this time (learned from the Alarms & Events review round).

## Investigation

This is the first screen about advice, not data. The book's own two
panels: a DSLM advisory panel honestly labeled "ROADMAP · PLANNED, not
running in this build," and a "Predictive Diagnostics" panel labeled a
working Phase-0 demonstrator — whose Component Risk table colors demo
risk percentages with the SAME `led ok/warn/crit` classes the real alarm
table uses, and whose example-interaction citations point at a Component
Registry and a Root-Cause causal graph that don't exist yet.

Checked directly before building:

- **RootCause Domain** (`Nexus1.RootCause.Domain`): confirmed, per
  ADR-005, this is deliberately a minimal investigation-case workflow —
  `RootCauseAnalysis` (Open/Closed status, free-text `Verdict`, a list of
  `AnalysisHypothesis`), no scored `AnalysisCandidate`, no
  `RejectedCandidate`, no `CausalGraph` — all explicitly deferred per
  ADR-005. No `RiskScore`, no confidence field, no per-component health
  concept anywhere in this project.
- **RootCause Application**: exactly one query (`GetAnalysisByIdQuery`,
  needs a specific id — no fleet-wide list), five write commands, and the
  real production path — `AlarmFloodConsumerBackgroundService` auto-opens
  a case from AlarmManagement's flood-detection event. `Nexus1.RootCause.Host`
  exposes only health endpoints; nothing exposes RootCause's Application
  layer over HTTP directly.
- **ComponentRegistry**: confirmed absent everywhere in this codebase —
  only the Angular nav/route stub (`components`, chapter 28, still
  `PlaceholderComponent`, not reached).
- **The one real, honest "diagnostic-shaped" data anywhere**: Reporting's
  own projection (`GetCaseSummariesForUnitQuery`, `GET
  /api/v1/reporting/units/{id}`, already live) — real investigation-case
  status (`Open`/`VerdictIssued`) and free-text verdicts, projected from
  RootCause's own domain events. Genuinely real, genuinely diagnostic in
  nature, and definitively NOT a risk score.
- **Reconciled a naming conflict, with the user**: this exact route's own
  code comment (written in an earlier slice) called it "the Trends &
  History screen's" data. Confirmed with the user this was a non-binding
  naming guess from before this cluster's own investigation — the data
  genuinely belongs to AI Diagnostics architecturally (the "From Flood to
  Cause" companion book's own later-phase vision shows this exact
  RootCause verdict data as the intended grounding evidence for this
  screen). Ch.26 Trends & History, when reached, will be investigated on
  its own real merits, not assumed to need this same endpoint.

**Decision applied** (per explicit user direction): show RootCause's real
case-history data as an honestly-named, real "Predictive Diagnostics"-
style panel — distinctly styled, never reusing the alarm/safety `ok/warn/
crit` LED classes, since a case status is neither a severity nor a score.
Build **nothing** from the future RAG/DSLM architecture: no grounding
triangle, no citation-generation pipeline, no Semantic Kernel/Ollama
wiring, no real "Grounded in: ..." line implying this system produces
cited explanations today. The DSLM panel and its example interaction stay
exactly as the book has them, clearly marked illustrative/scripted — the
RAG phase itself is out of scope, not opened by this slice.

## No new BFF route needed

`GET /api/v1/reporting/units/{id:int}` (Program.cs, unchanged) already
existed, wrapping `GetCaseSummariesForUnitQueryHandler`, fully proven
since an earlier slice — zero backend code changes this cluster.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Data-cleanliness pass (done proactively, before the first screenshot)

Checking `Reporting.RootCauseCaseSummary` live turned up the same
pattern as the Alarms & Events cluster: 942 rows, all component/e2e-test
fixture residue across 8 synthetic unit ids (`9201`, `9202`, `9301`,
`9302`, `9101`, ...), **zero** rows for the real units (`1`, `2`).
Learned from the prior review round — checked and cleaned this up before
capturing any screenshot, not after:

- **Safety check**: confirmed no `FOREIGN KEY` anywhere in the solution
  references `Reporting.RootCauseCaseSummary` (`sys.foreign_keys` query,
  zero rows) — a leaf projection table, safe to clear.
- **Removed**: `DELETE FROM Reporting.RootCauseCaseSummary WHERE UnitId
  NOT IN (1,2)` — all 942 residue rows.
- **Seeded 4 realistic cases** for the real units, each tied to a
  concept already established in prior clusters (turbine speed sensor,
  aux-building radiation monitor, steam-generator level) — continuity,
  not arbitrary text:
  ```
  Unit 1, VerdictIssued: "Turbine shaft speed sensor UNIT1-TURB-001 recalibrated; no mechanical fault found."
  Unit 1, VerdictIssued: "Loose fitting on aux building radiation monitor RM-AUX-1 confirmed as cause."
  Unit 2, VerdictIssued: "Steam generator SG-2 level transmitter drift confirmed as root cause."
  Unit 2, Open:          (no verdict yet — investigation in progress)
  ```

## Frontend: what was built

- `core/api/root-cause-cases-api.ts` — dedicated client for the direct
  per-unit endpoint (mirrors `CaseSummaryDto` exactly), named after what
  the data actually is (RootCause case history), not its technical host
  (Reporting).
- `features/ai-diagnostics/ai-diagnostics.ts/.html/.scss` — the DSLM
  panel (static, book's own wording, ROADMAP · PLANNED, illustrative
  example interaction with an explicit non-live disclaimer under the
  citation line) plus the real case-history panel. `.case-status` is its
  own dedicated CSS class, deliberately never `.pill.ok/.warn/.crit` —
  verified by a Jest test that queries the rendered DOM and asserts zero
  alarm-style pill elements are ever present.
- `app.routes.ts` — the single `ai` route now points at
  `AiDiagnosticsComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest ai-diagnostics → 5/5 passing (new specs alone)
npx jest (full suite)   → 176/176 passing (was 171)
```

- Loading/error/loaded states, fetches real per-unit case history.
- Real case status and verdict text asserted directly (not a score).
- Real empty state when no cases exist.
- **Guard test**: renders the loaded cases, then asserts
  `.pill.ok/.warn/.crit` never appears anywhere in the DOM and exactly 2
  `.case-status` elements do — verifying the chapter's own core fix is
  actually true in the rendered output, not just claimed in a comment.
- Real error state on an unreachable endpoint.

Production build:
```
npx ng build → 0 errors, 0 warnings. ai-diagnostics compiles to its own
               lazy chunk (~2.44 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently. Available memory was checked before
starting the live hosts (1.80 GB) and `dotnet build-server shutdown` was
run as a precaution, bringing it to 2.41 GB.

## Live evidence — real host, real database, real screenshot

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Reporting`; `ng serve --port 4200` alongside it.

```
GET /health/ready                       → Healthy, HTTP 200
GET /api/v1/reporting/units/1 → 2 cases, both VerdictIssued, real
  turbine/radiation-monitor verdict text
GET /api/v1/reporting/units/2 → 2 cases, one VerdictIssued (SG-2 level
  transmitter drift), one Open (no verdict yet)
```

`/ai` rendered live (`get_page_text`, no console errors): DSLM panel
exactly as written, example interaction marked "ILLUSTRATIVE ONLY — NOT
LIVE," gap declarations visible on-screen (Component Registry absent,
RootCause has no scored causal graph); Predictive Diagnostics panel
showing the 2 real cases for unit 1 with distinct `.case-status` pills —
visually nothing like the Alarms & Events screen's severity-colored
pills.

### Screenshot

- `ai-diagnostics.png` — `/ai`, full-width shell, sidebar correctly
  highlighting "AI Diagnostics" active, DSLM panel with muted
  "ROADMAP · PLANNED" tag, illustrative example clearly marked, real
  case-history panel with distinctly-styled status pills, clean layout.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x1)
```

One session, matching the one composed context. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Summary

Checked RootCause's real domain (ADR-005-minimal, no scoring, no causal
graph) and confirmed ComponentRegistry doesn't exist yet. Found the one
honest real data source for a "diagnostics" panel — RootCause's own
investigation-case history, projected by Reporting — and reconciled a
naming conflict with an earlier slice's own route comment directly with
the user before using it. Built the DSLM panel exactly as the book has
it, with its example interaction clearly marked illustrative and no
grounding/citation pipeline of any kind built. Built the real-data panel
with deliberately distinct styling from every alarm/safety LED class,
verified by a DOM-level test, not just a comment. Cleaned up 942 rows of
test-fixture residue proactively this time, before the first screenshot,
applying the lesson from the Alarms & Events review round.
