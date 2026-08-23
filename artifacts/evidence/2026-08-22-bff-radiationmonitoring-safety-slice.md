# Evidence: BFF fourth vertical slice — RadiationMonitoring, Radiation & Safety (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a fourth vertical slice, composed alongside
ReactorFleet, AlarmManagement, and DigitalTwin in the same host:

- `GET /api/v1/radiation-monitoring/units/{id}` — per-unit ambient monitor
  readings and zone classification for a "Radiation & Safety" screen.

No new ADR — recorded inline, same as the last two slices.

## 1. What RadiationMonitoring's Application layer already exposed

Checked before writing any endpoint. A rich existing surface, all fleet-wide:
`GetActiveRadiationZonesQuery` (atlas C.13.5.2 query 1), `GetLatestReadingPerMonitorQuery`
(query 3), `GetMonitorsWithCalibrationDueQuery`, `GetOpenDoseAlertsQuery`
(query 4), plus two commands (`RegisterRadiationZoneCommand`,
`RecordRadiationReadingCommand`).

None scoped to a single unit. Same pattern as the last two slices: added
minimal sibling methods rather than reusing the fleet-wide ones as-is:

- `ILatestReadingPerMonitorFinder.GetLatestReadingsForUnitAsync(int unitId, ...)`
  — new, alongside the existing `GetLatestReadingsAsync()`.
- `IActiveRadiationZonesFinder.GetActiveRadiationZonesForUnitAsync(int unitId, ...)`
  — new, alongside the existing `GetActiveRadiationZonesAsync()`.
- `GetUnitRadiationSafetyQuery(int UnitId)` / `GetUnitRadiationSafetyQueryHandler`
  — combines both finders into one screen-shaped response.
- New DTOs: `UnitRadiationMonitorReadingDto`, `UnitRadiationZoneDto`,
  `UnitRadiationSafetyDto`.

`RadiationMonitor.UnitId` and `RadiationZone.UnitId` are both **direct**
nullable FKs to `ReactorFleet.Unit` — no multi-hop join was needed here,
unlike DigitalTwin's divergence gap. `GetLatestReadingsForUnitAsync`
deliberately does **not** exclude a monitor with zero readings (the
fleet-wide `GetLatestReadingsAsync` does, via `where latest != null`) — a
per-unit safety screen should show a sited monitor that hasn't reported yet,
not hide it. Confirmed live below (`RM-UNIT-1-B`).

**Translation-safety note**: `EfLatestReadingPerMonitorFinder`'s own file
comment already records that `GroupBy+OrderByDescending+First` failed to
translate for Robotics' equivalent finder. To stay clear of that failure
mode, the per-unit lookup codes (engineering-unit symbol, quality code) are
resolved via two small dictionary reads *after* materializing the ordered
correlated-subquery results, rather than joining inside an ordered subquery
— confirmed to build and, more importantly, to actually execute correctly
against the real database (see live evidence below), not just to compile.

## 2. Hosted-service check

Read `RadiationMonitoring.Infrastructure`'s `ServiceCollectionExtensions`
directly: zero `AddHostedService<...>()` calls, same as DigitalTwin. This is
also a Phase 2 sector (ADR-027, no messaging/observability wiring), and this
was confirmed by reading the file, not assumed from the DigitalTwin
precedent holding twice in a row.

## 3. The endpoint, and the named gap

`GET /api/v1/radiation-monitoring/units/{id}` returns, per unit: every
radiation monitor sited there (code, name, status, latest reading if any)
and every radiation zone anchored there (code, name, classification,
status) — all real columns.

**Named gap, the main finding of this slice**: the domain model has **no
concept of "dose for a unit" at all.** "Dose" in this codebase is tracked
per **person** — `Dosimeter`, `PersonDosimeterAssignment`, `PersonDoseReading`,
and `DoseAlert` all key off a worker/person, never a unit. `GetOpenDoseAlertsQuery`'s
own DTO projects `PersonId`, not any unit reference. A "Radiation & Safety"
screen showing "current dose... for a unit" was the task's own framing, but
there is nothing in this domain model that maps onto it — a unit doesn't
have a dose, a person does. What genuinely is unit-scoped, and is what this
endpoint returns instead, is **ambient radiation levels** (monitor readings)
and **zone classification** — a real and different kind of safety data, not
a substitute for personnel dosimetry. This is recorded here explicitly
rather than either fabricating a "unit dose" field or silently omitting any
explanation of why dose data isn't present.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all three prior slices — zero regressions.

## Real host, real database — live evidence

Memory checked before starting the host (2.63 GB, confirmed stable across
two checks) — well above the ~1.7 GB threshold. Rechecked after host start
(2.42 GB) and again before the endpoint call (2.44 GB) — stable throughout,
no incident this run.

`RadiationMonitor`, `RadiationZone`, `RadiationReading` all had **zero
rows**, and `CorePlatform.EngineeringUnit` (needed as `RadiationReading`'s
NOT NULL FK) had zero rows too — no dev-run residue for this context at all,
one level deeper than DigitalTwin's case. Seeded minimal real data:

```sql
-- Lookups
INSERT INTO RadiationMonitoring.MonitorType (...) VALUES (1, 'GEIGER', 'Geiger-Muller Detector', ...);
INSERT INTO RadiationMonitoring.MonitorStatus (...) VALUES (1, 'OPERATIONAL', 'Operational', ...);
INSERT INTO RadiationMonitoring.RadiationZoneType (...) VALUES (1, 'CONTROLLED', 'Controlled Area', ...);
INSERT INTO RadiationMonitoring.RadiationZoneStatus (...) VALUES (1, 'POSTED', 'Posted', ...);
INSERT INTO RadiationMonitoring.RadiationAreaClassification (...) VALUES (1, 'LOW', 'Low Radiation Area', ...);
INSERT INTO RadiationMonitoring.MeasurementType (...) VALUES (1, 'DOSE-RATE', 'Dose Rate', ...);
INSERT INTO RadiationMonitoring.MeasurementQuality (...) VALUES (1, 'GOOD', 'Good', ...);
INSERT INTO CorePlatform.EngineeringUnit (...) VALUES (1, 'uSv/h', 'Microsievert per hour', 'RADIATION_DOSE_RATE', 0, 1, 1, ...);

-- Two monitors for UNIT-1: one with readings, one without (to exercise the nullable path)
INSERT INTO RadiationMonitoring.RadiationMonitor (RadiationMonitorId, UnitId, MonitorTypeId, MonitorStatusId, Code, Name)
  VALUES (1, 1, 1, 1, 'RM-UNIT-1', 'Demonstrator Radiation Monitor for Unit 1');
INSERT INTO RadiationMonitoring.RadiationMonitor (RadiationMonitorId, UnitId, MonitorTypeId, MonitorStatusId, Code, Name)
  VALUES (2, 1, 1, 1, 'RM-UNIT-1-B', 'Demonstrator Radiation Monitor B for Unit 1 (no readings yet)');
INSERT INTO RadiationMonitoring.RadiationReading (RadiationReadingId, RadiationMonitorId, MeasurementTypeId, EngineeringUnitId, MeasurementQualityId, TimestampUtc, Value)
  VALUES (1, 1, 1, 1, 1, '2026-08-22T09:00:00', 0.15);
INSERT INTO RadiationMonitoring.RadiationReading (RadiationReadingId, RadiationMonitorId, MeasurementTypeId, EngineeringUnitId, MeasurementQualityId, TimestampUtc, Value)
  VALUES (2, 1, 1, 1, 1, '2026-08-22T10:00:00', 0.18);
INSERT INTO RadiationMonitoring.RadiationZone (RadiationZoneId, UnitId, RadiationZoneTypeId, RadiationZoneStatusId, RadiationAreaClassificationId, Code, Name)
  VALUES (1, 1, 1, 1, 1, 'ZONE-UNIT-1', 'Demonstrator Zone for Unit 1');
```

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/radiation-monitoring/units/1`

```json
{
  "unitId": 1,
  "monitors": [
    {"monitorCode":"RM-UNIT-1","monitorName":"Demonstrator Radiation Monitor for Unit 1","monitorStatus":"OPERATIONAL","latestValue":0.180000,"engineeringUnitSymbol":"uSv/h","quality":"GOOD","latestReadingAtUtc":"2026-08-22T10:00:00"},
    {"monitorCode":"RM-UNIT-1-B","monitorName":"Demonstrator Radiation Monitor B for Unit 1 (no readings yet)","monitorStatus":"OPERATIONAL","latestValue":null,"engineeringUnitSymbol":null,"quality":null,"latestReadingAtUtc":null}
  ],
  "zones": [
    {"code":"ZONE-UNIT-1","name":"Demonstrator Zone for Unit 1","classification":"LOW","status":"POSTED"}
  ]
}
```

HTTP 200. Confirms: (a) the "latest" reading is genuinely the most recent —
`0.18` at `10:00`, not `0.15` at `09:00`, proving the `OrderByDescending`
correlated subquery picked correctly, not just the first row inserted; (b)
the unreported monitor `RM-UNIT-1-B` appears with null fields rather than
being silently excluded.

### `GET /api/v1/radiation-monitoring/units/999` (unit with no data at all)

```json
{"unitId":999,"monitors":[],"zones":[]}
```

HTTP 200 — empty lists, not an error.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Confirmed — `nexus1_app`, no fallback.

Host stopped cleanly; `sys.databases` confirmed all `ONLINE` afterward.

## Summary

Four vertical slices now exist in `Nexus1.Bff`:

- **ReactorFleet** — read-only.
- **AlarmManagement** — read + write, messaging question settled.
- **DigitalTwin** — read-only, named gap (no per-unit divergence data).
- **RadiationMonitoring** — read-only, named gap (no unit-level "dose"
  concept exists at all — dose is a person concept in this domain model;
  ambient monitor/zone data is what's genuinely unit-scoped).

Pattern holds across four contexts. The recurring shape across all three
Phase 2 slices (DigitalTwin, RadiationMonitoring, and implicitly any future
one): rich fleet-wide query surface already exists, nothing is scoped to a
single unit, so a minimal sibling method is the right-sized addition each
time — and each context has surfaced its own genuine, differently-shaped gap
rather than the same gap repeating.
