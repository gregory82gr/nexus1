# Evidence: BFF third vertical slice — DigitalTwin, Plant 3D View (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a third vertical slice, composed alongside
ReactorFleet and AlarmManagement in the same host:

- `GET /api/v1/digital-twin/units/{id}` — per-unit twin state for a "Plant 3D
  View" screen.

No new ADR — recorded inline, same as the AlarmManagement slice.

## 1. What DigitalTwin's Application layer exposed before this task

Checked before writing any endpoint, per the task's instruction. Unlike
ReactorFleet (zero queries before its first slice), DigitalTwin already had
a substantial Application layer: `GetActiveTwinsForFleetQuery` (fleet-wide
active twins — atlas C.6.8 query 1), `GetOpenDivergencesQuery` (fleet-wide
open divergences — atlas C.6.8 query 3), `TraceModelVariableToSignalQuery`,
plus three commands (`CaptureTwinSnapshotCommand`, `RecordTwinDivergenceCommand`,
`ReviewTwinDivergenceCommand`).

None of these are scoped to a single unit — `GetActiveTwinsForFleetQuery`
returns every active twin across the whole fleet, and `ActiveTwinDto` doesn't
even carry a unit id (only `UnitCode`, no int). So, same as ReactorFleet and
AlarmManagement's fleet-wide/per-unit mismatches, a minimal sibling query was
added rather than reusing the existing one as-is:

- `IActiveTwinFinder.GetActiveTwinsForUnitAsync(int unitId, ...)` — added
  alongside the existing `GetActiveTwinsAsync()`, same interface, same
  pattern as `IAlarmEventFinder.GetAllActiveAsync`.
- `GetUnitTwinStateQuery(int UnitId)` / `GetUnitTwinStateQueryHandler`.
- `UnitTwinStateDto` — `UnitId`, `UnitCode`, `TwinCode`, `TwinName`,
  `ModelType`, `Status`, `Fidelity`, `IsAuthoritative`. `IsAuthoritative` is
  included because a unit can genuinely have more than one active,
  non-deleted twin model, and it's the domain's own way of saying which one
  is the live one — not fabricated, a real existing column.
- `EfActiveTwinFinder.GetActiveTwinsForUnitAsync` — the exact same
  four-way join `GetActiveTwinsAsync` already used (TwinModel →
  ReactorFleetUnitReference → TwinModelType → TwinModelStatus →
  TwinFidelityLevel), with one added `tm.UnitId == unitId` filter.

## 2. Hosted-service check (the AlarmManagement-outbox question, asked again for this context)

Checked `Nexus1.DigitalTwin.Infrastructure`'s `ServiceCollectionExtensions`
directly: **zero `AddHostedService<...>()` calls, zero outbox writer, zero
messaging dependency of any kind.** DigitalTwin is a Phase 2 sector; per
ADR-027, Phase 2 sectors have no messaging/observability wiring at all — so
there was no equivalent of AlarmManagement's `OutboxPublisherBackgroundService`/
`OutboxMetricRefreshBackgroundService` to trip over. No opt-out parameter was
needed. Confirmed by reading the file, not assumed from the ADR-027 deferral
alone.

## 3. The endpoint, and the named gap

`GET /api/v1/digital-twin/units/{id}` returns twin state (model code/name,
type, status, fidelity, whether it's the authoritative model) for the given
unit — real columns, no fabrication.

**Named gap, not filled in**: the endpoint does **not** include divergence or
sync-drift information (e.g. "does this twin currently disagree with its
measured signals"). `TwinDivergence` links to `TwinSnapshotId`/`SignalId`,
not directly to a unit. Reaching a unit from a divergence requires a
four-hop join (`TwinDivergence` → `TwinSnapshot` → `TwinRuntimeSession` →
`TwinModelVersion` → `TwinModel.UnitId`) that no existing query performs, and
`GetOpenDivergencesQuery` (fleet-wide) doesn't carry a unit reference in its
own DTO either. Building that per-unit divergence join is a real, separate
addition — recorded here as a gap for a future slice, not bundled into this
one and not faked with placeholder data.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as both prior slices — zero regressions from the new
query/finder method or the third context's composition into the BFF.

## Real host, real database — live evidence

Memory checked before starting the host (2.06 GB, confirmed stable across
two checks) — well above the ~1.7 GB threshold this session's own established
practice calls for. Rechecked again before the DB-seeding queries (1.94 GB)
and again before the endpoint call (2.08 GB) — stable throughout, no incident
this run.

`DigitalTwin.TwinModel` and its three lookup tables (`TwinModelType`,
`TwinModelStatus`, `TwinFidelityLevel`) had **zero rows** in the real
database — unlike ReactorFleet/AlarmManagement, no dev-run residue existed
for this context. Seeded minimal real data for live evidence:

```sql
INSERT INTO DigitalTwin.TwinModelType (TwinModelTypeId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc)
  VALUES (1, 'PHYSICS-BASED', 'Physics-Based Model', 1, 1, SYSUTCDATETIME());
INSERT INTO DigitalTwin.TwinModelStatus (TwinModelStatusId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc)
  VALUES (1, 'ACTIVE', 'Active', 1, 1, SYSUTCDATETIME());
INSERT INTO DigitalTwin.TwinFidelityLevel (TwinFidelityLevelId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc)
  VALUES (1, 'HIGH', 'High Fidelity', 1, 1, SYSUTCDATETIME());
INSERT INTO DigitalTwin.TwinModel (TwinModelId, UnitId, TwinModelTypeId, TwinModelStatusId, TwinFidelityLevelId, Code, Name, IsAuthoritative, IsDeleted, CreatedAtUtc)
  VALUES (1, 1, 1, 1, 1, 'TWIN-UNIT-1', 'Demonstrator Twin for Unit 1', 1, 0, SYSUTCDATETIME());
```

(`UnitId 1` = `UNIT-1`, the same demonstrator ReactorFleet unit seeded in the
first slice.)

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/digital-twin/units/1`

```json
[{"unitId":1,"unitCode":"UNIT-1","twinCode":"TWIN-UNIT-1","twinName":"Demonstrator Twin for Unit 1","modelType":"PHYSICS-BASED","status":"ACTIVE","fidelity":"HIGH","isAuthoritative":true}]
```

HTTP 200.

### `GET /api/v1/digital-twin/units/999` (unit with no twin)

```json
[]
```

HTTP 200 — empty list, not an error; a unit legitimately having no twin
modeled is not a fault condition.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Three sessions — one per composed `DbContext` (ReactorFleet, AlarmManagement,
DigitalTwin) — all under the scoped login, no fallback.

Host stopped cleanly after evidence capture; `sys.databases` confirmed all
`ONLINE` afterward.

## Summary

Three vertical slices now exist in `Nexus1.Bff`:

- **ReactorFleet** — read-only.
- **AlarmManagement** — read + write, messaging question settled (no side
  effect).
- **DigitalTwin** — read-only, no hosted-service surprise this time (Phase 2
  sector, no messaging at all per ADR-027), one real gap named (no
  divergence/sync-drift data reachable per-unit today).

Pattern holds across three different contexts with three different shapes
of "what already exists": zero queries (ReactorFleet), a wrongly-scoped
query plus an unconditional hosted-service trap (AlarmManagement), and a
wrongly-scoped query with no trap (DigitalTwin).
