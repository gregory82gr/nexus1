# Evidence: Angular console, Ch. 18 — Plant Lifecycle (Ageing & Degradation only)

## Scope

One real screen plus a genuine, minimal backend addition:

1. **BFF**: `GET /api/v1/maintenance/degradation-cases` — a new thin route
   wrapping the existing `GetActiveDegradationCasesQueryHandler`/
   `ActiveDegradationCaseDto`, same shape as the Absence Stress Test
   precedent.
2. `AgeingDegradationComponent` (`features/ageing-degradation/`) — serves
   `aging`, wired to the new route.

Decommissioning and Waste & Spent Fuel are **not built** — confirmed
total absence in the domain, named as a real gap, not silently dropped.

## Investigation: the book's own source has nothing either, and its real argument is about progress bars, not data

Ch. 18 states its own boundary as plainly as Ch. 16/17 did: *"Volume III
has no ageing, decommissioning, or inventory endpoints. Every figure on
these three screens is generated from the unit's commissioning year and
simulated operating time."* So the book's own source material has zero
real data behind all three screens too — a "Life consumed" percentage
computed as `calendar age × limit × a seeded random constant`.

But the chapter's real argument isn't about missing data — it's that a
**progress bar** is the wrong component for this class of quantity
regardless of whether the underlying number is real. A bar filled to a
labelled limit asserts four things at once: a defined endpoint, a
continuously-measured current position, monotonic movement toward it,
and a position known *now*. None of those hold for vessel embrittlement,
measured only at decade-spaced surveillance-capsule withdrawals and
interpolated between them. The chapter's fix is structural, not
cosmetic: `AgeingSeries` has no `currentValue` accessor at all, only
`lastMeasured()` (which forces a date onto every reading), and the
honest chart shows *measured points as points, the limit as a line, and
the gap since the last measurement as an explicit widening projection* —
never a percentage.

**Real backend facts, checked directly**:
- `ActiveDegradationCaseDto(AssetCode, Mechanism, Severity, DetectedAtUtc,
  TrendPoints)` already existed in the Application layer, already
  registered in DI (`IActiveDegradationCasesFinder`/
  `EfActiveDegradationCasesFinder`), zero infrastructure changes needed —
  only a route was missing, the same shape as the Absence Stress Test
  precedent.
- Genuinely fleet-wide: `GetActiveDegradationCasesQuery` takes no
  parameter at all (not per-unit, not per-department).
- `TrendPoints` is a **count** of `DegradationTrendPoint` rows, not the
  individual measured values — and there is no limit/threshold field
  anywhere in this DTO. No per-case trend-detail query exists in this
  codebase to retrieve the actual point values.
- Decommissioning and Waste & Spent Fuel: confirmed directly (per the
  already-known finding) — no entity, table, or concept anywhere in
  `Nexus1.Maintenance.Domain`. A total-absence gap, the same shape as
  Security's own zone-access finding, not missing fields on an
  otherwise-shaped model.

**Decision applied**: add the thin BFF route (same minimal pattern as
every prior slice's own addition) and build Ageing & Degradation for
real — but honestly narrower than the book's own chart. The real data
supports exactly the alternative the book itself prefers to a
percentage — *"each shows its last real reading and how many exist —
not a percentage. A count of data points is itself information the bar
was hiding"* — but not the fuller measured-points-vs-limit-line chart
with a projection band, since no per-point values or thresholds are
exposed. Named that scope reduction explicitly on the screen and in
code, the same restraint as Model Analysis's own "narrower than the
book's six-group solver audit" note. Decline Decommissioning/Waste &
Spent Fuel entirely, same as Rod Type/Film.

## The new BFF route

```csharp
app.MapGet("/api/v1/maintenance/degradation-cases", async ([FromServices] GetActiveDegradationCasesQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetActiveDegradationCasesQuery(), cancellationToken);
    return Results.Ok(result.Value);
});
```

No `{id}` parameter at all — the query itself takes none. No DI/
infrastructure changes needed.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as every prior slice — confirmed genuinely unchanged.

## Frontend: what was built

- `core/api/maintenance-api.ts` — extended with `ActiveDegradationCase`
  (mirrors `ActiveDegradationCaseDto` exactly) and
  `MaintenanceApi.getActiveDegradationCases()`.
- `features/ageing-degradation/severity-tone.ts` — pure, conservative
  keyword mapping (same discipline as `plant-3d`'s `statusTone` and
  `reactor-kinetics`'s `power-signal`): `Severity` is a free-text
  lookup-table code, so an unrecognized string maps to `unknown`, never
  guessed into `ok`/`warn`/`crit`.
- `features/ageing-degradation/ageing-degradation.ts/.html/.scss` —
  loading/error/loaded state over the real endpoint; renders each
  case's asset code, mechanism, severity pill, detected date, and
  trend-point count as a count (`"3 measured points"`), never a
  percentage or bar.

## Tests

```
npx jest → 136/136 passing (was 128; 8 new specs)
```

- `severity-tone.spec.ts` — high/critical/severe → crit,
  medium/moderate → warn, low/minor → ok, an unrecognized string →
  unknown (never guessed).
- `ageing-degradation.spec.ts` — loading/error/loaded states, fetches
  the fleet-wide endpoint with no id parameter, renders the real
  trend-point count and severity tone, an honest empty state for zero
  active cases (not fake data), and a real error state when the
  endpoint is unreachable.

Production build:

```
npx ng build → 0 errors, 0 warnings. ageing-degradation compiles to its
               own small lazy chunk (~1.8 KB transfer).
```

One real environment finding along the way, not a code defect: running
the full Jest suite concurrently with the full `dotnet test` run in the
background crashed a Jest worker process (a bare native stack trace, no
test output) under the combined memory pressure of both processes
running at once. Re-ran Jest alone once the .NET suite had finished —
136/136 passed cleanly. Recorded here as a resource-contention artifact
of this environment, not a test failure, and as a reminder not to run
the two heavy suites fully concurrently going forward.

## Live evidence — real host, real database, real screenshot

Memory checked before starting both processes (2.69 GB, healthy).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Maintenance`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                              → Healthy, HTTP 200
GET /api/v1/maintenance/degradation-cases (before seeding) → []
```

Confirmed the new route's genuine "no active cases" behavior live,
before seeding anything.

**Seeded real degradation data**, reusing Assets 1/2 already seeded for
the Rod Inspection cluster and EngineeringUnit 1 already seeded for
RadiationMonitoring — no new assets or engineering units invented:

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO Maintenance.DegradationMechanism (DegradationMechanismId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (1, 'CORROSION', 'Corrosion', 1, 1, SYSUTCDATETIME());
INSERT INTO Maintenance.DegradationMechanism (DegradationMechanismId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (2, 'FATIGUE', 'Fatigue', 2, 1, SYSUTCDATETIME());
INSERT INTO Maintenance.FindingSeverity (FindingSeverityId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (1, 'LOW', 'Low', 1, 1, SYSUTCDATETIME());
INSERT INTO Maintenance.FindingSeverity (FindingSeverityId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (2, 'HIGH', 'High', 2, 1, SYSUTCDATETIME());
INSERT INTO Maintenance.DegradationRecord (DegradationRecordId, AssetId, DegradationMechanismId, FindingSeverityId, DetectedAtUtc, Description, IsActive) VALUES (1, 1, 1, 2, '2026-08-20T09:00:00', 'Localized corrosion pitting observed on pump casing during scheduled inspection.', 1);
INSERT INTO Maintenance.DegradationRecord (DegradationRecordId, AssetId, DegradationMechanismId, FindingSeverityId, DetectedAtUtc, Description, IsActive) VALUES (2, 2, 2, 1, '2026-08-18T09:00:00', 'Minor fatigue indication noted on valve stem, within tolerance.', 1);
INSERT INTO Maintenance.DegradationTrendPoint (DegradationTrendPointId, DegradationRecordId, EngineeringUnitId, MeasuredAtUtc, Value) VALUES (1, 1, 1, '2026-08-01T00:00:00', 0.4);
INSERT INTO Maintenance.DegradationTrendPoint (DegradationTrendPointId, DegradationRecordId, EngineeringUnitId, MeasuredAtUtc, Value) VALUES (2, 1, 1, '2026-08-10T00:00:00', 0.6);
INSERT INTO Maintenance.DegradationTrendPoint (DegradationTrendPointId, DegradationRecordId, EngineeringUnitId, MeasuredAtUtc, Value) VALUES (3, 1, 1, '2026-08-20T00:00:00', 0.9);
INSERT INTO Maintenance.DegradationTrendPoint (DegradationTrendPointId, DegradationRecordId, EngineeringUnitId, MeasuredAtUtc, Value) VALUES (4, 2, 1, '2026-08-18T00:00:00', 0.1);
```

```
GET /api/v1/maintenance/degradation-cases (after seeding) →
  [{"assetCode":"ASSET-UNIT1-001","mechanism":"CORROSION","severity":"HIGH","detectedAtUtc":"2026-08-20T09:00:00","trendPoints":3},
   {"assetCode":"ASSET-UNIT1-002","mechanism":"FATIGUE","severity":"LOW","detectedAtUtc":"2026-08-18T09:00:00","trendPoints":1}]
```

Trend-point counts (3 and 1) computed correctly by the real query's own
`Count(tp => tp.DegradationRecordId == d.Id)` subquery.

`/aging` rendered live (`get_page_text`, no console errors): `2 across
the fleet`, `ASSET-UNIT1-001` / `CORROSION` / `HIGH` / `3 MEASURED
POINTS`, `ASSET-UNIT1-002` / `FATIGUE` / `LOW` / `1 MEASURED POINT` —
singular/plural handled correctly, matching the seeded data exactly.

### Screenshot

- `ageing-degradation.png` — the no-progress-bars note, both cases with
  correctly-toned severity pills (HIGH red, LOW green) and real
  trend-point counts.

Reviewed directly: full-width shell, no regression, no cramped columns.
The sidebar's own active-state fix (from an earlier slice) again
generalizes correctly to this new nav item.

Login/session verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

One session this check (connection-pool reuse varies run to run — not
all composed contexts necessarily hold an open session at the same
instant, consistent with prior slices' own noted variance). Both
processes stopped cleanly after capture; `sys.databases` confirmed all 9
databases `ONLINE` afterward.

## Summary

Added one thin, real BFF route wrapping an Application-layer capability
that already existed and was already registered — the same minimal
shape as the Absence Stress Test precedent, with 869/869 tests confirmed
unchanged. Built Ageing & Degradation honestly around what that real
data supports: a case list with real severity/mechanism/detection-date
and a trend-point count, deliberately not the book's own fuller
measured-points-vs-limit chart, since no per-point values or thresholds
are exposed by this query — named as a real scope reduction, not a
silent one. Decommissioning and Waste & Spent Fuel were confirmed absent
and declined, matching the Rod Type/Film precedent.
