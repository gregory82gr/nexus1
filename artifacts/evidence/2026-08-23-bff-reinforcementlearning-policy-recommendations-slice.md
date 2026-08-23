# Evidence: BFF seventeenth vertical slice — ReinforcementLearning (policy grid + clamped recommendation history)

## Scope

Built directly on the `From_Trial_to_Policy` vs. codebase audit (reported
separately): two endpoints, honestly scoped to what actually exists —
persisted policy/recommendation data, not live training or live advisory
computation.

- `GET /api/v1/reinforcement-learning/policy` — the current policy grid,
  resolved through a new "active policy" lookup rather than requiring the
  caller to supply a `PolicyId`.
- `GET /api/v1/reinforcement-learning/recommendations` — clamped advisory
  recommendation history.

## 1. The new "active policy" lookup

No `IsCurrent`/`IsActive` concept exists on `Policy` itself, and
`PolicyStatus` is a generic lookup with no enforced single-active-row rule
— confirmed by reading `Policy.cs` and `PolicyStatus.cs` directly rather
than assumed. Added `IActivePolicyFinder`/`GetActivePolicyIdQuery`/Handler/
`EfActivePolicyFinder`, defined as: **the most recently extracted `Policy`
whose source `QTable.IsFinal` is true** — the one real, documented
invariant this domain does support (atlas C.11.5.2 query 2's own "a final
Q-table should contain 175 state-action values"). This is recorded
explicitly, in both the code comments and here, as this slice's own
judgment call, not a recovered fact from the domain model. Returns 404
when no final policy has ever been extracted, rather than fabricating one.

## 2. Named audit findings, carried into the code and this report

**Finding 1 — `Policy`/`PolicyEntry` are stored tables, not the book's
view.** Appendix C's `dbo.Policy` is a SQL `VIEW`
(`ROW_NUMBER() OVER (PARTITION BY RunId, State ORDER BY Q DESC)`),
explicitly "never stored... cannot drift from the values beneath it."
This codebase's `Policy`/`PolicyEntry` are real, materialized EF tables,
populated once by `ExtractPolicyCommand`. **They can drift from
`QTableEntry` if that command isn't re-run after a `QTable` update** — a
genuine deviation from the book's stated guarantee, not a cosmetic
difference. Noted in `Program.cs`'s composition comment for this context.

**Finding 2 — no live advisory computation exists.** Chapter 10's
`AdvisoryService` (read a live state, discretize, `Greedy()`, clamp to the
validated band, return a suggestion) has no equivalent anywhere in this
codebase. `AdvisoryRecommendation`/`AdvisorySession` only record an
already-computed recommendation (`RecordAdvisoryRecommendationCommand`)
and read history back (`GetClampedRecommendationsQuery`) — this directly
answers ADR-026's open question: "training/persistence only" excluded
Chapter 10's synchronous read-side logic too, not just messaging. The
`/recommendations` endpoint is named and commented explicitly as history,
not a "Why This Action" live-suggestion verb, so nothing in the route
shape implies a capability that isn't there.

## 3. Hosted-service check — confirmed directly

`AddReinforcementLearningInfrastructure` has zero `AddHostedService<...>()`
calls (re-confirmed directly, not assumed from ADR-026's own text alone) —
"training/persistence only" per ADR-026 Option A. No opt-out needed.

## 4. Build and full regression suite

```
dotnet build src/Hosts/Nexus1.Bff/Nexus1.Bff.csproj → 0 Warning(s), 0 Error(s)
dotnet build Nexus1.Runtime.sln                     → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln                       → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged from the EmergencyPreparedness slice's baseline — no regressions.

## 5. Memory discipline

| Check | Reading | Notes |
|---|---|---|
| Before host start, 1st | 3.14 GB | |
| Before host start, 2nd (+5s) | 3.38 GB | stable |

Comfortably above threshold both readings.

## 6. Real host, real database — live evidence (subset composition: ReactorFleet + ReinforcementLearning)

All ReinforcementLearning tables were empty — no dev-run residue. Seeded a
full but deliberately reduced dev chain (3 states x 3 actions = 9 cells,
not the book's full 35 x 5 = 175 — same "minimal, not full-scale" seeding
discipline as every prior slice) through the entire seven-FK
`TrainingRun` chain: 9 lookup rows, `Experiment`, `EnvironmentModel`,
`StateSpace`/3x`StateDefinition`, `ActionSpace`/3x`ActionDefinition`,
`RewardFunction`, `HyperparameterSet` (`α=0.2, γ=0.95` — the book's own
values, for continuity), `TrainingRun`, one `QTable` (`IsFinal = true`), 9
`QTableEntry` rows, one `Policy` + 3 `PolicyEntry` rows (argmax computed by
hand to match the seeded `QTableEntry` values, as `ExtractPolicyCommand`
would), `PolicyDeployment`, `AdvisorySession`, and 2
`AdvisoryRecommendation` rows — one `WasClamped = 1`, one `WasClamped = 0`,
to prove the query's filter live.

One seeding error caught and fixed: the first `ConfidenceScore` draft
reused a raw Q-value (`1.40`), violating
`CK_ReinforcementLearning_AdvisoryRecommendation_ConfidenceScore`
([0,1]) — corrected to `0.90` before re-inserting that one row.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/reinforcement-learning/policy`

```json
[{"stateIndex":0,"stateCode":"S0","bestActionCode":"WITHDRAW_2","bestQValue":0.6200000000,"actionMargin":0.7000000000},
 {"stateIndex":1,"stateCode":"S1","bestActionCode":"HOLD","bestQValue":1.4000000000,"actionMargin":2.2500000000},
 {"stateIndex":2,"stateCode":"S2","bestActionCode":"INSERT_2","bestQValue":0.5500000000,"actionMargin":0.0100000000}]
```

HTTP 200. Confirms: the active-policy lookup correctly found `PolicyId 1`
(the only `IsFinal` `QTable`'s extracted policy) without being told which
one; S1 shows a clearly peaked row (margin 2.25, matching the book's
Figure 5.1 shape); S2 shows a near-tie (margin 0.01) — both read straight
from the seeded `PolicyEntry` values, not recomputed.

### `GET /api/v1/reinforcement-learning/recommendations`

```json
[{"advisoryRecommendationId":1,"requestedAtUtc":"2026-08-21T14:05:00","stateCode":"S0","recommendedActionCode":"WITHDRAW_2","clampedActionCode":"HOLD","clampReason":"Recommended withdraw exceeded validated band; clamped to hold."}]
```

HTTP 200. Exactly one row — the `WasClamped = 1` recommendation. The
second, unclamped recommendation was correctly excluded, confirming the
query's filter live.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                            status
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `ReinforcementLearning`), both under `nexus1_app`.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## Summary

Seventeen vertical slices now exist in `Nexus1.Bff`. ReinforcementLearning's
own contribution is built precisely on what the prior audit established:
a real, working policy-grid read (resolved through a new, explicitly
judgment-call "active policy" lookup) and a real, working clamped-
recommendation history read — with both of the audit's named findings
(the `Policy`-as-table deviation from the book's view design, and the
complete absence of live advisory computation) carried forward into the
shipped code's own comments and this report, not left behind in the
investigation. Nothing in either endpoint's name or shape claims a live
capability this codebase does not have.
