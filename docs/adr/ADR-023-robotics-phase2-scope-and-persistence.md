# ADR-023: Robotics (Phase 2, sector 8) — scope, domain shape, persistence, and the whole-sector FK audit

## Status

Accepted.

## Context

Phase 2's eighth sector, atlas Appendix **C.12** (confirmed via the real
`"C.12.1 Sector purpose"` header, not the garbled TOC — the real atlas
sequence through this point, now confirmed by direct header reads across
eight consecutive sectors: C.1 CorePlatform, C.2 Security, C.3
Organization, C.4 ReactorFleet, C.5 Instrumentation, C.6 DigitalTwin, C.7
AlarmManagement, C.8 EventManagement, C.9 Maintenance, C.10 RootCause,
C.11 ReinforcementLearning, **C.12 Robotics**, C.13 RadiationMonitoring).

- `From_Schema_to_System` Appendix C.12: **thirty-eight tables** (fifteen
  lookup, twenty-three substantive), across six groups the sector-purpose
  text itself names: fleet, telemetry, docking, mission, readiness,
  integration. C.12.1's own design choice: *"Robot telemetry is not mixed
  with plant instrumentation history. Robot-mounted sensors are bound to
  Instrumentation.Signal, but robot health metrics remain in
  Robotics.RobotTelemetry."* C.12.8's own honest boundary: *"This schema
  models robotic assets for the NEXUS-1 demonstrator. It is not a
  certified robotics control system, not a navigation safety case and not
  an autonomy stack."*
- `From_Domain_to_Twin`'s Supporting-domain table lists **Robotics**
  directly (re-verified by direct quote, not memory): *"Represents
  missions and inspections that may produce evidence."* Good design
  question: *"Is the robot output evidence or action?"* Supporting, not
  core — same tier as Instrumentation, Maintenance, Emergency
  Preparedness. No conflict with the atlas; both sources agree Robotics
  supports the twin/root-cause core rather than defining it.
- **Whole-sector FK audit, done first and across the FULL 38-table graph
  before any scope trim** — this is the specific check the user asked for
  by name, referencing EventManagement's AlarmManagement finding. C.12.7's
  foreign-key mapping was read table-by-table; every external target
  belongs to a sector that **already exists** in this codebase:
  `ReactorFleet.Unit` (Robot, RobotWorkEnvelope, RobotLocationHistory,
  DockingStation, Mission), `CorePlatform.EngineeringUnit`
  (RobotCapability, RobotTelemetry), `Security.ApplicationUser` (nine
  columns across RobotCapabilityAssignment/ChargingSession/Mission/
  MissionRobotAssignment/MissionRoute/MissionEvent/
  MissionReadinessAssessment/RobotMaintenanceLink/
  MissionOperationalEventLink), `Instrumentation.Signal`
  (RobotSensorBinding), `Maintenance.WorkOrder` (RobotMaintenanceLink),
  `EventManagement.OperationalEvent` (MissionOperationalEventLink). **This
  is the first Phase 2 sector whose complete FK graph has zero
  whole-sector gaps** — contrast EventManagement's AlarmManagement finding
  (existed, but had to be checked) and Maintenance's original
  WorkOrderEventLink finding (EventManagement did not exist yet at the
  time). Robotics is the eighth sector built and every one of its six
  named external contexts was already built before it.
- **Individual-table gaps** (the ReactorFleet.Equipment/PlantSystem
  pattern, not a whole-sector gap) confirmed by reading
  `Nexus1.ReactorFleet.Domain`/`Nexus1.ReactorFleet.Infrastructure`
  directly: only `Unit` and `UnitPowerSnapshot` exist. Neither
  `ReactorFleet.Equipment` nor `ReactorFleet.EquipmentLocation` exists in
  this codebase. This affects `MissionTask.TargetEquipmentId` (→
  `Equipment`) and four `EquipmentLocationId` columns on
  `RobotWorkEnvelope`/`RobotLocationHistory`/`DockingStation`/
  `MissionRouteWaypoint` (→ `EquipmentLocation`) — all four of those
  tables land outside this pass's scope regardless (see below), so the
  gap is recorded but does not force any in-scope downgrade this time.
- The atlas's own four "useful verification queries" (C.12.5.2) name real
  Application-layer operations: (1) robots currently available by unit,
  (2) latest health snapshot per robot, (3) mission timeline for one
  mission, (4) readiness failures that block dispatch. Reading each
  query's own `JOIN`/`SELECT` list, following the same discipline
  EventManagement's ADR used ("nothing more"), drives the scope decision
  below.

## Decision

### Scope: fifteen of thirty-eight tables — the atlas's own four named verification queries, plus the two lookups their own NOT NULL chain requires, nothing more

Query-by-query:

1. *Robots currently available by unit* joins `Robot`, `RobotStatus`, and
   (left join) `ReactorFleet.Unit`.
2. *Latest health snapshot for each robot* joins `Robot`,
   `RobotHealthSnapshot`, `BatteryStatus`, `CommunicationStatus`.
3. *Mission timeline for one mission* joins `Mission`, `MissionEvent`,
   `Robot`.
4. *Readiness failures that block dispatch* joins `Mission`,
   `MissionReadinessAssessment`, `MissionReadinessItem`,
   `ReadinessStatus`.

Union: `Robot`, `RobotStatus`, `RobotHealthSnapshot`, `BatteryStatus`,
`CommunicationStatus`, `Mission`, `MissionEvent`,
`MissionReadinessAssessment`, `MissionReadinessItem`, `ReadinessStatus` —
ten tables. `Robot.RobotModelId` is `NOT NULL` in the real DDL, so
`RobotModel` must exist for `Robot` to have a single valid row;
`RobotModel.RobotTypeId` is likewise `NOT NULL`, pulling in `RobotType`.
`Mission.MissionTypeId`/`MissionStatusId`/`MissionPriorityId` are all
`NOT NULL`, pulling in `MissionType`, `MissionStatus`, `MissionPriority`.
Same FK-integrity-closure reasoning DigitalTwin and Maintenance's ADRs
already used for their own "one small addition."

**In scope (15):** lookups `RobotType`, `RobotStatus`, `BatteryStatus`,
`CommunicationStatus`, `MissionType`, `MissionStatus`, `MissionPriority`,
`ReadinessStatus`; substantive `RobotModel`, `Robot`,
`RobotHealthSnapshot`, `Mission`, `MissionEvent`,
`MissionReadinessAssessment`, `MissionReadinessItem`.

**Out of scope (23), grouped by reason, not a blanket cut:**

- **Capability/payload/sensor/communication** (`RobotCapabilityType`,
  `RobotCapability`, `RobotCapabilityAssignment`, `PayloadType`,
  `RobotPayload`, `RobotSensorBinding`, `RobotCommunicationLink`) — none
  of the four named queries touch any of these. Including them would also
  pull in `CorePlatform.EngineeringUnit` and `Instrumentation.Signal` as
  live cross-context surface for no query-proven benefit. This is the
  group C.12.1's own design-choice callout (Signal binding) belongs to;
  the callout is a narrative boundary statement, not something a table
  must exist to honor — recorded as reconnection-ready, not blocked.
- **`RobotWorkEnvelope`** — not queried; would additionally require the
  confirmed-absent `ReactorFleet.EquipmentLocation`.
- **Docking group** (`DockingStationStatus`, `DockingStation`,
  `ChargingSession`) — not queried. Consequence: `Robot.
  HomeDockingStationId` is omitted from the Domain model entirely (not
  even as a passport column) — its target table doesn't exist in this
  pass at all, unlike a passport reference to a table that exists in a
  different physical database. Reconnection note below.
- **`RobotLocationHistory`** — not queried (query 2 asks for the health
  snapshot, not the location trace); would additionally require the
  confirmed-absent `ReactorFleet.EquipmentLocation`.
- **Mission planning/execution detail** (`RouteType`, `WaypointType`,
  `MissionRobotAssignment`, `MissionTask`, `MissionRoute`,
  `MissionRouteWaypoint`, `MissionPayloadUsage`) — not queried.
  `MissionTask.TargetEquipmentId` would additionally require the
  confirmed-absent `ReactorFleet.Equipment` — consistent with every prior
  sector's identical finding on that table.
- **Readiness authoring** (`MissionChecklist`, `MissionChecklistItem`) —
  not queried; only the assessment/outcome side
  (`MissionReadinessAssessment`/`MissionReadinessItem`) is. Consequence:
  `MissionReadinessItem.MissionChecklistItemId` is omitted from the
  Domain model entirely, same reasoning as `HomeDockingStationId`.
- **Integration links** (`RobotMaintenanceLink`,
  `MissionOperationalEventLink`) — this is the pair the whole-sector audit
  above specifically cleared: both targets (`Maintenance.WorkOrder`,
  `EventManagement.OperationalEvent`) exist and are real, buildable FKs
  today, unlike Maintenance's original `WorkOrderEventLink` finding. But
  neither is touched by any of the four named verification queries, so —
  same "nothing more" discipline EventManagement's own ADR applied to
  itself — they stay out of *this* pass. This is a third, distinct
  category from both prior reversal notes: not blocked (Maintenance's
  `WorkOrderEventLink` at the time), not reconnected-now (EventManagement's
  `WorkOrder.OriginOperationalEventId`/`OriginIncidentActionId`), but
  *buildable and clean, deliberately left for a future pass because
  nothing in this sector's own verification surface asks for it yet*.
  Recorded explicitly so a future session does not have to re-derive this.

### Domain shape: a fleet/status spine plus the dispatch-and-readiness gate, matching the Supporting-domain "evidence or action" question

`Robot` carries its own lifecycle (`RobotStatusId`) and model reference
(`RobotModelId` → `RobotModel` → `RobotTypeId` → `RobotType`) as real
internal invariants — a `Robot` cannot exist without a valid model, a
`RobotModel` cannot exist without a valid type, matching the atlas's own
`NOT NULL` chain. `RobotHealthSnapshot` is an append-only fact table (no
`IsDeleted`/audit columns in its DDL — matches `RobotTelemetry`'s and
`RobotLocationHistory`'s own append-only shape from C.12.4.4, confirmed by
reading the DDL directly rather than assumed). `Mission` is the dispatch
header with its own three-way lookup chain (`MissionTypeId`/
`MissionStatusId`/`MissionPriorityId`, all `NOT NULL`); `MissionEvent` is
its append-only timeline (mirrors `EventManagement.EventTimelineEntry`'s
shape — a mission is itself a small event-sourced record, the same
pattern EventManagement's ADR already established for
`OperationalEvent`). `MissionReadinessAssessment`/`MissionReadinessItem`
are the dispatch gate the sector purpose names explicitly — the pass/
fail/conditional verdict a mission must clear, structurally the closest
analogue in this codebase to a governance gate, matching
`From_Domain_to_Twin`'s own "is the robot output evidence or action"
framing: a `MissionReadinessAssessment` records evidence about whether a
mission *may* act, it does not itself act.

### Application layer: the atlas's own four named verification queries

1. `GetAvailableRobotsByUnitQuery` — robots with `RobotStatus.Code =
   'AVAILABLE'`, joined to `ReactorFleet.Unit` for the unit code.
2. `GetLatestHealthSnapshotPerRobotQuery` — one row per robot, most recent
   `RobotHealthSnapshot`, with battery/communication status codes.
3. `GetMissionTimelineQuery` — `MissionEvent` rows for one mission,
   ordered by `OccurredAtUtc`, with the acting robot's code.
4. `GetBlockingReadinessFailuresQuery` — `MissionReadinessItem` rows where
   `IsBlocking = 1` and status is `BLOCKED`/`EXPIRED`.

Plus the two commands the sector's own core premise needs to produce data
for those four reads to have anything to report on: `RegisterRobotCommand`
(creates a `Robot` against a `RobotModel`/`RobotStatus`) and
`DispatchMissionCommand` (creates a `Mission` against
`MissionType`/`MissionStatus`/`MissionPriority`) — same "read queries need
at least one write path to be provably real" reasoning every prior
sector's Application layer used.

### Persistence: shares `AlarmManagementDb` — all three axes agree cleanly

- **Topology.** Robotics is plant-operational fleet/mission data,
  physically colocated with the demonstrator plant — same category as
  `ReactorFleet`/`Instrumentation`/`DigitalTwin`/`Maintenance`/
  `EventManagement`, all of which already share `AlarmManagementDb`.
- **Sensitivity.** Fleet and mission-dispatch data carries no personnel/HR
  sensitivity (unlike `Organization`) and no access-control sensitivity
  (unlike `Security`) — ordinary operational data, the same tier as its
  plant-operational siblings.
- **FK-locality.** Within the fifteen-table scope, the only real
  cross-context FK is `Robot.HomeUnitId`/`Mission.UnitId` →
  `ReactorFleet.Unit`, which already lives in `AlarmManagementDb`. Sharing
  makes that a genuine same-database SQL `FOREIGN KEY` (via the
  `ReactorFleetUnitReference` shadow-entity technique, ADR-019's pattern,
  a fresh local copy per this codebase's own "shadow entities stay local
  to their project" convention) rather than a passport-only cross-database
  reference.

Own migration history (`__EFMigrationsHistory_Robotics`), own schema
(`Robotics`), same physical database. `Security.ApplicationUser`
references on `Mission` (`RequestedByUserId`, `ApprovedByUserId`) and
`MissionEvent`/`MissionReadinessAssessment` (`RecordedByUserId`,
`AssessedByUserId`) stay passport-only, no enforced constraint — `SecurityDb`
is a separate physical database, the same downgrade every prior sector's
Security references has needed (Instrumentation, DigitalTwin, Maintenance,
EventManagement).

### `.sln` nesting discipline (verified before adding anything)

Confirmed via `grep -n 'Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") =
"Contexts", "Contexts"' Nexus1.Runtime.sln` — exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, before any Robotics project was
added. A new `Robotics` solution folder is created and nested under that
GUID in `GlobalSection(NestedProjects)`, exactly matching
EventManagement's own entry shape (`{new-robotics-folder-guid} =
{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, then each of Robotics.Domain/
Application/Infrastructure/UnitTests/ComponentTests nested under the new
Robotics folder guid). Re-verified with the same `grep` after the edit,
per the standing discipline (still exactly one match).

## Consequences

- Robotics becomes the eighth sector sharing `AlarmManagementDb`'s
  physical database, alongside `ReactorFleet`/`CorePlatform`/
  `AlarmManagement`/`Instrumentation`/`DigitalTwin`/`Maintenance`/
  `EventManagement`.
- `RobotMaintenanceLink` and `MissionOperationalEventLink` are explicitly
  recorded as clean, buildable, not-yet-reconnected — a future
  RadiationMonitoring-and-beyond session (or a dedicated pass) can add
  them without any structural blocker, unlike a genuine whole-sector gap.
- `Robot.HomeDockingStationId` and `MissionReadinessItem.
  MissionChecklistItemId` are omitted from the Domain model entirely, not
  merely downgraded — their target tables do not exist in this pass at
  all. Distinct from a passport column (target exists, different
  database) — recorded so a future docking/checklist-authoring pass knows
  to add both the column and its owning table together.
- This is the first Phase 2 sector with a clean (zero-gap) whole-sector FK
  audit result — worth noting as a genuine milestone in the FK-locality
  discipline this project has built up sector by sector.

## Rejected alternatives

- **Own database for Robotics.** Rejected — no sensitivity or topology
  argument distinguishes it from its plant-operational siblings, and the
  one real cross-context FK it has (`ReactorFleet.Unit`) already lives in
  `AlarmManagementDb`; a separate database would force that FK to
  passport-only for no benefit.
- **Include `RobotMaintenanceLink`/`MissionOperationalEventLink` now,
  since the whole-sector audit proved them buildable.** Considered
  directly, given the user's specific focus on FK buildability this
  sector. Rejected for *this* pass — "buildable" and "verification-query-
  justified" are different bars, and the atlas's own four named queries
  (the bar every prior sector's scope decision has used) do not touch
  either link table. Recorded explicitly above rather than silently
  dropped, so the distinction from a genuine gap is not lost.

## Evidence required

- `dotnet build` warning-clean.
- `dotnet test` green, including `Nexus1.ArchitectureTests`.
- Migration applied to the real `AlarmManagementDb`; `Robotics.*` tables
  and the `FK_Robotics_Robot_Unit`/`FK_Robotics_Mission_Unit` constraints
  confirmed via `sys.foreign_keys` against `ReactorFleet.Unit`.
- Real host startup; `GET /health/ready` returns `200 Healthy` with a
  `robotics-db` check present.
- Evidence report written only after all of the above are independently
  confirmed — build, test, real host, health check, evidence report,
  commit, in that order.
