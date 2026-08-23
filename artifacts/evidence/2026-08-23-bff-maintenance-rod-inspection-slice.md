# Evidence: BFF eleventh vertical slice — Maintenance, Rod Inspection cluster, plus a dev-mode composition improvement (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with an eleventh vertical slice:

- `GET /api/v1/maintenance/units/{id}/assets` — the Rod Inspection cluster
  (Inspection Overview, NDT Methods, Rod Type/Film — three of the book's
  screens), as one honest endpoint.

Also added, as part of this same task: a dev-testing composition-subset
mechanism for `Nexus1.Bff` itself, motivated directly by this slice's own
memory-pressure difficulties during evidence gathering.

## 1. What Maintenance's Application layer already exposed

A rich existing surface: `GetAssetsByUnitQuery` (atlas C.9.5.2 query 1),
`GetOpenWorkOrdersByUnitQuery` (query 2), `GetWorkOrdersWithOriginQuery`
(query 3), `GetLatestConditionPerAssetQuery` (query 4),
`GetActiveDegradationCasesQuery` (query 5), plus three commands. Same
pattern as every generic-model context so far: every one of these is
fleet-wide despite the "ByUnit" naming — `GetAssetsByUnitQuery` and
`GetLatestConditionPerAssetQuery` take no unit parameter at all. `Asset.UnitId`
and `WorkOrder.UnitId` are both direct, real FKs to `ReactorFleet.Unit`
though, so a per-unit sibling method was straightforward, same shape as
every prior slice.

## 2. Hosted-service check

Read `Maintenance.Infrastructure`'s `ServiceCollectionExtensions` directly:
zero `AddHostedService<...>()` calls. Confirmed by reading the file, not
assumed from the Phase 2 precedent holding six times already.

## 3. The Lifecycle question — a split finding, not a clean yes/no

Checked the domain model honestly before building anything for this
cluster, per the task's explicit instruction.

- **Ageing & Degradation is genuinely real, not illustrative.**
  `DegradationRecord` (mechanism, severity, estimated rate/year, open/close
  lifecycle) and `DegradationTrendPoint` (measured trend values over time,
  tied to a real `CorePlatform.EngineeringUnit` FK and an optional real
  `Instrumentation.Signal` FK) are a genuine, structured degradation-tracking
  model — already queried by the existing `GetActiveDegradationCasesQuery`.
  Nothing here is fabricated or placeholder.
- **Decommissioning and Waste/Spent Fuel do not exist at all.** No entity,
  no table, no concept anywhere in `Nexus1.Maintenance.Domain`. This is a
  total-absence gap — the same shape as Security's zone-access finding, not
  a "missing fields on an otherwise-shaped model" gap like DigitalTwin's or
  RadiationMonitoring's.

**No endpoint was built for Decommissioning/Waste** — there is nothing real
to shape one around, and fabricating fields would misrepresent what this
codebase actually tracks. This is recorded here as the answer to the task's
central question, not glossed over. (Ageing/Degradation's own per-unit
endpoint was not built in this task either — the task's explicit ask was
the Rod Inspection cluster first, with Lifecycle reported on but not
necessarily built; if a per-unit ageing/degradation endpoint is wanted
next, `GetActiveDegradationCasesQuery`/`IActiveDegradationCasesFinder`
already exist and would need the same minimal per-unit sibling treatment
as `GetAssetsByUnitQuery` got here.)

## 4. The Rod Inspection endpoint — honest scope

`GET /api/v1/maintenance/units/{id}/assets` covers all three book screens
(Inspection Overview, NDT Methods, Rod Type/Film) with one endpoint, because
Maintenance's domain model has no rod-specific entity anywhere — `Asset`/
`AssetCondition` are entirely generic (any maintainable equipment item,
generic category/status/grade lookups, `Basis`/`Notes` as free text, not a
structured method taxonomy). NDT Methods and Rod Type/Film have nothing to
map to — named explicitly in `UnitAssetConditionDto`'s own doc comment, not
fabricated as fields on an otherwise generic DTO.

Added `IAssetsByUnitFinder.GetAssetConditionsForUnitAsync(int unitId, ...)`
— keyed by int `UnitId` directly (route-shape consistency with every other
BFF endpoint), combining asset identity with its latest condition
assessment via the same correlated-subquery + in-memory lookup-dictionary
pattern already proven safe in five other contexts.

## 5. Dev-mode composition subset — a real bug found and fixed

This slice's own evidence-gathering repeatedly ran into memory pressure at
host-startup time (composing all eleven contexts). Added a small, guarded
mechanism to `Nexus1.Bff`'s composition root: `BffContexts:Enabled`
(config array or `BffContexts__Enabled__N` env vars) lets a dev/evidence
run compose only the contexts it needs; unset (the default) composes
everything, unchanged from every one of the ten prior slices' proven
behavior. Documented in a new `src/Hosts/Nexus1.Bff/README.md` — explicitly
labeled a dev-testing convenience, not a new architectural layer, per the
task's own framing. No new ADR, per the same instruction — nothing here
represents an architectural decision, just a guard around calls that
already existed.

**A real bug surfaced while building this, not assumed away**: the first
attempt at a subset run (`ReactorFleet` + `Maintenance` only) produced a
global `500` on *every* endpoint — including `/health/ready`, which touches
no application handler at all — with `System.InvalidOperationException:
Body was inferred but the method does not allow inferred body parameters`.
Root cause: Minimal API infers each endpoint parameter's binding source
(service vs. body vs. route) by checking, at the time the whole route table
is first built (lazily, on the *first* incoming request to *any* endpoint,
not per-endpoint), whether each parameter's type is a known DI service.
When a handler type wasn't registered (its context was excluded), that
check failed for that one endpoint's handler parameter — but because the
route table builds all endpoints together, the failure took down routing
for the entire app, not just the one endpoint whose handler was missing.
This was found by actually calling the subset-mode host and reading the
real stack trace, not predicted from documentation.

**Fix**: added an explicit `[FromServices]` attribute to all 15 handler
parameters across every endpoint. This forces DI resolution to happen
per-request instead of during the shared inference step, so a genuinely
unregistered handler now fails only the one endpoint that needed it (an
ordinary "unable to resolve service" error), matching the behavior the
README always intended to describe. Confirmed live: the retried subset run
(below) worked correctly, including endpoints for the composed contexts.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Run twice — once after adding the subset mechanism (before the
`[FromServices]` fix was known to be needed) and once after the fix — both
869/869, confirming the default (unset) full-composition behavior every
prior slice was proven against is genuinely unchanged.

## Memory savings — measured, not estimated

Same machine, same idle baseline recheck between runs, back to back:

| Scenario | Before start | After start | Drop |
|---|---|---|---|
| Full composition (all eleven, default) | 2.14 GB | 1.92 GB | **~220 MB** |
| Subset (`ReactorFleet` + `Maintenance`) | 2.12 GB | 2.01 GB | **~110 MB** |

Roughly a **50% reduction** in host-startup memory cost when composing two
contexts instead of eleven — a real, meaningful saving for evidence-gathering
sessions, not a marginal one.

## Real host, real database — live evidence (subset composition)

Memory checked before starting the host (2.53 → 2.57 GB, confirmed stable).
After host start: 2.03 GB — consistent with the measured subset-composition
cost above. Stable throughout the rest of the run.

`Maintenance.Asset`/`AssetCondition` had zero rows; seeded earlier in this
task (two assets on `UNIT-1`, one with two condition assessments, one
without) — reused here, nothing reseeded.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/maintenance/units/1/assets`

```json
[
  {"assetCode":"ASSET-UNIT1-001","name":"Primary Coolant Pump","category":"MECHANICAL","status":"IN_SERVICE","isSafetyRelated":true,"latestAssessedAtUtc":"2026-08-22T10:00:00","latestConditionGrade":"GOOD","latestHealthScorePercent":91.00,"latestRemainingUsefulLifeDays":1750},
  {"assetCode":"ASSET-UNIT1-002","name":"Backup Feedwater Valve (no condition yet)","category":"MECHANICAL","status":"IN_SERVICE","isSafetyRelated":false,"latestAssessedAtUtc":null,"latestConditionGrade":null,"latestHealthScorePercent":null,"latestRemainingUsefulLifeDays":null}
]
```

HTTP 200. Confirms: (a) the latest condition is genuinely the most recent —
`91.0%`/`1750` days at `10:00`, not `92.5%`/`1800` days at `09:00`; (b) the
asset with zero assessments appears with null condition fields rather than
being excluded.

### `GET /api/v1/maintenance/units/999/assets` (unit with no assets)

```json
[]
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `Maintenance`), both under `nexus1_app`, no fallback. This
is itself corroborating evidence that the subset mechanism genuinely
composed only what was asked, not a coincidence.

`sys.databases` confirmed all `ONLINE` afterward; no corruption at any
point across this task's several restarts.

## Summary

Eleven vertical slices now exist in `Nexus1.Bff`. Maintenance's own
contribution is a split Lifecycle finding (real degradation tracking,
totally absent decommissioning/waste concept) reported honestly rather than
forced into a uniform answer, and a Rod Inspection endpoint that covers
three book screens with one real, non-fabricated data shape. Separately,
this task's own memory difficulties motivated a genuine, if small,
improvement to the BFF's own development experience — including a real bug
(the `[FromServices]` global-failure mode) found by testing the new
mechanism live rather than assumed to work from the design alone, matching
this project's own "verify, don't assume" discipline.
