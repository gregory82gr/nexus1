# Evidence: BFF twelfth vertical slice — CorePlatform, reference/lookup data

## Scope

Extended `Nexus1.Bff` with a twelfth vertical slice:

- `GET /api/v1/core-platform/deployment-versions` — currently-deployed
  platform software components (atlas C.1.8 verification query, verbatim).
- `GET /api/v1/core-platform/engineering-units` — active units of measure
  (atlas C.1.8 verification query, verbatim).

Both wire in already-existing, unmodified Application-layer handlers.
**Zero new Application-layer code was needed for this slice.**

## Central question: is "Component Registry" an honest name for CorePlatform?

**No.** CorePlatform has no physical-equipment/component entity anywhere.
Its own "components" (`DeploymentVersion.ComponentName`/`ComponentType`) are
**software deployment artifacts** — the enum `DeploymentComponentType` is
`Console, Schema, SeedData, ApiService, Worker, Documentation` (atlas
C.1.4.9's own check constraint) — not plant equipment, not anything a
"Component Registry" screen (in the book's sense of physical
pumps/valves/instruments) would recognize.

What CorePlatform genuinely provides is two **fleet-wide/global reference
registries**, not per-unit data:

1. A **deployment-version registry** — which software components are
   currently deployed, at what version (`DeploymentVersion` entity,
   `GetCurrentDeploymentVersionsQuery`).
2. A genuine **units-of-measure registry** (`EngineeringUnit` — °C, %RTP,
   kPa, uSv/h, etc.), referenced across the whole platform (instrumentation
   signals, alarm thresholds, model variables, reports) instead of
   free-text symbols. Also `From_Domain_to_Twin`'s own bounded-context
   naming-collision teaching example (pp. 24, 45):
   `CorePlatform.EngineeringUnit` is deliberately not `ReactorFleet.Unit`.

Both are fleet-wide, not per-unit — shaped honestly as flat listing
endpoints (`GET .../deployment-versions`, `GET .../engineering-units`), not
forced into a `{id}`-scoped route that doesn't fit either concept.

## 1. What CorePlatform's Application layer already exposed

Checked `ServiceCollectionExtensions.cs` before writing anything.
`GetCurrentDeploymentVersionsQueryHandler` and
`GetActiveEngineeringUnitsQueryHandler` were **already registered**,
alongside `UpdateAppSettingValueCommandHandler`,
`EvaluateFeatureFlagQueryHandler`, and `ResolveLocalizedTextQueryHandler`
(feature flags / app settings / localization — out of scope for this
"Component Registry" screen, not touched this slice). Nothing needed to be
added on the Application side; this slice is pure BFF wiring.

## 2. Domain model — what's actually there

- `DeploymentVersion` (table `CorePlatform.Version`): `ComponentName`,
  `ComponentType` (enum, software artifacts as above), `VersionNumber`,
  `IsCurrent` with `MarkCurrent()`/`MarkNotCurrent()`. Confirms the
  "Component Registry" naming mismatch decisively — see above.
- `EngineeringUnit`: unit-of-measure passport, real and structurally
  complete (`Symbol`, `Name`, `QuantityType`, SI conversion factor/offset,
  `IsDimensionless`, `IsActive`, `DisplayOrder`).

No physical-equipment/classification entity exists anywhere in
`Nexus1.CorePlatform.Domain`. This is a total-absence gap for the "Component
Registry" concept as the book screen names it — the same shape as
Security's zone-access finding and Maintenance's Decommissioning finding,
not a "missing fields on an otherwise-real model" gap.

## 3. Hosted-service check

Read `CorePlatform.Infrastructure`'s `ServiceCollectionExtensions` directly:
zero `AddHostedService<...>()` calls. Confirmed by reading the file, not
assumed from the (now seven-times-holding) Phase 2 precedent.

## 4. Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total, 0 failed
```

Matches the established baseline exactly — no regressions.

## 5. Memory checks and subset-mode data point

Per-step memory discipline, checked twice for trend before any host start:

| Check | Reading | Notes |
|---|---|---|
| Pre-host, 1st | 1.73 GB | borderline |
| Pre-host, 2nd (+5s) | 1.56 GB | declining — **stopped, did not start host** |
| (test suite finished; re-checked) 1st | 2.30 GB | recovered |
| (re-checked) 2nd (+5s) | 2.29 GB | stable |
| After subset-mode host start (ReactorFleet + CorePlatform) | 2.16 GB | |

Startup cost for this two-context combination: **~130–140 MB**, consistent
with the Maintenance slice's own ReactorFleet+Maintenance measurement
(~110 MB) — the subset-composition saving continues to hold across a
different context pairing, as requested.

## 6. Real host, real database — live evidence (subset composition: ReactorFleet + CorePlatform)

Checked for existing data before seeding, per the established discipline:

- `CorePlatform.EngineeringUnit`: **2 rows already present** (real,
  pre-existing) — no seeding needed.
- `CorePlatform.Version` (`DeploymentVersion`): **0 rows** — seeded two
  minimal dev rows (`Nexus1.Bff` / `ApiService`, versions `3.1.0` current
  and `3.0.0` superseded).

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/core-platform/deployment-versions`

```json
[{"deploymentVersionId":1,"componentName":"Nexus1.Bff","componentType":"ApiService","versionNumber":"3.1.0","releaseDateUtc":"2026-08-01T00:00:00"}]
```

HTTP 200. Confirms the endpoint, the seeded data, and `IsCurrent`
filtering (only the current row returned) all work correctly.

### `GET /api/v1/core-platform/engineering-units` — real bug found live, not introduced by this slice

```
HTTP 500
System.InvalidOperationException: Cannot convert string value 'POWER_FRACTION'
from the database to any value in the mapped 'EngineeringQuantityType' enum.
  at ... EfEngineeringUnitFinder.GetActiveAsync(...)
  at ... GetActiveEngineeringUnitsQueryHandler.Handle(...)
```

**Root cause, confirmed by reading the code, not guessed:**
`EngineeringUnitConfiguration.cs` maps `QuantityType` with a plain
`.HasConversion<string>()` — EF Core's default string-enum converter
round-trips using the **exact C# enum member spelling**
(`Power`, `RadiationDoseRate`, `Percentage`, ...). The two **pre-existing**
`CorePlatform.EngineeringUnit` rows (already in the database before this
slice touched anything) store `QuantityType` as
`RADIATION_DOSE_RATE` / `POWER_FRACTION` — SCREAMING_SNAKE_CASE, matching
the atlas's own check-constraint naming convention
(`CK_CorePlatform_EngineeringUnit_QuantityType`), not the C# enum's PascalCase
member names. This isn't just a casing mismatch either: `POWER_FRACTION`
has no corresponding enum member under any casing convention — the enum has
`Power`, `Percentage`, `ThermalPower`, but no `PowerFraction`.

**This is a genuine, pre-existing defect, not something this slice
introduced** — zero Domain/Infrastructure code was touched for CorePlatform
in this task; the bug was surfaced purely by calling a previously-untested
code path live. Per this session's own "verify, don't assume" discipline,
it's reported here rather than silently patched (fixing it means either
changing the `HasConversion<string>()` mapping to a snake_case convention,
or correcting the seed/production data's `QuantityType` values, or possibly
retiring the `POWER_FRACTION` row's value to a real enum member — all three
are decisions with actual data-integrity consequences, not a wiring choice
this task's scope covers).

Initially left unfixed pending investigation into origin (see below) —
not something to patch on assumption alone.

### Login verification (first host run)

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                            status
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `CorePlatform`), both under `nexus1_app`, no fallback.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## 7. `QuantityType` mismatch — investigation, then fix at the source

Investigated before touching anything, per instruction.

**Origin.** Neither bad value came from a migration or a checked-in seed
script — `POWER_FRACTION` and `RADIATION_DOSE_RATE` appear nowhere in the
solution except this session's own prior evidence reports. Both were
inserted via raw `sqlcmd INSERT` statements in **earlier turns of this same
session**: `RADIATION_DOSE_RATE` while seeding `RadiationMonitoring`'s FK
dependency ([2026-08-22-bff-radiationmonitoring-safety-slice.md:109](2026-08-22-bff-radiationmonitoring-safety-slice.md)),
`POWER_FRACTION` while seeding `Instrumentation`'s
([2026-08-22-bff-instrumentation-reactor-signals-slice.md:107](2026-08-22-bff-instrumentation-reactor-signals-slice.md)).
Both were pattern-matched against the SCREAMING_SNAKE_CASE convention used
by *other, genuinely code-based lookup tables* in this codebase
(`SignalQuality`, `MeasurementType`, etc. — real int-Id-plus-string-Code
reference tables) without checking that `EngineeringUnit.QuantityType` is
structurally different: an EF `HasConversion<string>()`-backed C# enum
column, not a free lookup code. Dev-seed residue from our own earlier
slices, not a production bug.

**Orphaned or renamed?** `RADIATION_DOSE_RATE` has an exact real target —
the enum's `RadiationDoseRate` member — wrong format, not wrong concept.
`POWER_FRACTION` matches no enum member under any casing convention and
isn't a stale rename (no such identifier exists anywhere in git history or
the solution) — it was invented outright. Best real fit: `Percentage`
(%RTP genuinely is a percentage-type quantity).

**Was this live elsewhere?** Searched the whole solution for callers of
`GetActiveEngineeringUnitsQuery`/`EfEngineeringUnitFinder`/
`IEngineeringUnitFinder` — only CorePlatform's own registration and this
slice's new BFF endpoint call this path; `Nexus1.ModularRuntime` never
does (its `Program.cs` only mentions `EngineeringUnit` in FK doc comments
on unrelated tables). This was genuinely unreachable until this slice's
first live call — dormant dev-seed residue, not a pre-existing production
defect. The full test suite gave no signal because
`GetActiveEngineeringUnitsQueryHandlerTests` seeds its own clean data
through the domain factory, never touching the shared LocalDB's bad
literals.

**Are both rows equally broken?** Yes, same root cause. `EfEngineeringUnitFinder`
does `OrderBy(x => x.QuantityType)` translated to SQL *before* conversion,
so the raw string `'POWER_FRACTION'` (alphabetically before
`'RADIATION_DOSE_RATE'`) is the first row EF attempts to materialize —
that's why the exception named it specifically, not because
`RADIATION_DOSE_RATE` would have succeeded. EF Core's
`EnumToStringConverter` does an exact, case-sensitive `Enum.Parse` with no
normalization, so `RADIATION_DOSE_RATE` (underscores, all-caps) would fail
identically if the query ever reached it.

**Fix applied — data only, not the converter.** Per explicit instruction,
the case-sensitive `HasConversion<string>()` design is correct and was left
untouched. Corrected the two seeded rows directly:

```sql
UPDATE CorePlatform.EngineeringUnit SET QuantityType = 'RadiationDoseRate' WHERE EngineeringUnitId = 1;
UPDATE CorePlatform.EngineeringUnit SET QuantityType = 'Percentage' WHERE EngineeringUnitId = 2;
```

`RadiationDoseRate` is the certain, exact original intent. `Percentage` is
a **judgment call**, not a recovered original value — no `PowerFraction`
member ever existed, so this is the best available real fit for %RTP,
recorded here as a judgment call rather than a certain fact.

**Tracked, not fixed: doc-vs-implementation mismatch.** `EngineeringUnit.cs`'s
doc comment claims a DB-level CHECK constraint from the atlas
(`CK_CorePlatform_EngineeringUnit_QuantityType`) exists. The actual migration
(`20260816114650_InitialCorePlatformSchema.cs`) defines `QuantityType` as a
plain `nvarchar(50)` with **no CHECK constraint at all** — nothing in the
database would have stopped either bad insert. Recorded here as a named
gap for later (not added now, per instruction) — worth closing at some
future hardening pass so a repeat of this exact mistake isn't possible
again.

### Re-verification — second host run, subset composition (ReactorFleet + CorePlatform)

Memory checked twice before restart: 2.52 GB → 2.53 GB, stable.

```
GET /health/ready                              → Healthy, HTTP 200
GET /api/v1/core-platform/engineering-units    → HTTP 200 (was HTTP 500 before the fix)
GET /api/v1/core-platform/deployment-versions  → HTTP 200 (regression check, unchanged)
```

```json
[{"engineeringUnitId":2,"symbol":"%RTP","name":"Percent Rated Thermal Power","quantityType":"Percentage"},
 {"engineeringUnitId":1,"symbol":"uSv/h","name":"Microsievert per hour","quantityType":"RadiationDoseRate"}]
```

Both rows now resolve correctly; `deployment-versions` unaffected (no
regression from the data fix).

### Login verification (second host run)

Two `nexus1_app` sessions confirmed again, matching `ReactorFleet` +
`CorePlatform`. `sys.databases` confirmed all `ONLINE` after stop; no
corruption across either host run.

### Full regression suite after the data fix

```
dotnet test Nexus1.Runtime.sln → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged from before the fix, as expected — this was a data correction,
not a code change.

## Summary

Twelve vertical slices now exist in `Nexus1.Bff`, and this slice is
**closed out**. CorePlatform's own contribution is almost entirely a
naming/scope correction rather than new code: "Component Registry" is not
an honest name for what this context models — it is a software
deployment-version registry plus a genuine, fleet-wide
engineering-units-of-measure registry, both already fully built in the
Application layer with zero gaps to fill.

The one real defect surfaced — `POWER_FRACTION`/`RADIATION_DOSE_RATE`
unmappable to `EngineeringQuantityType` — was investigated to its source
before any fix was applied: both values were dev-seed residue this same
session inserted in earlier slices (RadiationMonitoring, Instrumentation),
pattern-matched against the wrong lookup-table convention, never reachable
in production or in any test. Corrected at the data layer only —
`RadiationDoseRate` with certainty, `Percentage` as an explicit judgment
call — leaving the case-sensitive `EnumToStringConverter` untouched since
its design was correct all along. Both endpoints now verified working live.
The missing DB-level CHECK constraint (`EngineeringUnit.cs`'s doc comment
claims one exists; the migration has none) is recorded as a tracked gap,
not fixed now. The dev-mode subset-composition mechanism continues to hold
its ~50%-class memory saving on a second, different pair of contexts,
confirmed again across both host runs in this task.
