# Evidence: Angular console, Ch. 22 — Radiation & Safety

## Scope

One real screen, `rad` route, no new BFF route (an existing per-unit
endpoint already returns exactly what's needed):

1. **New real data**: 3 new `RadiationMonitor` rows (Fuel Handling,
   Turbine Hall, Stack Effluent) plus renaming the 2 pre-existing
   demonstrator monitors to their real siting (Containment Interior, Aux
   Building) — 5 genuinely independent monitors total, each with its own
   `RadiationReading`.
2. `RadiationSafetyComponent` (`features/radiation-safety/`) — the
   book's own safety banner, unchanged, plus the 5 independent monitors.

## Investigation

The book's own finding is narrower than a missing concept: its "Area
Radiation Monitors" table shows 5 rows that look like 5 independent
instruments, but its own source computes 4 of them as linear scalings of
only 2 upstream signals (containment dose and reactor power) — admitted
in the source's own inline comment, invisible on the rendered screen.

Checked directly before building, same discipline as the Power & Grid
investigation (real seeded/tested/live data, not just domain shape):

- **Domain layer** (`Nexus1.RadiationMonitoring.Domain`): `RadiationZone`
  (the zone registry, already used for Ch. 20) has no dose/reading value
  at all. But a genuinely **separate** pair of entities exists:
  `RadiationMonitor` (the instrument/siting record — `Code`, `Name`,
  optional `UnitId`/`RadiationZoneId`, `MonitorTypeId`,
  `MonitorStatusId`) and `RadiationReading` (an append-only fact table,
  one `Value` per reading, keyed by `RadiationMonitorId`). This is a real
  per-instrument model, not folded into `RadiationZone` and not routed
  through Instrumentation's generic `Signal`/`Measurement` machinery.
- **Application layer**: `GetLatestReadingPerMonitorQuery` and
  `GetUnitRadiationSafetyQuery` both resolve each monitor's own latest
  `RadiationReading` via a plain per-monitor lookup
  (`EfLatestReadingPerMonitorFinder`: `Where(r => r.RadiationMonitorId ==
  m.Id).OrderByDescending(r => r.TimestampUtc).FirstOrDefault()`) — no
  formula, no cross-monitor arithmetic, no derivation from any other
  signal anywhere in this codebase.
- **Real seeded/tested/live history**: the test seed helper and both
  prior live-evidence sessions (`2026-08-22-bff-radiationmonitoring-safety-slice.md`,
  `2026-08-24-frontend-ch20-zone-access-slice.md`) confirm every real
  `RadiationReading.Value` ever entered was a literal, independent number
  — e.g. the original BFF slice's two monitors (`0.15`/`0.18` for one,
  no readings at all for the other) were manually inserted values, never
  computed from each other.
- **RadiationZone vs. this screen** (task's own question 4): confirmed
  they are genuinely different concepts — `RadiationZone` is a physical
  posting (already shown fleet-wide by the Zone Access screen, Ch. 20);
  `RadiationMonitor`/`RadiationReading` is the actual instrument/reading
  pair this chapter's own finding is about. Not the same thing, and not
  interchangeable.

**Conclusion**: this system's real data model is architecturally
**better** than the book's own shortcut here — it already supports N
genuinely independent monitors with zero scaling required or present
anywhere in the pipeline, not "1 real value scaled 5 ways." The honest
build is to seed 5 real, independent monitors matching the book's own 5
locations, each with its own real reading — not to reproduce the book's
scaling formula in this codebase.

## What was seeded

The 2 pre-existing monitors (`RM-UNIT-1`/`RM-UNIT-1-B`, generic
"Demonstrator..." placeholders from the original RadiationMonitoring BFF
slice) were renamed to real sitings rather than left alongside 3 newly
literally-named ones — their own names already said "demonstrator," never
meant to represent a specific location:

```sql
SET QUOTED_IDENTIFIER ON;
UPDATE RadiationMonitoring.RadiationMonitor SET Code = 'RM-CONT-1', Name = 'Containment Interior Monitor', RadiationZoneId = 2 WHERE RadiationMonitorId = 1;
UPDATE RadiationMonitoring.RadiationMonitor SET Code = 'RM-AUX-1', Name = 'Aux Building Monitor' WHERE RadiationMonitorId = 2;
INSERT INTO RadiationMonitoring.RadiationMonitor (RadiationMonitorId, UnitId, RadiationZoneId, MonitorTypeId, MonitorStatusId, Code, Name, InstalledAtUtc)
  VALUES (3, 1, NULL, 1, 1, 'RM-FUEL-1', 'Fuel Handling Monitor', SYSUTCDATETIME());
INSERT INTO RadiationMonitoring.RadiationMonitor (RadiationMonitorId, UnitId, RadiationZoneId, MonitorTypeId, MonitorStatusId, Code, Name, InstalledAtUtc)
  VALUES (4, 1, NULL, 1, 1, 'RM-TURB-1', 'Turbine Hall Monitor', SYSUTCDATETIME());
INSERT INTO RadiationMonitoring.RadiationMonitor (RadiationMonitorId, UnitId, RadiationZoneId, MonitorTypeId, MonitorStatusId, Code, Name, InstalledAtUtc)
  VALUES (5, 1, NULL, 1, 1, 'RM-STACK-1', 'Stack Effluent Monitor', SYSUTCDATETIME());
-- + 4 new independent RadiationReading rows (RM-AUX-1: 0.42, RM-FUEL-1: 3.1,
--   RM-TURB-1: 0.08, RM-STACK-1: 0.015 uSv/h) and 1 new reading (14.2) on
--   the renamed Containment Interior monitor, on top of its 2 pre-existing
--   readings, to confirm "latest wins" still holds after the rename.
```

`RM-CONT-1` was linked to `RadiationZoneId = 2` (`ZONE-CONTAINMENT-1`,
classification `HIGH`, seeded in the Zone Access slice) — a real,
thematically-consistent siting; the other 4 monitors were left
unposted (`RadiationZoneId = NULL`), which is realistic — not every
physical instrument sits inside a formally posted zone.

## No new BFF route needed

`GET /api/v1/radiation-monitoring/units/{id:int}` (Program.cs, unchanged)
already wraps `GetUnitRadiationSafetyQueryHandler` and returns
`UnitRadiationSafetyDto(UnitId, Monitors, Zones)` — every field this
screen needs, with zero backend code changes. This same handler already
backs the Overview screen's composite `/overview` endpoint (Ch. 6), so
the DTO shape and DI wiring were already fully proven before this slice.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `core/api/radiation-safety-api.ts` — a dedicated client for the direct
  per-unit endpoint (mirrors `UnitRadiationSafetyDto`), rather than
  reusing the heavier composite `/overview` response Ch. 6's Overview
  screen already folds this same data into — same "call the real
  capability directly where a screen only needs it" pattern as
  `radiation-zones-api.ts` versus that same composite endpoint's `Zones`
  field.
- `features/radiation-safety/radiation-safety.ts/.html/.scss` — the
  book's own safety-banner wording, unchanged, plus the 5 independent
  monitors, each showing its own real value, unit, and status. Zones are
  deliberately NOT repeated on this screen: the same real per-unit
  `RadiationZone` data is already shown fleet-wide by the Zone Access
  screen (Ch. 20) — showing it again here would be a duplicate view over
  the same real rows, not a second real capability.
- `app.routes.ts` — the single `rad` route now points at
  `RadiationSafetyComponent` instead of `PlaceholderComponent`.

## Tests

```
npx jest radiation-safety → 4/4 passing (new specs alone)
npx jest (full suite)     → 164/164 passing (was 160)
```

- Loading/error/loaded states, fetches the real per-unit endpoint.
- Each monitor's `latestValue` is asserted as its own independent number
  (not derived from another monitor's).
- Real no-reading state for a monitor with no value (not fabricated).
- Real error state on an unreachable endpoint.

Production build:
```
npx ng build → 0 errors, 0 warnings. radiation-safety compiles to its
               own lazy chunk (~1.74 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently. Before starting the live hosts, available
memory was checked (1.44 GB — low again) and `dotnet build-server
shutdown` was run, bringing it to 1.97 GB before proceeding — same
lingering-build-server pattern as the Power & Grid slice.

## Live evidence — real host, real database, real screenshot

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=RadiationMonitoring`, `__1=ReactorFleet`; `ng
serve --port 4200` alongside it.

```
GET /health/ready                                     → Healthy, HTTP 200
GET /api/v1/radiation-monitoring/units/1 (before) →
  monitors: RM-UNIT-1 (0.18 uSv/h), RM-UNIT-1-B (no reading)
```

After the rename/seed above:

```
GET /api/v1/radiation-monitoring/units/1 (after) →
  RM-CONT-1  "Containment Interior Monitor"  14.2   uSv/h
  RM-AUX-1   "Aux Building Monitor"           0.42  uSv/h
  RM-FUEL-1  "Fuel Handling Monitor"          3.1   uSv/h
  RM-TURB-1  "Turbine Hall Monitor"           0.08  uSv/h
  RM-STACK-1 "Stack Effluent Monitor"         0.015 uSv/h
```

Five real, independently-valued monitors returned live — matching the
book's own 5-row shape, with none of them a scaled function of another.

`/rad` rendered live (`get_page_text`, no console errors): the safety
banner exactly as specified, and all 5 monitors with their real values,
matching the built component exactly, live, not just asserted from the
spec.

### Screenshot

- `radiation-safety.png` — `/rad`, full-width shell, sidebar correctly
  highlighting "Radiation / Safety" active, the safety banner in a
  prominent red/critical tone, all 5 monitors listed with their real
  values in green "operational" pills, clean layout.

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

Checked the real RadiationMonitoring domain and its actual
seeded/tested/live-verified data, and found the model already supports
what the book's own source only fakes: N genuinely independent monitors,
each with its own independently-entered reading, no scaling formula
anywhere in the pipeline. Renamed the 2 pre-existing demonstrator
monitors to real sitings and added 3 more, giving 5 real, independent
readings matching the book's own 5-location table — the honest version
of the screen the book's own inline comment admits it didn't build.
