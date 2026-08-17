# ADR-025: EmergencyPreparedness (Phase 2, sector 10) — scope, domain shape, persistence, and the whole-sector FK audit

## Status

Accepted.

## Context

Phase 2's tenth sector, atlas Appendix **C.14** (confirmed via the real
`"C.14.1 Purpose and boundary"` header, not the garbled TOC — the real
atlas sequence through this point, now confirmed by direct header reads
across ten consecutive sectors: C.1 CorePlatform, C.2 Security, C.3
Organization, C.4 ReactorFleet, C.5 Instrumentation, C.6 DigitalTwin, C.7
AlarmManagement, C.8 EventManagement, C.9 Maintenance, C.10 RootCause,
C.11 ReinforcementLearning, C.12 Robotics, C.13 RadiationMonitoring,
**C.14 EmergencyPreparedness**). This sector's own numbering is another
instance of the atlas's known extraction quirks: C.14 has no separate
"C.14.5 ER diagram" heading distinct from its "core tables" DDL section —
confirmed by reading the real section list at the top of C.14 itself
(Purpose/boundary, Full table list, Lookup tables, SQL DDL, ER diagram,
Foreign-key mapping, Indexes/seeds/verification queries, Honest boundary)
rather than assuming a numbering gap meant missing content.

- `From_Schema_to_System` Appendix C.14: forty-two tables (eighteen
  lookup, twenty-four substantive — counted directly from the real table
  list and DDL, since this sector's own text does not state the total the
  way most prior sectors' C.N.2 openings do), across nine named groups:
  planning, classification, teams, scenario, exercise, routes, muster,
  actions, comms, resources, integration. The atlas's own "position in
  the backbone" callout for this sector, read directly: *"This sector
  depends on CorePlatform, Security, Organization, ReactorFleet,
  AlarmManagement, EventManagement, RadiationMonitoring and Robotics.
  Later sectors such as Compliance, Reporting and Audit will read from
  it."* C.14.8's own honest boundary: *"EmergencyPreparedness is not a
  replacement for procedure documents. It is the structured backbone
  that lets the platform join procedures, exercises, roles, routes,
  muster records, resources and actual events."*
- `From_Domain_to_Twin`'s Supporting-domain table **does** carry a
  dedicated row for this sector, unlike RadiationMonitoring/
  EventManagement — re-verified directly rather than assumed, since the
  table's own PDF-extraction layout staggers row labels against their
  text by one position (the same known garbling this project has hit
  before): *"Connects scenarios, plans, and exercises to operational
  readiness."* Good design question: *"Which procedure or exercise is
  being tested?"* Supporting, not core — same tier as Instrumentation,
  Maintenance, Robotics.
- **Whole-sector FK audit, done first and across the FULL 42-table graph
  before any scope trim.** C.14.6's foreign-key passport map and C.14.2's
  own table list were both read table-by-table; every external target
  belongs to a sector that **already exists** in this codebase:
  `Organization.Site`/`Plant`/`Team`/`Person` (confirmed directly —
  `Site.cs`/`Team.cs` exist in `Nexus1.Organization.Domain`),
  `Security.ApplicationUser`, `ReactorFleet.Unit`,
  `EventManagement.OperationalEvent`/`Incident`,
  `RadiationMonitoring.RadiationZone`, `CorePlatform.Language`/
  `EngineeringUnit`, `AlarmManagement.AlarmEvent`, `Robotics.Mission`.
  **Zero whole-sector gaps — the third consecutive Phase 2 sector with a
  clean result on this check** (after Robotics and RadiationMonitoring).
  EmergencyPreparedness is the tenth sector built and every one of its
  eight named external contexts was already built before it — including
  `RadiationMonitoring` and `Robotics`, both built earlier in this same
  session.
- No individual-table gaps found anywhere in the full graph either
  (unlike every sector through RadiationMonitoring, which each hit at
  least one absent `ReactorFleet.Equipment`/`EquipmentLocation` target) —
  this sector's own FK graph simply never names either table.
- The atlas's own four "verification queries" (C.14.7.3) name real
  Application-layer operations: (1) one site's active plans and current
  revision count, (2) exercises with observations requiring corrective
  action, (3) open/restricted evacuation routes crossing radiological
  zones, (4) resource readiness dashboard by site. Reading each query's
  own `JOIN`/`SELECT` list, following the same "nothing more" discipline
  every prior sector's ADR has used, drives the scope decision below.

## Decision

### Scope: seventeen of forty-two tables — the atlas's own four named verification queries, plus the FK-integrity chain their own NOT NULL columns require

Query-by-query:

1. *One site's active plans and current revision count* joins
   `EmergencyPlan`, `Organization.Site`, `PlanStatus`, (left join)
   `EmergencyPlanRevision`.
2. *Exercises with observations requiring corrective action* joins
   `Exercise`, `ExerciseObservation`.
3. *Open/restricted evacuation routes crossing radiological zones* joins
   `EvacuationRoute`, `RouteStatus`, `EvacuationRouteZone`,
   `RadiationMonitoring.RadiationZone`.
4. *Resource readiness dashboard by site* joins `EmergencyResource`,
   `Organization.Site`, `ResourceType`, `ResourceReadinessCheck`,
   `ReadinessStatus`.

Union of directly-named tables: lookups `PlanStatus`, `RouteStatus`,
`ResourceType`, `ReadinessStatus`; substantive `EmergencyPlan`,
`EmergencyPlanRevision`, `Exercise`, `ExerciseObservation`,
`EvacuationRoute`, `EvacuationRouteZone`, `EmergencyResource`,
`ResourceReadinessCheck` — twelve tables. Real DDL (read directly) adds
four more `NOT NULL`-chain lookups: `Exercise.ExerciseTypeId`/
`ExerciseStatusId` → `ExerciseType`/`ExerciseStatus`;
`ExerciseObservation.ObservationSeverityId` → `ObservationSeverity`;
`EmergencyResource.ResourceStatusId` → `ResourceStatus`. `EvacuationRoute.
AssemblyPointId` is `NOT NULL`, pulling in the `AssemblyPoint`
substantive table itself. Same FK-integrity-closure reasoning
Robotics'/RadiationMonitoring's/DigitalTwin's ADRs already used.

**In scope (17):** lookups `PlanStatus`, `RouteStatus`, `ResourceType`,
`ReadinessStatus`, `ExerciseType`, `ExerciseStatus`,
`ObservationSeverity`, `ResourceStatus` (8); substantive `EmergencyPlan`,
`EmergencyPlanRevision`, `Exercise`, `ExerciseObservation`,
`EvacuationRoute`, `EvacuationRouteZone`, `AssemblyPoint`,
`EmergencyResource`, `ResourceReadinessCheck` (9).

**Out of scope (25), grouped by reason, not a blanket cut:**

- **Classification/teams group** (`EmergencyClassType`,
  `EmergencyClassification`, `EmergencyRole`, `ResponseTeam`,
  `ResponseTeamMember`) — none of the four named queries touch any of
  these.
- **Scenario/exercise-detail group** (`ScenarioStatus`, `EmergencyScenario`,
  `ParticipantStatus`, `ExerciseParticipant`, `InjectType`,
  `ExerciseInject`, `EvaluationStatus`, `ExerciseEvaluation`) — not
  queried. `ExerciseObservation.ExerciseInjectId` is nullable, so
  `ExerciseInject` is not FK-integrity-forced the way `AssemblyPoint`
  was; left out entirely along with the rest of the exercise-detail
  chain.
- **`MusterStatus`/`MusterRecord`** — not queried; the muster/
  accountability layer C.14.1's own purpose statement names explicitly
  (*"where people must assemble"*) but no verification query exercises.
- **Actions group** (`ProtectiveActionType`, `ProtectiveActionStatus`,
  `ProtectiveAction`) — not queried.
- **Comms group** (`NotificationChannel`, `CommunicationStatus`,
  `NotificationTemplate`, `EmergencyCommunication`) — not queried; would
  additionally require `CorePlatform.Language`, not otherwise needed by
  this pass.
- **Integration links** (`PlanIncidentLink`, `PlanAlarmLink`,
  `PlanRobotMissionLink`) — the whole-sector audit above specifically
  cleared all three targets (`EventManagement.Incident`,
  `AlarmManagement.AlarmEvent`, `Robotics.Mission`) as real and buildable.
  Same third category Robotics'/RadiationMonitoring's own ADRs named for
  their own link tables: not blocked, not reconnected-now, *buildable
  and clean, deliberately left for a future pass because nothing in this
  sector's own verification surface asks for it yet*. Recorded
  explicitly, not silently dropped.

### Domain shape: a plan/revision spine plus the exercise-and-resource proof layer, matching the design choice's own "structured backbone, not a procedure replacement" framing

`EmergencyPlan` carries its `PlanStatusId` `NOT NULL` chain as a real
internal invariant, plus a `CurrentRevisionNumber` counter that
`EmergencyPlanRevision` rows accumulate against — the atlas's own design
statement realized directly: *"The active plan points to the current
revision number."* `Exercise` is the readiness-testing header (own
`ExerciseTypeId`/`ExerciseStatusId` chain, `NOT NULL`), with
`ExerciseObservation` as its append-style finding record (no full audit
shape in the real DDL — matches `RadiationReading`'s and
`RobotHealthSnapshot`'s own append-only pattern from prior sectors,
confirmed by reading the DDL directly rather than assumed).
`EvacuationRoute`/`EvacuationRouteZone`/`AssemblyPoint` is the physical
route-and-muster-destination layer, deliberately anchored to
`RadiationMonitoring.RadiationZone` — the atlas's own C.14.1 purpose text
names this exact linkage (*"where people must assemble"*, radiological
awareness). `EmergencyResource`/`ResourceReadinessCheck` mirrors
`RadiationMonitoring.RadiationMonitor`/`RadiationMonitorCalibration`'s own
shape from the immediately prior sector — a governed asset with a
periodic readiness/calibration check trail, the closest structural
analogue this sector has to that pattern.

### Application layer: the atlas's own four named verification queries

1. `GetSiteActivePlansQuery` — active (`IsDeleted = 0`) `EmergencyPlan`
   rows for one site, joined to `PlanStatus`, with a revision-row count
   from `EmergencyPlanRevision`.
2. `GetExercisesWithCorrectiveObservationsQuery` — `Exercise` rows with a
   count of `ExerciseObservation` rows where
   `CorrectiveActionRequired = 1`.
3. `GetOpenOrRestrictedRoutesCrossingZonesQuery` — `EvacuationRoute` rows
   joined through `EvacuationRouteZone` to `RadiationMonitoring.
   RadiationZone`, where `RouteStatus.Code IN ('OPEN','RESTRICTED')`.
4. `GetResourceReadinessDashboardQuery` — `EmergencyResource` rows by
   site and type, with each resource's latest
   `ResourceReadinessCheck.ReadinessStatusId` (correlated-subquery
   pattern, matching the "latest per parent row" fix Robotics'/
   RadiationMonitoring's own finders already established rather than a
   `GroupBy`+`Join` that doesn't translate to SQL).

Plus the two commands the sector's own core premise needs to produce
data for those four reads to have anything to report on:
`ApproveEmergencyPlanCommand` (creates an `EmergencyPlan` against
`PlanStatusId`) and `ScheduleExerciseCommand` (creates an `Exercise`
against `ExerciseTypeId`/`ExerciseStatusId`) — same "read queries need at
least one write path to be provably real" reasoning every prior sector's
Application layer used.

### Persistence: shares `AlarmManagementDb` — all three axes agree cleanly

- **Topology.** EmergencyPreparedness is plant-operational readiness
  data, physically colocated with the demonstrator plant — same category
  as `ReactorFleet`/`Instrumentation`/`DigitalTwin`/`Maintenance`/
  `EventManagement`/`Robotics`/`RadiationMonitoring`, all of which
  already share `AlarmManagementDb`.
- **Sensitivity.** Plan/exercise/resource-readiness data carries no
  personnel-HR sensitivity (unlike `Organization`) and no access-control
  sensitivity (unlike `Security`) — ordinary operational/safety data, the
  same tier as its plant-operational siblings and the same reasoning
  RadiationMonitoring's own ADR-024 used for its comparably sensitive
  personal-dose data.
- **FK-locality.** Within the seventeen-table scope, the real
  cross-context FKs are `EmergencyResource.EngineeringUnitId` →
  `CorePlatform.EngineeringUnit` and `AssemblyPoint.RadiationZoneId`/
  `EvacuationRouteZone.RadiationZoneId` → `RadiationMonitoring.
  RadiationZone` — both already live in `AlarmManagementDb`. The second
  is a genuinely new extension of the shadow-entity technique: every
  prior real cross-context FK in this codebase has targeted a V1 context
  (`ReactorFleet`, `AlarmManagement`) or an early Phase 2 context
  (`CorePlatform`); this is the first FK targeting a table from a sector
  built *within this same Phase 2 sequence* (`RadiationMonitoring`,
  sector 9). The technique itself is unchanged — a fresh local
  `RadiationMonitoringRadiationZoneReference` shadow entity, same
  `ExcludeFromMigrations` mechanism — but worth naming explicitly as the
  first instance of this specific shape. No `ReactorFleetUnitReference`
  is needed this sector: none of the seventeen in-scope tables carries a
  `UnitId` column, a genuine first among the plant-operational sectors.

Own migration history (`__EFMigrationsHistory_EmergencyPreparedness`),
own schema (`EmergencyPreparedness`), same physical database.
`Organization.Site`/`Plant`/`Team` and `Security.ApplicationUser`
references stay passport-only, no enforced constraint, even where
`NOT NULL` (`EmergencyPlan.SiteId`, `Exercise.SiteId`,
`EmergencyResource.SiteId`, `AssemblyPoint.SiteId`, and every
`*UserId` column) — `OrganizationDb`/`SecurityDb` are separate physical
databases, the same downgrade every prior sector's Organization/Security
references has needed. A `NOT NULL` passport column without an enforced
FK is not a new shape in this codebase (`RadiationMonitoring.
PersonDosimeterAssignment.PersonId` set the precedent in ADR-024) — it
recurs here across five columns rather than one, worth naming but not a
new pattern.

### `.sln` nesting discipline (verified before adding anything)

Confirmed via `grep -n 'Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") =
"Contexts", "Contexts"' Nexus1.Runtime.sln` — exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, before any EmergencyPreparedness
project was added. A new `EmergencyPreparedness` solution folder is
created and nested under that GUID in `GlobalSection(NestedProjects)`,
matching RadiationMonitoring's/Robotics' own entry shape
(`{new-folder-guid} = {981F0668-...}` for Domain/Application/
Infrastructure; `UnitTests`/`ComponentTests` nest under the shared
`tests` folder GUID `{DFD64979-71D4-46B5-BF62-217FA110CF39}`, confirmed
against the real `.sln` content directly). Re-verified with the same
`grep` after the edit — still exactly one match.

## Consequences

- EmergencyPreparedness becomes the tenth sector sharing
  `AlarmManagementDb`'s physical database.
- `PlanIncidentLink`, `PlanAlarmLink`, and `PlanRobotMissionLink` are
  explicitly recorded as clean, buildable, not-yet-reconnected — all
  three targets exist today, unlike a genuine whole-sector gap.
- `RadiationMonitoringRadiationZoneReference` is the first shadow entity
  targeting a table built in this same Phase 2 sequence rather than a V1
  or early-Phase-2 context — the technique generalizes cleanly, recorded
  as a milestone rather than a new decision.
- This is the third consecutive Phase 2 sector with a clean (zero-gap)
  whole-sector FK audit result, and the first with zero individual-table
  gaps in its full FK graph as well.

## Rejected alternatives

- **Own database for EmergencyPreparedness.** Rejected — no sensitivity
  or topology argument distinguishes it from its plant-operational
  siblings, and both of its real cross-context FKs already live in
  `AlarmManagementDb`; a separate database would force both to
  passport-only for no benefit.
- **Include the three integration links now, since the whole-sector audit
  proved them buildable.** Rejected for *this* pass, same reasoning as
  Robotics' and RadiationMonitoring's own equivalents — "buildable" and
  "verification-query-justified" are different bars, and the atlas's own
  four named queries do not touch any of the three link tables.

## Evidence required

- `dotnet build` warning-clean.
- `dotnet test` green, including `Nexus1.ArchitectureTests`.
- Migration applied to the real `AlarmManagementDb`;
  `EmergencyPreparedness.*` tables and the
  `FK_EmergencyResource_EngineeringUnit`/`FK_AssemblyPoint_RadiationZone`/
  `FK_EvacuationRouteZone_RadiationZone` constraints confirmed via
  `sys.foreign_keys` against `CorePlatform.EngineeringUnit`/
  `RadiationMonitoring.RadiationZone`.
- Real host startup; `GET /health/ready` returns `200 Healthy` with an
  `emergencypreparedness-db` check present.
- Evidence report written only after all of the above are independently
  confirmed — build, test, real host, health check, evidence report,
  commit, in that order.
