# ADR-024: RadiationMonitoring (Phase 2, sector 9) — scope, domain shape, persistence, and the whole-sector FK audit

## Status

Accepted.

## Context

Phase 2's ninth sector, atlas Appendix **C.13** (confirmed via the real
`"C.13.1 Sector purpose"` header, not the garbled TOC — the real atlas
sequence through this point, now confirmed by direct header reads across
nine consecutive sectors: C.1 CorePlatform, C.2 Security, C.3
Organization, C.4 ReactorFleet, C.5 Instrumentation, C.6 DigitalTwin, C.7
AlarmManagement, C.8 EventManagement, C.9 Maintenance, C.10 RootCause,
C.11 ReinforcementLearning, C.12 Robotics, **C.13 RadiationMonitoring**,
C.14 EmergencyPreparedness).

- `From_Schema_to_System` Appendix C.13: **forty-six tables** (seventeen
  lookup, twenty-nine substantive), across eight named groups: zones,
  monitors, dosimetry, surveys, environmental sampling, snapshots,
  permits, integration. C.13.1's own design choice: *"Engineering units
  are still owned by CorePlatform.EngineeringUnit. The
  RadiationMonitoring schema owns the radiological meaning of the
  record, not the unit catalogue itself."* C.13.8's own honest boundary:
  *"This schema is a demonstrator-grade radiation-monitoring data model
  for NEXUS-1. It is not a licensed radiological protection system, not
  a dosimetry legal record, and not a safety-class monitoring
  platform."*
- `From_Domain_to_Twin`'s Supporting-domain section mentions
  RadiationMonitoring only in the intro prose (*"instrumentation, alarm
  management, maintenance, robotics, radiation monitoring, and emergency
  preparedness can be read this way"*) — it does **not** get its own
  table row with a "good design question," the same situation
  EventManagement's ADR-022 found for itself (*"zero mentions... absent
  from the classification tables"*). No dedicated classification to lean
  on; scope and domain-shape decisions below are derived directly from
  the atlas's own signals, per that established precedent.
- **Whole-sector FK audit, done first and across the FULL 46-table graph
  before any scope trim.** C.13.7's foreign-key mapping and C.13.2's own
  table list were both read table-by-table; every external target
  belongs to a sector that **already exists** in this codebase:
  `ReactorFleet.Unit`/`ReactorFleet.EquipmentLocation`/
  `ReactorFleet.Equipment` (zones, monitors, isotope inventory),
  `CorePlatform.EngineeringUnit` (used pervasively — access requirements,
  readings, dose limits, dose readings, survey readings, sample results,
  snapshots, permit dose records, trend summaries),
  `CorePlatform.Country`/`CorePlatform.Region` (environmental sampling
  locations), `Instrumentation.Signal` (monitor signal binding),
  `Security.ApplicationUser` (calibration, dosimeter assignment, dose
  alert acknowledgement, survey review, permit accountability),
  `Organization.Person`/`Organization.Plant` (dosimeter assignment, dose
  limit assignment, survey assignment, sample collection, permit
  participants, environmental sampling location), `EventManagement.
  OperationalEvent` (radiation-event link), `Robotics.Mission`
  (radiation-robot-mission link — the first sector to reference Robotics
  since it was built). **Zero whole-sector gaps** — the second
  consecutive Phase 2 sector with a clean result on this check (after
  Robotics). RadiationMonitoring is the ninth sector built and every one
  of its eight named external contexts was already built before it.
- **Individual-table gaps** (the now-familiar `ReactorFleet.Equipment`/
  `EquipmentLocation` pattern, not a whole-sector gap), re-confirmed
  directly rather than assumed from the earlier Robotics finding: only
  `Unit`/`UnitPowerSnapshot` exist in `Nexus1.ReactorFleet.Domain`.
  Affects `RadiationZone.EquipmentLocationId`, `RadiationMonitor.
  EquipmentId`, and `IsotopeInventory.EquipmentId`/`UnitId`'s equipment
  half — the first two land inside this pass's chosen scope (see below,
  downgraded to passport-only), the third does not (`IsotopeInventory`
  is out of scope).
- The atlas's own four "useful verification queries" (C.13.5.2) name
  real Application-layer operations: (1) active radiation zones with
  unit and classification, (2) monitors whose calibration is due, (3)
  latest reading per monitor, (4) open dose alerts with the person
  assignment that produced them. Reading each query's own `JOIN`/
  `SELECT` list, following the same "nothing more" discipline
  EventManagement's and Robotics' ADRs used, drives the scope decision
  below.

## Decision

### Scope: twenty of forty-six tables — the atlas's own four named verification queries, plus the FK-integrity chain their own NOT NULL columns require

Query-by-query:

1. *Active radiation zones with their unit and classification* joins
   `RadiationZone`, `RadiationAreaClassification`, `RadiationZoneStatus`,
   (left join) `ReactorFleet.Unit`.
2. *Monitors whose calibration is due* joins `RadiationMonitor`,
   `MonitorType`.
3. *Latest reading per monitor* joins `RadiationMonitor`,
   `RadiationReading`, `CorePlatform.EngineeringUnit`,
   `MeasurementQuality`.
4. *Open dose alerts and the person assignment that produced them* joins
   `DoseAlert`, `DoseLimit`, `PersonDoseReading`,
   `PersonDosimeterAssignment`, `Organization.Person`,
   `CorePlatform.EngineeringUnit`, `AlertStatus`.

Union of directly-named tables: lookups `RadiationAreaClassification`,
`RadiationZoneStatus`, `MonitorType`, `MeasurementQuality`,
`AlertStatus`; substantive `RadiationZone`, `RadiationMonitor`,
`RadiationReading`, `DoseAlert`, `DoseLimit`, `PersonDoseReading`,
`PersonDosimeterAssignment` — twelve tables. Real DDL (read directly, not
inferred from the table-list summary) adds five more `NOT NULL`-chain
lookups the above substantive tables cannot have a single valid row
without: `RadiationZone.RadiationZoneTypeId` → `RadiationZoneType`;
`RadiationMonitor.MonitorStatusId` → `MonitorStatus`; `RadiationReading.
MeasurementTypeId` → `MeasurementType`; `DoseLimit.DoseTypeId`/
`PersonDoseReading.DoseTypeId` → `DoseType`; `DoseLimit.LimitTypeId` →
`LimitType`. `PersonDosimeterAssignment.DosimeterId` is `NOT NULL`,
pulling in the `Dosimeter` substantive table itself, whose own
`DosimeterTypeId`/`DosimeterStatusId` are in turn `NOT NULL`, pulling in
`DosimeterType`/`DosimeterStatus`. Same FK-integrity-closure reasoning
Robotics/DigitalTwin/Maintenance's ADRs already used.

**In scope (20):** lookups `RadiationZoneType`, `RadiationZoneStatus`,
`RadiationAreaClassification`, `MonitorType`, `MonitorStatus`,
`MeasurementType`, `MeasurementQuality`, `DoseType`, `DosimeterType`,
`DosimeterStatus`, `LimitType`, `AlertStatus` (12); substantive
`RadiationZone`, `RadiationMonitor`, `RadiationReading`, `Dosimeter`,
`PersonDosimeterAssignment`, `PersonDoseReading`, `DoseLimit`,
`DoseAlert` (8).

**Out of scope (26), grouped by reason, not a blanket cut:**

- **`RadiationZoneBoundaryPoint`, `RadiationZoneAccessRequirement`** —
  not queried; geometry/access-rule detail beyond what any named query
  reads.
- **`RadiationMonitorSignalBinding`, `RadiationMonitorCalibration`** —
  not queried. Query 2 asks only for the calibration *due date* already
  stored directly on `RadiationMonitor.CalibrationDueAtUtc`, not the
  calibration history table.
- **Dosimetry limits/exposure detail** (`DoseLimitAssignment`,
  `ExposureCategory`) — not queried; query 4 reads `DoseLimit` and
  `PersonDoseReading` directly, not how a limit is assigned to a person
  or exposure class.
- **Survey group** (`SurveyType`, `SurveyStatus`, `ContaminationType`,
  `SampleType`, `RadiologicalSurvey`, `RadiologicalSurveyReading`,
  `ContaminationSample`, `ContaminationSampleResult`) — none of the four
  named queries touch any survey table.
- **Environmental sampling group**
  (`EnvironmentalSamplingLocation`/`Sample`/`SampleResult`) — not
  queried; would additionally require `CorePlatform.Country`/`Region`
  and `Organization.Plant`, neither otherwise needed by this pass.
- **`IsotopeInventory`** — not queried; would additionally require the
  confirmed-absent `ReactorFleet.Equipment`.
- **`ZoneDoseSnapshot`, `RadiationTrendSummary`** — not queried;
  precomputed dashboard/trend tables, explicitly named in the sector
  purpose but not exercised by any verification query — same
  "sector-purpose-named but query-silent" pattern every Phase 2 sector
  has shown for at least one group.
- **Permit group** (`RadiationWorkPermit`, `RadiationWorkPermitZone`,
  `RadiationWorkPermitPerson`, `RadiationWorkPermitDoseRecord`) — not
  queried.
- **Integration links** (`RadiationEventLink` →
  `EventManagement.OperationalEvent`, `RadiationRobotMissionLink` →
  `Robotics.Mission`) — the whole-sector audit above specifically
  cleared both targets as real and buildable. Same third category
  Robotics' own ADR-023 named for its own two link tables: not blocked,
  not reconnected-now, *buildable and clean, deliberately left for a
  future pass because nothing in this sector's own verification surface
  asks for it yet*. Recorded explicitly, not silently dropped.

### Domain shape: a zone/monitor spine plus person-centric dose accountability, matching the design choice's own emphasis on measurable, auditable facts

`RadiationZone` carries its own three-lookup `NOT NULL` chain
(`RadiationZoneTypeId`/`RadiationZoneStatusId`/
`RadiationAreaClassificationId`) as a real internal invariant — a zone
cannot exist without a type, status, and classification, matching the
atlas's own shape. `RadiationMonitor` is nullable on all three of its
plant-topology anchors (`UnitId`/`EquipmentId`/`RadiationZoneId`) — a
monitor can exist before it is sited, matching `RadiationMonitor`'s own
DDL exactly (unlike `Robot.HomeUnitId`, which is likewise nullable, this
sector's own zone/monitor relationship is itself optional too).
`RadiationReading` is an append-only fact table (no audit columns in its
DDL, matching `RobotHealthSnapshot`'s and `EventTimelineEntry`'s own
append-only shape). The `Dosimeter`/`PersonDosimeterAssignment`/
`PersonDoseReading` chain is the sector's own explicit design
statement realized in code: *"personal exposure is tied to an assignment
window rather than a loose person ID"* — a dose reading cannot exist
without an assignment, and an assignment cannot exist without a real
`Organization.Person` passport, matching the atlas's `NOT NULL`
`PersonDosimeterAssignmentId`/`PersonId` chain exactly. `DoseLimit`/
`DoseAlert` is the threshold-and-alert layer, structurally the closest
analogue in this sector to `Robotics.MissionReadinessAssessment` — a
computed verdict record, not a command that changes anything by itself.

### Application layer: the atlas's own four named verification queries

1. `GetActiveRadiationZonesQuery` — zones with `IsDeleted = 0`, joined to
   `RadiationAreaClassification`/`RadiationZoneStatus`/(left)
   `ReactorFleet.Unit`.
2. `GetMonitorsWithCalibrationDueQuery` — monitors where
   `CalibrationDueAtUtc IS NOT NULL AND CalibrationDueAtUtc <=
   SYSUTCDATETIME()`, joined to `MonitorType`.
3. `GetLatestReadingPerMonitorQuery` — one row per monitor, most recent
   `RadiationReading`, with engineering-unit symbol and quality code.
4. `GetOpenDoseAlertsQuery` — `DoseAlert` rows where status is
   `OPEN`/`ACKNOWLEDGED`, joined through to the person and dose value
   that produced them.

Plus the two commands the sector's own core premise needs to produce
data for those four reads to have anything to report on:
`RegisterRadiationZoneCommand` (creates a `RadiationZone` against its
three-lookup chain) and `RecordRadiationReadingCommand` (creates a
`RadiationReading` against a `RadiationMonitor`) — same "read queries
need at least one write path to be provably real" reasoning every prior
sector's Application layer used.

### Persistence: shares `AlarmManagementDb` — all three axes agree cleanly

- **Topology.** RadiationMonitoring is plant-operational radiological
  data, physically colocated with the demonstrator plant — same category
  as `ReactorFleet`/`Instrumentation`/`DigitalTwin`/`Maintenance`/
  `EventManagement`/`Robotics`, all of which already share
  `AlarmManagementDb`.
- **Sensitivity.** Radiological zone/monitor/dose data carries no
  personnel-HR sensitivity (unlike `Organization`) and no access-control
  sensitivity (unlike `Security`) — ordinary operational/safety data, the
  same tier as its plant-operational siblings. (Personal dose *values*
  are sensitive in a real regulatory sense, but this codebase's own
  established sensitivity axis is about *database placement*, not
  field-level classification — no prior sector has split a table across
  databases by column sensitivity, and this one does not either.)
- **FK-locality.** Within the twenty-table scope, the real cross-context
  FKs are `RadiationZone.UnitId`/`RadiationMonitor.UnitId` →
  `ReactorFleet.Unit` and `RadiationReading.EngineeringUnitId`/
  `DoseLimit.EngineeringUnitId`/`PersonDoseReading.EngineeringUnitId` →
  `CorePlatform.EngineeringUnit` — both already live in
  `AlarmManagementDb`. Sharing makes both genuine same-database SQL
  `FOREIGN KEY`s (via the already-established `ReactorFleetUnitReference`
  and `CorePlatformEngineeringUnitReference` shadow-entity types — this
  is the first sector to need *both* shadow-entity families in the same
  `DbContext`, each a fresh local copy per this codebase's own "shadow
  entities stay local to their project" convention, not a new pattern).

Own migration history (`__EFMigrationsHistory_RadiationMonitoring`), own
schema (`RadiationMonitoring`), same physical database.
`Organization.Person` (`PersonDosimeterAssignment.PersonId`, `NOT NULL`)
and `Security.ApplicationUser` (`PersonDosimeterAssignment.
AssignedByUserId`, `DoseAlert.AcknowledgedByUserId`, both nullable) stay
passport-only, no enforced constraint — `OrganizationDb`/`SecurityDb` are
separate physical databases, the same downgrade every prior sector's
Organization/Security references has needed. Note `PersonId` is
passport-only *and* `NOT NULL` — a business requirement without a
database-enforced constraint, the same shape `Mission.UnitId` had in
Robotics before the shadow-entity fix was available for that particular
target; here it stays intentionally passport because the target
database itself is different, not because the target table is missing.

### `.sln` nesting discipline (verified before adding anything)

Confirmed via `grep -n 'Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") =
"Contexts", "Contexts"' Nexus1.Runtime.sln` — exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, before any RadiationMonitoring
project was added. A new `RadiationMonitoring` solution folder is created
and nested under that GUID in `GlobalSection(NestedProjects)`, matching
Robotics'/EventManagement's own entry shape
(`{new-folder-guid} = {981F0668-...}` for Domain/Application/
Infrastructure; `UnitTests`/`ComponentTests` nest under the shared
`tests` folder GUID `{DFD64979-71D4-46B5-BF62-217FA110CF39}`, confirmed
against the real `.sln` content directly, not assumed from a paraphrase).
Re-verified with the same `grep` after the edit — still exactly one
match.

## Consequences

- RadiationMonitoring becomes the ninth sector sharing `AlarmManagementDb`'s
  physical database.
- `RadiationEventLink` and `RadiationRobotMissionLink` are explicitly
  recorded as clean, buildable, not-yet-reconnected — both targets exist
  today, unlike a genuine whole-sector gap.
- `RadiationZone.EquipmentLocationId` and `RadiationMonitor.EquipmentId`
  are downgraded to plain nullable passport ints, no enforced FK — their
  targets (`ReactorFleet.EquipmentLocation`/`Equipment`) don't exist in
  this codebase, the same class of finding as every prior sector's
  identical `ReactorFleet.Equipment` gap.
- This is the second consecutive Phase 2 sector with a clean (zero-gap)
  whole-sector FK audit result.

## Rejected alternatives

- **Own database for RadiationMonitoring, on the theory that dose data
  deserves stricter isolation.** Considered given the real-world
  sensitivity of personal dose records. Rejected — this codebase's
  established sensitivity axis has never split a table across databases
  by field content, only by whether a whole *context* (Security,
  Organization) has its own compliance/access-control reason to be
  isolated. RadiationMonitoring's own real cross-context FKs point
  entirely at `AlarmManagementDb`-resident contexts; a separate database
  would force both into passport-only for no benefit this project's
  existing conventions would recognize as a real gain.
- **Include `RadiationEventLink`/`RadiationRobotMissionLink` now, since
  the whole-sector audit proved them buildable.** Rejected for *this*
  pass, same reasoning as Robotics' own ADR-023 rejected its two
  equivalents — "buildable" and "verification-query-justified" are
  different bars, and the atlas's own four named queries do not touch
  either link table.

## Evidence required

- `dotnet build` warning-clean.
- `dotnet test` green, including `Nexus1.ArchitectureTests`.
- Migration applied to the real `AlarmManagementDb`;
  `RadiationMonitoring.*` tables and the
  `FK_RadiationZone_Unit`/`FK_RadiationMonitor_Unit`/
  `FK_RadiationReading_EngineeringUnit`/`FK_DoseLimit_EngineeringUnit`/
  `FK_PersonDoseReading_EngineeringUnit` constraints confirmed via
  `sys.foreign_keys` against `ReactorFleet.Unit`/
  `CorePlatform.EngineeringUnit`.
- Real host startup; `GET /health/ready` returns `200 Healthy` with a
  `radiationmonitoring-db` check present.
- Evidence report written only after all of the above are independently
  confirmed — build, test, real host, health check, evidence report,
  commit, in that order.
