# Evidence: BFF seventh vertical slice — Instrumentation, Reactor sub-screens, grouping 1 of 2 (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with the first of two Instrumentation groupings,
composed alongside all six existing contexts in the same host:

- `GET /api/v1/instrumentation/units/{id}/signals` — one endpoint covering
  six of the book's seven Reactor screens (Core, Control Rods, Kinetics,
  Neutronics, Coolant/TH, Steam Generators).

No new ADR — recorded inline, same as the prior slices. Reported separately
per the task's own walking-skeleton instruction, before building the second
grouping (Model Analysis).

## 1. What Instrumentation's Application layer already exposed

A rich existing surface: `GetActiveHistorizedSignalsForUnitQuery` (per unit,
but keyed by `UnitCode` string, not int), `GetLatestMeasurementsForTagQuery`
(per one specific tag, TOP-N history), `GetOpenSignalQualityEventsForUnitQuery`
(per unit, data-quality issues), `GetAcquisitionPathForTagQuery` (per tag,
wiring path), plus three commands.

**The central finding of this slice**: checked the domain model before
assuming the book's seven Reactor sub-screens map to seven real groupings —
they do not. `Signal` and `Measurement` are Instrumentation's *entire*
domain model for readings: a generic tagged point (`Signal`, with `Tag`,
`Name`, `SignalCategoryId`, `UnitId`) and a generic time-series fact table
(`Measurement`, with `SignalId`, `TimestampUtc`, `NumericValue`, quality,
source). There is no `CoreState`, `ControlRodPosition`, `ReactivityMeasurement`,
`CoolantReading`, or `SteamGeneratorReading` entity anywhere in this
context — every one of those six screens would just be a *filtered view*
over the same `Signal`/`Measurement` rows, distinguished only by
`SignalCategory` (a data-content lookup, not a domain-modeled subsystem).

Built **one** endpoint for all six screens, not six endpoints manufactured
to match the book's screen count — exactly what the task asked for if the
real model didn't support seven distinct groupings. `CategoryCode` is
included in the DTO so a client-side view can group/filter if the seed data
happens to distinguish subsystems by category, but that's data content, not
something this codebase's domain model enforces.

**Added, same minimal-sibling pattern as every prior slice**:
`IActiveHistorizedSignalFinder.GetSignalReadingsForUnitAsync(int unitId, ...)`
— unlike the existing `GetByUnitCodeAsync(string unitCode)`, this is keyed
by the int `UnitId` directly (matching every other BFF route's `{id:int}`
convention, and filtering `Signal.UnitId` directly rather than joining
through the unit-code shadow reference), and it also includes each signal's
latest measurement, which the existing query doesn't. New
`GetUnitSignalReadingsQuery`/Handler, new `UnitSignalReadingDto`.

## 2. Hosted-service check

Read `Instrumentation.Infrastructure`'s `ServiceCollectionExtensions`
directly: zero `AddHostedService<...>()` calls — same as DigitalTwin,
RadiationMonitoring, and Robotics. Instrumentation is Phase 2 (ADR-019), no
messaging/observability wiring (ADR-027). Confirmed by reading the file,
not assumed from the precedent holding three times already.

## Translation safety

Lookup codes (measurement quality) are resolved via a small in-memory
dictionary pass after materializing the ordered correlated-subquery
results, not joined inside the ordered subquery — same discipline as
RadiationMonitoring's and Robotics' per-unit finders, guarding against the
EF translation failure this project has already hit once
(`GroupBy+OrderByDescending+First` not translating, originally found in
Robotics' own finder).

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

(One transient MSBuild file-lock warning — `MSB3101` on an unrelated
`Nexus1.RootCause.Domain` obj-cache file — appeared on the first build
attempt and did not reproduce on a clean rebuild; not a code warning, not
related to any file this slice touched.)

Same 869 baseline as all six prior slices — zero regressions.

## Real host, real database — live evidence

Memory checked before starting the host (2.82 GB, confirmed stable across
two checks — 2.82 → 2.72 GB). Rechecked after host start (2.63 GB) and
before the endpoint call (2.85 GB) — stable throughout, no incident this
run.

`Instrumentation.Signal` and `Instrumentation.Measurement` both had **zero
rows** — same pattern as DigitalTwin/RadiationMonitoring/Robotics. Seeded
minimal real data (reusing `CorePlatform.EngineeringUnit` id 1 seeded
earlier for RadiationMonitoring, and adding a second, more topically
appropriate one, `%RTP`):

```sql
-- Lookups (Code/Name/DisplayOrder/IsActive/CreatedAtUtc, no defaults exist
-- on these tables in this context, unlike some other sectors — all supplied explicitly)
INSERT INTO Instrumentation.SignalType (...) VALUES (1, 'ANALOG', 'Analog', ...);
INSERT INTO Instrumentation.SignalCategory (...) VALUES (1, 'NEUTRONICS', 'Neutronics', ...);
INSERT INTO Instrumentation.SignalRole (...) VALUES (1, 'PROCESS', 'Process Variable', ...);
INSERT INTO Instrumentation.SamplingMode (...) VALUES (1, 'CONTINUOUS', 'Continuous', ...);
INSERT INTO Instrumentation.HistorianRetentionClass (...) VALUES (1, 'STANDARD', 'Standard Retention', ...);
INSERT INTO Instrumentation.SignalQuality (...) VALUES (1, 'GOOD', 'Good', ...);
INSERT INTO Instrumentation.MeasurementSource (...) VALUES (1, 'HISTORIAN', 'Historian', ...);
INSERT INTO CorePlatform.EngineeringUnit (EngineeringUnitId, Symbol, Name, QuantityType, IsDimensionless, IsActive, DisplayOrder, CreatedAtUtc)
  VALUES (2, '%RTP', 'Percent Rated Thermal Power', 'POWER_FRACTION', 0, 1, 2, ...);

-- Two signals for UNIT-1: one with two measurements, one with none
INSERT INTO Instrumentation.Signal (SignalId, UnitId, SignalTypeId, SignalCategoryId, SignalRoleId, EngineeringUnitId, SamplingModeId, HistorianRetentionClassId, Tag, Name, IsSafetyRelated, IsHistorized, CreatedAtUtc)
  VALUES (1, 1, 1, 1, 1, 2, 1, 1, 'UNIT1-NI-001', 'Neutron Flux Channel 1', 1, 1, ...);
INSERT INTO Instrumentation.Signal (...)
  VALUES (2, 1, 1, 1, 1, 2, 1, 1, 'UNIT1-NI-002', 'Neutron Flux Channel 2 (no readings yet)', 1, 1, ...);
INSERT INTO Instrumentation.Measurement (MeasurementId, SignalId, SignalQualityId, MeasurementSourceId, TimestampUtc, NumericValue, IsEstimated, InsertedAtUtc)
  VALUES (1, 1, 1, 1, '2026-08-22T09:00:00', 98.5, 0, ...);
INSERT INTO Instrumentation.Measurement (...)
  VALUES (2, 1, 1, 1, '2026-08-22T10:00:00', 99.2, 0, ...);
```

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/instrumentation/units/1/signals`

```json
[
  {"tag":"UNIT1-NI-001","name":"Neutron Flux Channel 1","categoryCode":"NEUTRONICS","latestValue":99.2,"latestQualityCode":"GOOD","latestTimestampUtc":"2026-08-22T10:00:00"},
  {"tag":"UNIT1-NI-002","name":"Neutron Flux Channel 2 (no readings yet)","categoryCode":"NEUTRONICS","latestValue":null,"latestQualityCode":null,"latestTimestampUtc":null}
]
```

HTTP 200. Confirms: (a) the latest value is genuinely the most recent —
`99.2` at `10:00`, not `98.5` at `09:00`; (b) the signal with zero
measurements appears with null reading fields rather than being excluded.

### `GET /api/v1/instrumentation/units/999/signals` (unit with no signals)

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
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x4)
```

Four sessions this check (connection pool reuse varies run to run; not all
six composed contexts necessarily hold an open session at the same instant
— this has been observed before, e.g. RadiationMonitoring's own evidence
showed 1 rather than 3). Confirmed — `nexus1_app`, no fallback.

Host stopped cleanly; `sys.databases` confirmed all `ONLINE` afterward.

## Summary so far

Seven vertical slices now exist in `Nexus1.Bff` (counting Instrumentation).
This first Instrumentation grouping covers six of the book's seven Reactor
screens with one honest endpoint, rather than manufacturing six to match
the screen count. The second grouping — Model Analysis, mapped to
Instrumentation's own real "verification" concept (signal-quality/data-trust
events, `GetOpenSignalQualityEventsForUnitQuery`) — follows as a separate
piece of work with its own evidence, per the walking-skeleton instruction
for this task.
