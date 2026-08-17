# ADR-021: Maintenance (Phase 2, sector 6) — scope, domain shape, and persistence

## Status

Accepted. **Amended by ADR-022** (EventManagement, Phase 2 sector 7):
`WorkOrder.OriginOperationalEventId`/`OriginIncidentActionId`, originally
built here as passport-only bigints with no enforced FK at all (because
`EventManagement` did not exist anywhere in this project yet), are now
real foreign keys to `EventManagement.OperationalEvent`/`IncidentAction`.
See ADR-022's own "Maintenance reconnection" section for the full
reasoning; this ADR's own text below is left as originally written except
for this note and the two inline call-outs marked "amended by ADR-022,"
so the historical record of what was actually decided at each point in
time stays intact rather than being silently rewritten.

## Context

Phase 2's sixth sector, atlas Appendix **C.9** (confirmed via the real
`"C.9.1 Sector purpose"` header, not the file's garbled table of
contents — full real sequence now confirmed through direct header reads:
C.1 CorePlatform, C.2 Security, C.3 Organization, C.4 ReactorFleet, C.5
Instrumentation, C.6 DigitalTwin, C.7 AlarmManagement, **C.8
EventManagement, C.9 Maintenance**, C.10 RootCause, C.11
ReinforcementLearning, C.12 Robotics, C.13 RadiationMonitoring — recorded
here so a future session doesn't have to re-derive it).

- `From_Schema_to_System` Appendix C.9: **forty-six tables** (eighteen
  lookup, twenty-eight substantive) — "the table count is deliberate:
  asset health, work execution, scheduling and material control have
  different lifecycles and should not be collapsed into one overloaded
  work-order table." C.9.1's own design choice: *"Physical identity
  stays in ReactorFleet. Maintenance adds maintainability, work
  execution, material planning and condition history. This prevents the
  same pump or valve from being described twice with competing
  identities."*
- `From_Domain_to_Twin`'s Chapter 14 classifies Maintenance as a
  **Supporting domain** (same tier as Instrumentation), with its own
  design question: *"Does history explain this behaviour?"* — pointing
  toward the condition/degradation history tables as the real substance
  worth modeling, not deep exploratory invariants.
- **C.9.1's own opening sentence names a dependency this project cannot
  yet satisfy**: *"Maintenance depends on ReactorFleet, Organization,
  Security, Instrumentation, CorePlatform and **EventManagement**."*
  EventManagement is atlas sector **C.8** — not yet built anywhere in
  this project (per CLAUDE.md's Phase 2 ordering, Maintenance comes
  before EventManagement in this project's build order, the reverse of
  the atlas's own dependency order). This is checked **before** writing
  any code, per the discipline established by Instrumentation's
  Equipment/PlantSystem correction and applied proactively for DigitalTwin:
  - `Maintenance.WorkOrderEventLink`'s entire reason to exist is linking
    a `WorkOrder` to an `EventManagement.OperationalEvent` — excluded
    entirely, same treatment as DigitalTwin's `TwinModelComponent`.
  - **Amended by ADR-022**: `WorkOrder.OriginOperationalEventId`/
    `OriginIncidentActionId` are now real FKs — see the Status note above.
    The reasoning immediately below describes the original, since-amended
    decision, kept for the historical record.
  - `WorkOrder.OriginOperationalEventId`/`OriginIncidentActionId` are
    both nullable — `WorkOrder` keeps a valid identity without them, so
    they become plain nullable `long`s with no enforced FK (there is no
    local table to reference at all, unlike Instrumentation's
    `EquipmentId`/`PlantSystemId` downgrade, which at least references a
    genuinely-absent-from-this-codebase table; here the referenced
    *sector* itself doesn't exist).
  - The atlas's own verification query 3 (*"Work orders opened because of
    operational events or incident actions"*) is explicitly built around
    this same relationship — adapted below rather than dropped, since
    `WorkOrder` correctly *storing* its origin passport ints is still
    real, provable behavior even without a local table to join against.
- `Maintenance.Asset.EquipmentId`/`SystemId` reference
  `ReactorFleet.Equipment`/`ReactorFleet.System` (the same table
  Instrumentation/DigitalTwin call `PlantSystem` — an atlas naming
  inconsistency, not a different table). Both nullable; both
  downgraded to passport-only ints, same finding and same treatment as
  Instrumentation's own correction (`ReactorFleetDbContext` only exposes
  `Unit`/`UnitPowerSnapshot`).
- The atlas's own five "useful verification queries" (C.9.5.2) name real
  Application-layer operations, exercising: `Asset` (+`AssetCategory`/
  `AssetStatus`), `WorkOrder` (+`WorkOrderStatus`/`WorkPriority`),
  `WorkOrder`'s event/incident origin (adapted per above), `AssetCondition`
  (+`ConditionGrade`), and `DegradationRecord`+`DegradationTrendPoint`
  (+`DegradationMechanism`/`FindingSeverity`). None of the five touches
  `Inspection`/`InspectionFinding` (despite being named in C.9.1's own
  bullet list — the same "sector-purpose-named but query-silent" pattern
  DigitalTwin's simulation capability showed), the inventory/spares group,
  the planning/scheduling group, or the evidence/approval group.

## Decision

### Scope: sixteen of forty-six tables — the atlas's own five named verification queries plus one small addition for FK integrity

**Built** (9 lookups + 7 substantive):

- Lookups: `AssetCategory`, `AssetStatus`, `AssetCriticality` (all three
  required by `Asset`'s own DDL — `AssetCriticalityId` is `NOT NULL`,
  confirmed by reading the real DDL, not the abbreviated table-list FK
  summary, which omits it), `ConditionGrade`, `DegradationMechanism`,
  `FindingSeverity`, `WorkOrderType` (required by `WorkOrder`'s own DDL —
  same catch, absent from the abbreviated summary), `WorkOrderStatus`,
  `WorkPriority`.
- `Asset` — C.9.1's own anchor table and verification query 1's subject;
  the FK-mapping table's own first-listed, explicitly-narrated
  relationship (*"Every maintainable asset belongs to a physical unit"*).
- `AssetComponent` — not itself touched by any verification query, but
  included because `DegradationRecord.AssetComponentId` (nullable) needs
  a real internal FK target to be meaningful, and the table itself is
  small, self-contained (a self-referencing subcomponent tree), and
  cheap relative to the alternative of a passport-only int pointing at
  nothing in this database at all.
- `AssetCondition` + `AssetConditionMeasurement` — verification query 4's
  subject, built as a pair for the same reason DigitalTwin's
  `TwinSnapshot`/`TwinSnapshotValue` were: `AssetCondition` alone (a
  health score and RUL estimate) would be an unsupported opinion: C.9.8's
  own boundary insists the sector "records maintenance reality," and
  `AssetConditionMeasurement` is what ties a condition assessment to
  real measured evidence (`Instrumentation.Signal`).
- `DegradationRecord` + `DegradationTrendPoint` — verification query 5's
  subject.
- `WorkOrder` — verification queries 2 and 3's subject.

**Not built, with reasoning per group** (9 lookups + 21 substantive):

- **`InspectionType`, `DocumentType`, `ApprovalStatus`, `SupplierStatus`,
  `MaintenancePlanType`, `ScheduleBasis`, `TaskStatus`, `LabourRole`,
  `MaterialUsageType`** (9 lookups) — each backs only an excluded group
  below.
- **`AssetStatusHistory`, `WorkOrderStatusHistory`** (2, append-only
  audit-trail tables) — `Asset.AssetStatusId`/`WorkOrder.
  WorkOrderStatusId` already carry current status; no verification query
  touches either history table, and this project's own restraint
  discipline already treats audit-trail-shaped tables as secondary to
  current-state tables elsewhere.
- **`Inspection`, `InspectionFinding`** (2) — named in C.9.1's own bullet
  list, but zero verification-query consumer, the same "named but
  query-silent" exclusion already applied to DigitalTwin's simulation
  capability.
- **`MaintenanceDocument`, `MaintenanceApproval`** (2, evidence/approval
  group) — zero verification-query consumer.
- **`Supplier`, `SupplierPart`, `SparePart`, `SparePartStock`,
  `AssetSparePart`** (5, inventory group) — zero verification-query
  consumer; also the group with the most tangled cross-database pull
  (`SparePartStock.PlantId` → `Organization.Plant`, in a different
  physical database than this sector's own chosen home, see Persistence
  below), so deferring it also avoids a decision this scope doesn't need
  to make yet.
- **`MaintenancePlan`, `MaintenancePlanTask`, `MaintenanceSchedule`,
  `MaintenanceScheduleOccurrence`, `MaintenanceWindow`** (5, planning
  group) — named in C.9.1's own bullet list ("MaintenancePlan and
  MaintenanceSchedule define recurring or condition-based work"), but
  zero verification-query consumer; `WorkOrder.MaintenancePlanId`/
  `MaintenanceWindowId` are both nullable, so `WorkOrder` keeps valid
  identity without them (passport-only ints, no enforced FK, pointing at
  tables that genuinely could exist in a future step, unlike the
  EventManagement case).
- **`WorkOrderTask`, `WorkOrderLabour`, `WorkOrderMaterial`,
  `WorkOrderEventLink`** (4, work-order substructure) — zero
  verification-query consumer; `WorkOrderEventLink` additionally excluded
  outright because its entire purpose requires the not-yet-built
  `EventManagement` sector (see Context).

This lands at 16/46 ≈ 35% — close to Instrumentation's 38%, the expected
range for a Supporting-domain sector, and well below Organization's 68%
or DigitalTwin's 48% (Core domain).

### Domain shape: Supporting-domain history and evidence, not deep exploratory modeling

Matching the book's own design question ("Does history explain this
behaviour?"): `Asset`, `AssetComponent`, `WorkOrder` get `Create`
factories enforcing the atlas's real constraints. `AssetCondition.Create`
enforces `HealthScorePercent` in `[0, 100]` when set.
`AssetConditionMeasurement` is the real evidence-linking behavior — a
condition assessment gets measurements attached, mirroring the atlas's
own two-table design. `DegradationRecord` gets a real open/close
lifecycle (`IsActive`, `ClosedAtUtc`) with a `Close(DateTime)` method,
and `DegradationTrendPoint` rows attach real trend evidence to it,
mirroring `AssetCondition`/`AssetConditionMeasurement`'s own pairing
pattern. Audit columns not modeled in Domain, same restraint as every
prior sector.

### Application layer: the atlas's own five named verification queries, one adapted for the EventManagement gap

- `GetAssetsByUnitQuery` — C.9.5.2 query 1, verbatim.
- `GetOpenWorkOrdersByUnitQuery` — C.9.5.2 query 2, verbatim.
- `GetWorkOrdersWithOriginQuery` — C.9.5.2 query 3, **adapted**: returns
  `WorkOrderCode`/`Title` plus the raw `OriginOperationalEventId`/
  `OriginIncidentActionId` passport values themselves, rather than
  joining `EventManagement.OperationalEvent`/`IncidentAction` (which
  don't exist in this database). Proves the real, buildable half of the
  atlas's own intent — that `WorkOrder` correctly records *which*
  event/incident triggered it — without fabricating a join against
  tables this project hasn't built.
- `GetLatestConditionPerAssetQuery` — C.9.5.2 query 4, verbatim.
- `GetActiveDegradationCasesQuery` — C.9.5.2 query 5, verbatim.
- `RecordAssetConditionCommand` — writes one `AssetCondition` plus its
  `AssetConditionMeasurement` rows in one operation, matching how an
  assessment actually gets produced.
- `OpenWorkOrderCommand` — `WorkOrder`'s defining behavior.
- `RecordDegradationCommand` — writes one `DegradationRecord` plus its
  initial `DegradationTrendPoint` rows.

### Persistence: shares `AlarmManagementDb` — a genuine, weighed tradeoff, not a clean case like Instrumentation/DigitalTwin

This is the first Phase 2 sector where the FK-locality argument doesn't
point cleanly one direction. Maintenance's real, buildable external
references split across **two different physical databases**:

- `Asset.UnitId` → `ReactorFleet.Unit`, `AssetConditionMeasurement.
  SignalId`/`DegradationTrendPoint.SourceSignalId` → `Instrumentation.
  Signal`, `AssetConditionMeasurement.EngineeringUnitId`/
  `DegradationTrendPoint.EngineeringUnitId` → `CorePlatform.
  EngineeringUnit` — all three targets live in `AlarmManagementDb`.
- `WorkOrder.AssignedTeamId`/`AssignedPersonId` → `Organization.Team`/
  `Person` — both targets live in `OrganizationDb` (ADR-017's own
  database), a *different* physical database.

**Two options, weighed explicitly:**

- **Option A — share `AlarmManagementDb`** (chosen): keeps `Asset.UnitId`
  (the sector's own explicitly-narrated anchor relationship),
  `AssetConditionMeasurement`'s and `DegradationTrendPoint`'s
  signal/engineering-unit references (the real evidence backing two of
  the sector's five named verification queries and both of its
  condition/degradation history pillars) as real FKs. Cost:
  `WorkOrder.AssignedTeamId`/`AssignedPersonId` (one table, two columns)
  become passport-only ints.
- **Option B — share `OrganizationDb`**: keeps `WorkOrder`'s two
  team/person assignment columns as real FKs. Cost: `Asset.UnitId` (the
  sector's own anchor), plus every signal/engineering-unit reference in
  both `AssetConditionMeasurement` and `DegradationTrendPoint` (four
  columns across two tables, directly backing two of the five named
  verification queries), become passport-only.

Option A is chosen: it preserves the relationship the atlas itself
narrates as the anchor, and the relationships backing the larger,
more central share of this scope's own verification queries and
sector-purpose pillars (physical identity + condition/degradation
history), at the cost of a single table's two assignment columns —
a materially smaller loss than Option B's.

Own `Maintenance` schema in `AlarmManagementDb`, own migration-history
table (`__EFMigrationsHistory_Maintenance`). Real FKs to `ReactorFleet.
Unit`, `Instrumentation.Signal`, `CorePlatform.EngineeringUnit` via the
now-twice-established `ExcludeFromMigrations` shadow-entity technique
(ADR-019/ADR-020) — Maintenance needs its own local copies (shadow
entities are per-project, not shared across contexts, per DigitalTwin's
own precedent). `WorkOrder.AssignedTeamId`/`AssignedPersonId` (→
`OrganizationDb`), every `Security.ApplicationUser` reference (→
`SecurityDb`), and `WorkOrder.OriginOperationalEventId`/
`OriginIncidentActionId` (→ nonexistent `EventManagement`) are all
passport-only, no enforced constraint.

### `.sln` nesting discipline (per the just-fixed structural issue)

This sector's projects must be added under the **existing** `Contexts`
solution folder (`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, itself nested
under `src`) — not a newly-created folder of the same name. Verify by
grepping the `.sln` for `"Contexts", "Contexts"` before adding any new
`Project(...)` entries, confirming there is exactly one such entry, and
reusing its GUID directly in the new `GlobalSection(NestedProjects)`
line. This is process discipline recorded here because it has now failed
silently on three of five prior Phase 2 sectors (see the dedicated `.sln`
fix commit preceding this ADR).

## Consequences

- `Nexus1.Maintenance.Domain`, `Nexus1.Maintenance.Application`,
  `Nexus1.Maintenance.Infrastructure` — composed into
  `Nexus1.ModularRuntime` only, sharing `AlarmManagementDb` alongside
  `ReactorFleet`/`CorePlatform`/`AlarmManagement`/`Instrumentation`/
  `DigitalTwin`.
- Thirty tables remain unbuilt, named explicitly above in seven groups.
- `WorkOrder.AssignedTeamId`/`AssignedPersonId` are passport-only despite
  `Organization` existing — a direct, named consequence of the
  Option A/B tradeoff above, not an oversight.
- `WorkOrder.OriginOperationalEventId`/`OriginIncidentActionId` and the
  entire `WorkOrderEventLink` table are the first Phase 2 instance of a
  dependency on a sector that doesn't exist *at all* yet (not just a
  table Phase 1 chose to trim, like `ReactorFleet.Equipment`) — when
  `EventManagement` (atlas C.8) is eventually built, per this project's
  own Phase 2 order that will happen *after* Maintenance, revisiting
  these two columns to become real FKs is a natural, explicitly-flagged
  future step, distinct from the Organization/Instrumentation/DigitalTwin
  reversal notes (those involve Phase 1 contexts that exist but were
  never wired; this one involves a sector that doesn't exist at all yet).
  **This prediction came true**: ADR-022 built `EventManagement` next as
  planned and performed exactly this reconnection — see the Status note
  at the top of this ADR.

## Rejected alternatives

- **Build `WorkOrderEventLink` with a passport-only `OperationalEventId`.**
  Rejected: the table's entire schema and purpose (`WorkOrderEventLinkId`,
  `WorkOrderId`, `OperationalEventId` — nothing else) is the link itself;
  a passport-only version would be a table with one meaningful column and
  no way to verify it means anything, worse than not building it, the
  same reasoning DigitalTwin applied to `TwinModelComponent`.
- **Give Maintenance its own physical database to sidestep the
  Organization/ReactorFleet split.** Rejected: this would downgrade every
  external reference to passport-only, losing the real-FK benefit for the
  sector's own most-emphasized relationship (`Asset.UnitId`) for no
  sensitivity reason — Maintenance's data (asset status, condition scores,
  work order titles) is no more sensitive than Instrumentation's or
  DigitalTwin's.
- **Share `OrganizationDb` instead of `AlarmManagementDb`.** Rejected per
  the explicit Option A/B weighing above — Option B loses more real-FK
  coverage across more of the sector's own named verification queries.

## Evidence required

- Domain unit tests, no persistence, for all seven substantive entities'
  creation validation and real behaviors (`AssetCondition`'s health-score
  range check, `DegradationRecord`'s open/close lifecycle).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.Maintenance.Infrastructure`, targeting the `Maintenance` SQL
  schema against the existing `AlarmManagementDb`, independent migration
  history, real foreign keys to `ReactorFleet.Unit`, `Instrumentation.
  Signal`, `CorePlatform.EngineeringUnit` — verified directly against
  `sys.foreign_keys` on the live database.
- Component tests against real LocalDB for the eight Application-layer
  operations, including all five atlas verification queries (query 3
  proven in its adapted, honest form) against real seeded data spanning
  `ReactorFleet`, `Instrumentation`, `CorePlatform`, and `Maintenance`
  migrations together.
- `Nexus1.ArchitectureTests` passing with `Nexus1.Maintenance.*` composed
  into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with a `maintenance-db` health check reaching
  `AlarmManagementDb`, confirmed with the ADR-018-strengthened
  `DbContextHealthCheck<T>`.
- `.sln` nesting verified directly (not assumed): after adding
  Maintenance's projects, confirm exactly one `"Contexts", "Contexts"`
  solution-folder entry exists in the `.sln` and that Maintenance's own
  folder GUID has a `GlobalSection(NestedProjects)` mapping to it.
