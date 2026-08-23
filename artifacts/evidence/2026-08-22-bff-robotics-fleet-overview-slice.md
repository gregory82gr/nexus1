# Evidence: BFF sixth vertical slice — Robotics, Fleet Overview / Mission Readiness (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a sixth vertical slice, composed alongside all
five existing contexts in the same host:

- `GET /api/v1/robotics/units/{id}` — per-unit robot status/health and
  mission summaries for the "Fleet Overview" / "Mission Readiness" screens.

No new ADR — recorded inline, same as the last three slices.

## 1. What Robotics' Application layer already exposed

A rich existing surface, similar in shape to RadiationMonitoring: `GetAvailableRobotsByUnitQuery`
(despite its name, fleet-wide — every currently-available robot across all
units, each tagged with its unit code; "ByUnit" means "joined to its unit,"
not "for one specific unit"), `GetLatestHealthSnapshotPerRobotQuery`
(fleet-wide, no unit reference at all in the DTO), `GetMissionTimelineQuery`
and `GetBlockingReadinessFailuresQuery` (both scoped by a specific
`MissionId`, not a unit), plus two commands.

None scoped to a unit the way this endpoint needs. Checked the domain model
before assuming Robotics' fleet data isn't unit-scoped at all (the task
explicitly floated this possibility) — it **is**: `Robot.HomeUnitId` and
`Mission.UnitId` are both direct FKs to `ReactorFleet.Unit` (`HomeUnitId`
nullable — not every robot has a fixed home; `Mission.UnitId` NOT NULL).
Same pattern as the last three slices: added minimal sibling
methods/finders rather than reusing the fleet-wide ones as-is:

- `ILatestHealthSnapshotFinder.GetRobotStatusForUnitAsync(int unitId, ...)`
  — new, alongside the existing `GetLatestHealthSnapshotsAsync()`. Broader
  than the existing `RobotHealthSnapshotDto` (adds robot code/name/status),
  and — like RadiationMonitoring's per-unit finder — does **not** exclude a
  robot with zero health snapshots yet.
- `IUnitMissionsFinder` (new interface) / `EfUnitMissionsFinder` — no
  existing query lists missions at all; the two existing mission queries are
  both scoped to an already-known `MissionId`. `GetMissionsForUnitAsync`
  fills a genuine gap, not a wrongly-scoped existing query.
- `GetUnitRoboticsOverviewQuery(int UnitId)` / `GetUnitRoboticsOverviewQueryHandler`
  — combines both finders, same shape as RadiationMonitoring's two-finder
  handler.
- New DTOs: `UnitRobotStatusDto`, `UnitMissionDto`, `UnitRoboticsOverviewDto`.

**Translation-safety**: the per-unit health finder resolves battery/
communication status *codes* via a small in-memory dictionary pass after
materializing the ordered correlated-subquery results, not by joining
inside the ordered subquery — the same discipline used for RadiationMonitoring,
guarding against the exact EF translation failure this file's own top
comment already records (`GroupBy+OrderByDescending+First` not translating
— this is in fact the original file where that failure was first found, per
RadiationMonitoring's own comment crediting it).

## 2. Hosted-service check

Read `Robotics.Infrastructure`'s `ServiceCollectionExtensions` directly:
zero `AddHostedService<...>()` calls — same as DigitalTwin and
RadiationMonitoring. Robotics is Phase 2 (ADR-023), no messaging/observability
wiring (ADR-027). Confirmed by reading the file, not assumed from the
precedent holding twice already.

## 3. The endpoint, and the named boundary

`GET /api/v1/robotics/units/{id}` returns, per unit: every robot home-based
there (code, name, status, latest health if any) and every mission
requested for it (code, title, type/status/priority, timing) — all real
columns.

**Named boundary, not a gap in the same sense as prior slices**: the
endpoint deliberately stays at mission-*summary* level. It does **not**
include per-mission readiness-item detail (which checks are blocking, which
failed) or the mission event timeline — both exist
(`GetBlockingReadinessFailuresQuery`/`GetMissionTimelineQuery`), but both
are scoped to one already-selected mission, which is a mission-*detail*
drill-down screen's job, not a unit-level fleet overview's. Unlike
DigitalTwin's divergence data or RadiationMonitoring's dose data, this isn't
"the domain model can't do it" — it's "that data belongs to a different,
already-existing screen," named explicitly rather than silently pulled in
or silently left unmentioned.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all five prior slices — zero regressions.

## Real host, real database — live evidence

Memory checked before starting the host (2.47 GB, confirmed stable across
two checks). Rechecked after host start (2.41 GB) and before the endpoint
call (2.41 GB) — stable throughout, no incident this run.

`Robotics.Robot`, `Robotics.Mission`, `Robotics.RobotHealthSnapshot` all had
**zero rows** — same as DigitalTwin/RadiationMonitoring, no dev-run residue
for this context. Seeded minimal real data:

```sql
-- Lookups
INSERT INTO Robotics.RobotType (...) VALUES (1, 'INSPECTION', 'Inspection Robot', ...);
INSERT INTO Robotics.RobotStatus (...) VALUES (1, 'AVAILABLE', 'Available', ...);
INSERT INTO Robotics.BatteryStatus (...) VALUES (1, 'NORMAL', 'Normal', ...);
INSERT INTO Robotics.CommunicationStatus (...) VALUES (1, 'CONNECTED', 'Connected', ...);
INSERT INTO Robotics.MissionType (...) VALUES (1, 'INSPECTION', 'Inspection Mission', ...);
INSERT INTO Robotics.MissionStatus (...) VALUES (1, 'IN_PROGRESS', 'In Progress', ...);
INSERT INTO Robotics.MissionPriority (...) VALUES (1, 'NORMAL', 'Normal', ...);

-- Two robots for UNIT-1: one with health history, one without
INSERT INTO Robotics.RobotModel (RobotModelId, RobotTypeId, Code, Manufacturer, ModelName)
  VALUES (1, 1, 'MODEL-X1', 'Acme Robotics', 'X1 Inspector');
INSERT INTO Robotics.Robot (RobotId, RobotModelId, RobotStatusId, HomeUnitId, Code, Name)
  VALUES (1, 1, 1, 1, 'ROBOT-1', 'Demonstrator Robot 1 for Unit 1');
INSERT INTO Robotics.Robot (RobotId, RobotModelId, RobotStatusId, HomeUnitId, Code, Name)
  VALUES (2, 1, 1, 1, 'ROBOT-2', 'Demonstrator Robot 2 for Unit 1 (no health yet)');
INSERT INTO Robotics.RobotHealthSnapshot (RobotHealthSnapshotId, RobotId, BatteryStatusId, CommunicationStatusId, SnapshotAtUtc, BatteryPercent, FaultCount)
  VALUES (1, 1, 1, 1, '2026-08-22T09:00:00', 85.5, 0);
INSERT INTO Robotics.RobotHealthSnapshot (RobotHealthSnapshotId, RobotId, BatteryStatusId, CommunicationStatusId, SnapshotAtUtc, BatteryPercent, FaultCount)
  VALUES (2, 1, 1, 1, '2026-08-22T10:00:00', 82.0, 0);
INSERT INTO Robotics.Mission (MissionId, UnitId, MissionTypeId, MissionStatusId, MissionPriorityId, Code, Title, RequestedAtUtc)
  VALUES (1, 1, 1, 1, 1, 'MISSION-1', 'Inspect containment weld seams', '2026-08-22T08:00:00');
```

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/robotics/units/1`

```json
{
  "unitId": 1,
  "robots": [
    {"robotCode":"ROBOT-1","robotName":"Demonstrator Robot 1 for Unit 1","robotStatus":"AVAILABLE","latestBatteryPercent":82.00,"latestBatteryStatus":"NORMAL","latestCommunicationStatus":"CONNECTED","latestSnapshotAtUtc":"2026-08-22T10:00:00"},
    {"robotCode":"ROBOT-2","robotName":"Demonstrator Robot 2 for Unit 1 (no health yet)","robotStatus":"AVAILABLE","latestBatteryPercent":null,"latestBatteryStatus":null,"latestCommunicationStatus":null,"latestSnapshotAtUtc":null}
  ],
  "missions": [
    {"missionCode":"MISSION-1","title":"Inspect containment weld seams","missionType":"INSPECTION","missionStatus":"IN_PROGRESS","missionPriority":"NORMAL","requestedAtUtc":"2026-08-22T08:00:00","plannedStartUtc":null,"plannedEndUtc":null,"actualStartUtc":null,"actualEndUtc":null}
  ]
}
```

HTTP 200. Confirms: (a) the latest snapshot is genuinely the most recent —
`82.0%` at `10:00`, not `85.5%` at `09:00`; (b) `ROBOT-2` (no snapshots)
appears with null health fields rather than being excluded; (c) the mission
summary renders correctly with all-null timing fields for a mission that
hasn't started/planned yet.

### `GET /api/v1/robotics/units/999` (unit with no data at all)

```json
{"unitId":999,"robots":[],"missions":[]}
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                          status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x5)
```

Five sessions — one per context sharing `AlarmManagementDb` (ReactorFleet,
AlarmManagement, DigitalTwin, RadiationMonitoring, Robotics). Confirmed —
`nexus1_app`, no fallback.

Host stopped cleanly; `sys.databases` confirmed all `ONLINE` afterward.

## Summary

Six vertical slices now exist in `Nexus1.Bff`:

- **ReactorFleet** — read-only.
- **AlarmManagement** — read + write, messaging question settled.
- **DigitalTwin** — read-only, named gap (no per-unit divergence data).
- **RadiationMonitoring** — read-only, named gap (no unit-level dose
  concept — dose is a person concept).
- **Reporting** — read-only, new Application layer built from scratch,
  messaging opt-out added, named gap (case history, not sensor trends).
- **Robotics** — read-only, named boundary (mission-summary level; per-mission
  readiness/timeline detail belongs to an already-existing mission-detail
  screen, not this unit overview).

This closes out the two-slice task (Reporting + Robotics) requested
together. Both landed clean, both surfaced genuine, differently-shaped
findings rather than repeating a prior slice's gap.
