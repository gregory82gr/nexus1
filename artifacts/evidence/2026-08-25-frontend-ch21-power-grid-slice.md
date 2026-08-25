# Evidence: Angular console, Ch. 21 — Power & Grid

## Scope

One real screen, `power` route, no new BFF route (the existing generic
Instrumentation signals endpoint already covers it once the underlying
data exists):

1. **New real data**: `TURBINE` `SignalCategory` + one `Signal`
   (`UNIT1-TURB-001`, "Main Turbine Shaft Speed") + two `Measurement`
   rows, seeded via a live evidence session — the same mechanism used to
   introduce `NEUTRONICS` for the Reactor cluster (a new row in
   Instrumentation's existing generic model, not new domain/application
   code).
2. `PowerGridComponent` (`features/power-grid/`) — reads the real
   turbine-speed signal, and shows the book's `GridTie` fields
   (frequency, phase angle, breaker, sync) as a structurally separate,
   unconnected field set.

## Investigation

Ch. 21's own finding is a fabricated **relationship**, not a fabricated
value: the book's source computes grid frequency from local turbine RPM
(`hz = 50 + (rpm-3000)/60`) — physically backwards, since a synchronized
grid has one shared frequency every connected turbine tracks, not one any
single turbine sets. Before building, checked the real Instrumentation
domain/Application layers and, per the task's own instruction, went
further and checked the real *seeded data* across the whole solution's
history (migrations, test seed helpers, and every prior live-evidence
session), not just the schema shape:

- **Domain-model level**: Instrumentation has no dedicated property for
  any of active power, reactive power, generator voltage, power factor,
  or turbine speed — it's a fully generic `Signal`/`Measurement` model
  (tag + numeric value + `SignalCategory` lookup), same as every prior
  Instrumentation-backed screen (Reactor cluster, Ageing & Degradation).
  This much was expected.
- **Data level — the actual finding, reported and confirmed with the
  user before building**: across this solution's entire real history,
  only three `SignalCategory` codes had ever existed: `POWER` (reactor
  thermal power, `%RTP`, not electrical grid output), `NEUTRONICS`
  (flux channels), and `VIBRATION` (pump vibration, in Maintenance's own
  tests). **None** of the five requested telemetry quantities had ever
  been seeded, tested, or live-verified — contrary to the initial
  assumption that they'd already exist per prior BFF mapping.
- **Solution-wide grep for grid frequency, point-of-common-coupling
  telemetry, phase angle, breaker state, and sync status**: zero genuine
  hits anywhere, in any bounded context. The only near-miss matches
  (`EngineeringQuantityType.Frequency` enum member, `Signal.ScanRateHz`
  data-acquisition sampling rate, "PolicyGrid" in the unrelated
  ReinforcementLearning context, a `Breaker`-free, `Voltage`-free
  solution) were all confirmed false positives — genuinely nothing
  exists, matching the book's own gap.

**Resolution, decided with the user**: seed one new, genuinely plausible
plant-side signal — turbine shaft speed — the one field the book's own
`GridTie` type keeps as a real measurement, using the same
extend-the-generic-model mechanism as `NEUTRONICS`. Do not fabricate
active power, reactive power, generator voltage, or power factor: they
are declared, on screen and in code, as checked and found absent, same
discipline as every other total-absence gap this arc (Decommissioning,
Waste & Spent Fuel, Zone Access's two screens).

## The GridTie type — no route change needed

No new BFF route: `GET /api/v1/instrumentation/units/{id:int}/signals`
(Program.cs, unchanged) already returns every historized signal for a
unit, unfiltered by category — once a `TURBINE`-category row exists, this
endpoint returns it with zero backend code changes. Confirmed before
building (DI already wired for `GetUnitSignalReadingsQueryHandler`, same
as every prior use of this endpoint).

```ts
// grid-tie.ts
// GUARD, matching the book's own: no function in this file, or anywhere
// in this feature, may derive gridFrequencyHz (or phaseAngleDeg,
// breakerClosed, inSync) from turbineSpeedRpm or from each other. The
// absence of that function is the fix, not a detail below it.
export interface GridTie {
  turbineSpeedRpm: TurbineSpeedReading;      // real, from the TURBINE signal
  gridFrequencyHz: { source: 'awaiting-telemetry' };
  phaseAngleDeg: { source: 'no-source' };
  breakerClosed: { source: 'no-source' };
  inSync: { source: 'no-source' };
}
```

`buildGridTie()` finds the `TURBINE`-category signal in the real signal
list and reads its latest value; the other four fields are constants,
never derived from it or from each other — `grid-tie.spec.ts` includes a
test asserting `gridFrequencyHz` is identical across two different
turbine-speed inputs (2900 RPM vs. 3100 RPM), to make the guard's claim
verifiable, not just asserted in a comment.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `features/power-grid/grid-tie.ts` — pure `GridTie` type + `buildGridTie()`,
  with the no-derivation guard as its own doc comment.
- `features/power-grid/power-grid.ts` — `PowerGridComponent`, reuses the
  existing `InstrumentationApi.getSignals()` client (no new API client
  needed), unit-scoped via the shared `PlantStateService`.
- `.html`/`.scss` — three panels: the real turbine-speed measurement, an
  honest "checked, not present" list for the four unbacked quantities,
  and the `GridTie` panel with `AWAITING TELEMETRY` / `NO SOURCE` pills
  (reusing the existing global `pill.nosource` tone).
- `app.routes.ts` — the single `power` route (Ch. 21 has one screen, not
  a group) now points at `PowerGridComponent` instead of
  `PlaceholderComponent`.

## Tests

```
npx jest power-grid grid-tie → 9/9 passing (new specs alone)
npx jest (full suite)        → 160/160 passing (was 151)
```

- `grid-tie.spec.ts` — real turbine-speed extraction, `no-signal` when
  the category is absent or has never recorded a value, and the
  guard-verifying test above.
- `power-grid.spec.ts` — loading/error/loaded states, fetches the real
  per-unit signals endpoint, builds the `GridTie` correctly, real error
  state on an unreachable endpoint.

Production build:
```
npx ng build → 0 errors, 0 warnings. power-grid compiles to its own
               lazy chunk (~1.95 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently, per the resource-contention lesson.
Before starting the live hosts, available memory was checked (1.62 GB —
lower than the usual ~2.3 GB) and four lingering `dotnet` build-server
processes were found; `dotnet build-server shutdown` freed them,
bringing available memory to 2.23 GB before proceeding.

## Live evidence — real host, real database, real screenshot

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Instrumentation`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                                → Healthy, HTTP 200
GET /api/v1/instrumentation/units/1/signals (before) →
  [{"tag":"UNIT1-NI-001",...,"categoryCode":"NEUTRONICS","latestValue":93.5,...},
   {"tag":"UNIT1-NI-002",...,"categoryCode":"NEUTRONICS","latestValue":null,...}]
```

Seeded, in one batch:

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO CorePlatform.EngineeringUnit (EngineeringUnitId, Symbol, Name, QuantityType, IsDimensionless, IsActive, DisplayOrder, CreatedAtUtc)
  VALUES (3, 'RPM', 'Revolutions per Minute', 'Other', 0, 1, 3, SYSUTCDATETIME());
INSERT INTO Instrumentation.SignalCategory (SignalCategoryId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc)
  VALUES (2, 'TURBINE', 'Turbine', 2, 1, SYSUTCDATETIME());
INSERT INTO Instrumentation.Signal (SignalId, UnitId, SignalTypeId, SignalCategoryId, SignalRoleId, EngineeringUnitId, SamplingModeId, HistorianRetentionClassId, Tag, Name, IsSafetyRelated, IsHistorized, CreatedAtUtc)
  VALUES (3, 1, 1, 2, 1, 3, 1, 1, 'UNIT1-TURB-001', 'Main Turbine Shaft Speed', 0, 1, SYSUTCDATETIME());
INSERT INTO Instrumentation.Measurement (MeasurementId, SignalId, SignalQualityId, MeasurementSourceId, TimestampUtc, NumericValue, IsEstimated, InsertedAtUtc)
  VALUES (9, 3, 1, 1, '2026-08-25T08:00:00', 2998.4, 0, SYSUTCDATETIME());
INSERT INTO Instrumentation.Measurement (MeasurementId, SignalId, SignalQualityId, MeasurementSourceId, TimestampUtc, NumericValue, IsEstimated, InsertedAtUtc)
  VALUES (10, 3, 1, 1, '2026-08-25T09:00:00', 3001.1, 0, SYSUTCDATETIME());
```

`QuantityType 'Other'` was chosen deliberately, not `'Frequency'` — RPM
is a rotational speed, and using the `Frequency` enum member here would
recreate exactly the RPM/Hz conflation this chapter's own finding warns
against. All five inserts succeeded in one batch (no repeat of the
column-count/FK-ordering mistake from the Zone Access slice).

```
GET /api/v1/instrumentation/units/1/signals (after) →
  [...same two NEUTRONICS signals...,
   {"tag":"UNIT1-TURB-001","name":"Main Turbine Shaft Speed","categoryCode":"TURBINE","latestValue":3001.1,"latestQualityCode":"GOOD","latestTimestampUtc":"2026-08-25T09:00:00"}]
```

`latestValue` correctly returns `3001.1` — the later of the two seeded
measurements, confirming `EfActiveHistorizedSignalFinder` picks the most
recent reading, not just the first.

`/power` rendered live (`get_page_text`, no console errors): real
`3001.1 RPM` under "Turbine Shaft Speed", the four checked-absent
quantities each showing `NO DATA IN THIS SYSTEM`, and the Grid Tie panel
showing `AWAITING TELEMETRY` / `NO SOURCE` for frequency/phase/breaker/
sync — matching the built component exactly, live, not just asserted
from the spec.

### Screenshot

- `power-grid.png` — `/power`, full-width shell, sidebar correctly
  highlighting "Power & Grid" active, real turbine-speed pill in green,
  gap pills in the muted "no source" tone, clean layout.

Reviewed directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x2)
```

Two sessions, matching the two composed contexts. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Summary

Checked the real Instrumentation domain and, going further per the
task's own instruction, the real seeded/tested/live-verified data across
the whole solution — and found that none of the five requested telemetry
quantities had ever actually existed, a bigger gap than the initial
"expected yes" assumption. Reported this finding and three options back
before building anything; the user chose the narrowest: seed turbine
speed only, via the same generic-model-extension mechanism already
established for `NEUTRONICS`, and declare the other four quantities as an
honest, checked-and-absent gap rather than fabricate them. Built
`PowerGridComponent` with the real turbine-speed measurement and the
book's own `GridTie` fields kept structurally unconnected to it — the
chapter's central point (a fabricated relationship, not a fabricated
value) enforced by the plain absence of any function that could compute
one from the other, not by a comment alone.
