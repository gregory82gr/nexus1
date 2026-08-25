# Evidence: Angular console, Ch. 25 — Optimization

## Scope

One real screen, `rlopt` route, no new BFF route (both routes were
already live and proven since an earlier BFF slice):

1. `OptimizationComponent` (`features/optimization/`) — real active
   policy grid + real clamped-recommendation history. No live advisory
   panel — declared explicitly as an out-of-scope gap.

## Investigation

Already settled before this cluster, per ADR-026: ReinforcementLearning
is training/persistence only — no live advisory computation, no running
RL agent, no real-time optimization engine anywhere. Checked directly,
per the task's own instruction, before building:

- **Domain**: confirmed no `Episode`/`EpisodeStep`/reward-trend entity
  exists anywhere — `TrainingRun.TotalReward`/`AverageReward` are single
  aggregate values per run, not a series. `Policy` has no version-number
  field; policy versioning is only implicit through multiple `Policy`
  rows over time.
- **Application**: `GetPolicyGridQuery`/`GetActivePolicyIdQuery`
  (genuinely queryable — real state×action policy grid) and
  `GetClampedRecommendationsQuery` (genuinely queryable — recorded
  advisory history) both have real handlers + finders. `RecordTrainingRunCommand`
  and `ExtractPolicyCommand` are write-only — **no query exists to list
  training-run history or policy-version metadata**. Confirmed, not
  assumed: this is a real gap, not something to add new Application code
  for (no handler exists to wrap in a thin route).
- **Drift risk, restated as instructed**: Policy/PolicyEntry are real,
  materialized EF tables (`Program.cs` lines 334-342; also recorded as
  "Finding 1" in the `2026-08-23-bff-reinforcementlearning-policy-recommendations-slice.md`
  evidence) — not the book's own Appendix C design (a SQL `VIEW`
  recomputed via `ROW_NUMBER()` on every read, which "cannot drift from
  the values beneath it"). Here it CAN drift from `QTableEntry` if
  `ExtractPolicyCommand` isn't re-run after a training update. This
  bears directly on what the screen shows, so it's stated on-screen, not
  just in a comment.
- **"Active policy" is a judgment call, not a domain fact**:
  `IActivePolicyFinder`'s own doc comment states there is no
  `IsCurrent` flag anywhere — "active" means the most recently extracted
  `Policy` whose source `QTable` `IsFinal`. Stated on-screen as well.

**Conclusion**: no live "here's what to do next" advisory panel is built
— declared as a gap, matching ADR-026 exactly. No training-run/reward-
trend panel either — no query exists for either, so nothing is
fabricated to fill that gap. What's shown is the two things genuinely
queryable today: the active policy grid and the clamped-recommendation
history, both explicitly labeled for what they actually are (a
snapshot that can drift; recorded history, not a live explanation).

## No new BFF route needed

`GET /api/v1/reinforcement-learning/policy` and `GET
/api/v1/reinforcement-learning/recommendations` (Program.cs, unchanged)
already existed, fully proven since the `2026-08-23` BFF slice — zero
backend code changes this cluster.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `core/api/reinforcement-learning-api.ts` — mirrors `PolicyGridEntryDto`
  and `ClampedRecommendationDto` exactly. `getActivePolicyGrid()` maps a
  real `404` (no final policy has ever been extracted — a meaningful
  domain state per the BFF route's own comment) to `null`, distinct from
  a genuine connectivity error, rather than collapsing both into one
  generic "error" state.
- `features/optimization/optimization.ts/.html/.scss` — two independent
  panels (matching the two independent real queries, same
  two-signal-per-panel pattern as `mission-readiness.ts`): the policy
  grid (state → best action, real Q-value and margin, with the drift-
  risk caveat stated on-screen) and the clamped-recommendation history
  (explicitly labeled "recorded history, not live"). A top banner
  states the no-live-advisor and no-training-history gaps explicitly.
- `app.routes.ts` — the single `rlopt` route now points at
  `OptimizationComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest optimization → 5/5 passing (new specs alone)
npx jest (full suite) → 181/181 passing (was 176)
```

- Both panels start loading and fetch the real fleet-wide endpoints
  independently.
- Real policy grid entries render correctly once loaded.
- A real `404` maps to a distinct `no-policy` state, not `error` —
  verifying the meaningful-domain-state-vs-connectivity-failure
  distinction is actually implemented, not just described.
- Real clamped-recommendation history renders correctly (recommended vs.
  clamped action, clamp reason).
- Each panel shows its own real error state independently on a genuine
  connectivity failure.

Production build:
```
npx ng build → 0 errors, 0 warnings. optimization compiles to its own
               lazy chunk (~2.32 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently. Available memory was checked before
starting the live hosts (2.28 GB, already healthy) and `dotnet
build-server shutdown` was run as a precaution regardless, bringing it
to 2.78 GB.

## Live evidence — real host, real database, real screenshot

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=ReinforcementLearning`; `ng serve --port 4200`
alongside it.

```
GET /health/ready                                    → Healthy, HTTP 200
GET /api/v1/reinforcement-learning/policy            →
  [{"stateIndex":0,"stateCode":"S0","bestActionCode":"WITHDRAW_2","bestQValue":0.62,"actionMargin":0.70},
   {"stateIndex":1,"stateCode":"S1","bestActionCode":"HOLD","bestQValue":1.40,"actionMargin":2.25},
   {"stateIndex":2,"stateCode":"S2","bestActionCode":"INSERT_2","bestQValue":0.55,"actionMargin":0.01}]
GET /api/v1/reinforcement-learning/recommendations   →
  [{"advisoryRecommendationId":1,"requestedAtUtc":"2026-08-21T14:05:00","stateCode":"S0",
    "recommendedActionCode":"WITHDRAW_2","clampedActionCode":"HOLD",
    "clampReason":"Recommended withdraw exceeded validated band; clamped to hold."}]
```

This dataset (1 policy with 3 entries, 1 clamped recommendation) was
already real and already plausible from the original `2026-08-23` BFF
slice — no test-fixture residue found this time, so no cleanup pass was
needed.

`/rlopt` rendered live (`get_page_text`, network log confirmed both
requests `200 OK`): the no-live-advisor banner, the real 3-state policy
grid with correct Q-values/margins, and the real clamped-recommendation
record with its full clamp reason — matching the built component
exactly. One benign console message (`ERR_CONNECTION_REFUSED`) was
Vite's own dev-server HMR websocket reconnecting after a host restart on
a reused browser tab — unrelated to the application; confirmed both real
API requests returned `200 OK` via the network log directly.

### Screenshot

- `optimization.png` — `/rlopt`, full-width shell, sidebar correctly
  highlighting "Optimization (RL)" active, no-live-advisor banner, real
  3-row policy grid, real clamped-recommendation record with its clamp
  reason, clean layout.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x2)
```

Two sessions for the one composed context (ordinary EF connection-pool
behavior, not a second context). Both processes stopped cleanly after
capture; `sys.databases` confirmed all 9 databases `ONLINE` afterward.

## Summary

Confirmed ADR-026's training/persistence-only scoping holds directly in
Domain (no episode/reward-trend entity, no training-run/policy-version
query) before building anything. Restated the already-known table-vs-view
drift risk on-screen, since it bears directly on what this policy grid
actually shows. Built the two genuinely real panels (policy grid,
clamped-recommendation history), both explicitly labeled for what they
are — a stored snapshot that can drift, and recorded history, not a live
suggestion — and declared the no-live-advisor and no-training-history
gaps explicitly rather than fabricating either.
