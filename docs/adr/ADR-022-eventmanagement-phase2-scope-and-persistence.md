# ADR-022: EventManagement (Phase 2, sector 7) — scope, domain shape, persistence, and the Maintenance reconnection

## Status

Accepted.

## Context

Phase 2's seventh sector, atlas Appendix **C.8** (confirmed via the real
`"C.8.1 Sector purpose"` header — the real atlas sequence through this
point, now fully confirmed by direct header reads across three
consecutive ADRs: C.1 CorePlatform, C.2 Security, C.3 Organization, C.4
ReactorFleet, C.5 Instrumentation, C.6 DigitalTwin, C.7 AlarmManagement,
**C.8 EventManagement**, C.9 Maintenance).

- `From_Schema_to_System` Appendix C.8: **forty-two tables** (eighteen
  lookup, twenty-four substantive). C.8.1's own design choice:
  *"Incidents and near misses are not separate universes. Both are
  specialized records anchored to one OperationalEvent. This lets
  reporting ask for all events, while specialist screens can filter to
  incidents or near misses only."*
- `From_Domain_to_Twin` has **zero mentions of EventManagement anywhere**
  — absent from the Core/Supporting/Generic classification tables and
  every other chapter, the same situation Organization was in (the book
  predates this sector's addition to full atlas scope). No classification
  to lean on; scope and domain-shape decisions below are derived directly
  from the atlas's own signals (verification queries, FK mapping,
  design-choice callouts), the same discipline already used for
  Organization.
- The atlas's own three "useful verification queries" (C.8.5.2) name real
  Application-layer operations: (1) an event with its linked alarms and
  flood, (2) an event's replayable timeline, (3) open/overdue incident
  actions. Reading each query's own `JOIN` list confirmed none touches
  `EventParticipant`/`EventNote`/`EventAttachment` (named in C.8.1's own
  bullet list — the same "sector-purpose-named but query-silent" pattern
  every Phase 2 sector so far has shown), `NearMiss` (also named,
  design-choice-relevant, but `Incident` alone already demonstrates the
  specialization-without-duplication pattern the design choice describes,
  so it does not carry the same "the sector's core premise can't be
  honored without it" weight `TwinDivergenceReview` had for DigitalTwin),
  or `IncidentReview`/`IncidentReviewFinding` (also named, but only
  `IncidentAction` is exercised by query 3).
- **Cross-context FK buildability, checked directly before writing any
  code**: this is the first Phase 2 sector whose own dependency list
  (`ReactorFleet`, `Organization`, `Security`, `Instrumentation`, and —
  new here — **`AlarmManagement`**) includes a Phase 1 context that
  actually has tables worth referencing. `EventAlarmLink.AlarmEventId` →
  `AlarmManagement.AlarmEvent` and `EventFloodLink.AlarmFloodId` →
  `AlarmManagement.AlarmFlood` — both confirmed to exist, both `BIGINT`
  identity, both mapped to real tables (`AlarmManagement.AlarmEvent`/
  `AlarmManagement.AlarmFlood`) in `AlarmManagementDb`, verified directly
  by reading `AlarmEventConfiguration.cs`/`AlarmFloodConfiguration.cs`
  rather than assumed from the atlas alone. `EventEquipmentLink.
  EquipmentId` → `ReactorFleet.Equipment` — the now-familiar absent-table
  finding, same treatment as every prior sector.

## Decision

### Scope: fifteen of forty-two tables — the atlas's own three named verification queries, nothing more

**Built** (9 lookups + 6 substantive):

- Lookups: `EventType`, `EventStatus`, `EventSeverity`, `EventSourceType`
  (all four required by `OperationalEvent`'s own DDL — `EventSourceTypeId`
  is easy to miss from the abbreviated table-list summary, caught by
  reading the real DDL, same discipline as Maintenance's
  `AssetCriticality`/`WorkOrderType` catch), `EventTimelineEntryType`,
  `IncidentType`, `IncidentStatus`, `IncidentActionType`,
  `IncidentActionStatus`.
- `OperationalEvent` — C.8.1's own anchor ("the one place where an
  operational occurrence receives a title, owner, severity, status and
  lifecycle") and query 1's subject.
- `EventAlarmLink`, `EventFloodLink` — query 1's other subjects; the
  sector's first real FKs into `AlarmManagement` (see Persistence below).
- `EventTimelineEntry` — query 2's subject.
- `Incident`, `IncidentAction` — query 3's subject.

**Not built, with reasoning per group** (9 lookups + 18 substantive):

- **`EventRelationshipType`, `EventParticipantRole`, `EventAttachmentType`,
  `EventNoteType`, `NearMissType`, `BarrierType`,
  `IncidentClassification`, `IncidentReviewOutcome`, `EventImpactType`**
  (9 lookups) — each backs only an excluded group below.
- **`EventStatusHistory`** (1, append-only audit trail) — `OperationalEvent.
  EventStatusId` already carries current status; same restraint as every
  prior sector's history-table exclusions (Maintenance's
  `AssetStatusHistory`/`WorkOrderStatusHistory`).
- **`EventRelationship`, `EventTag`, `EventTagAssignment`** (3,
  cross-event linking/tagging) — zero verification-query consumer.
- **`EventSignalLink`, `EventEquipmentLink`** (2, further link tables) —
  zero verification-query consumer (unlike `EventAlarmLink`/
  `EventFloodLink`, which query 1 exercises directly);
  `EventEquipmentLink.EquipmentId` additionally can't be a real FK at all
  (`ReactorFleet.Equipment` doesn't exist).
- **`EventParticipant`, `EventNote`, `EventAttachment`** (3, named in
  C.8.1's bullet list, zero verification-query consumer).
- **`EventImpactAssessment`, `EventUnitStateSnapshot`** (2, context group)
  — zero verification-query consumer.
- **`NearMiss`, `NearMissBarrier`** (2) — named in C.8.1's own design
  choice, but `Incident` alone (which query 3 exercises) already proves
  the specialization-without-duplication pattern the design choice
  describes; no verification query touches `NearMiss` itself.
- **`IncidentClassificationAssignment`** (1) — zero verification-query
  consumer.
- **`IncidentActionOwner`** (1) — zero verification-query consumer;
  `IncidentAction`'s own real behavior (status, due date, verification)
  is in scope without it.
- **`IncidentReview`, `IncidentReviewFinding`** (2) — named in C.8.1's
  bullet list alongside `IncidentAction`, but only `IncidentAction` is
  exercised by query 3.

This lands at 15/42 ≈ 36% — squarely in the same range as Instrumentation
(38%) and Maintenance (35%), the expected band for a sector with no
Core-domain classification and a modest (three-query) verification
signal.

### Domain shape: real invariants on the base-event/specialization pattern the atlas itself designs around

No book classification to lean on, so the domain shape is derived
directly from the atlas's own explicit design choice: `Incident` is
built as a genuine specialization anchored to `OperationalEvent`
(`Incident.OperationalEventId` is unique — one incident per event, not a
free-floating record), matching the atlas's own *"specialized records
anchored to one OperationalEvent"* language exactly. `OperationalEvent.
Create` enforces required fields and the atlas's real defaults
(`IsDrill` default false). `IncidentAction` gets real lifecycle fields
(`DueAtUtc`/`CompletedAtUtc`/`VerifiedAtUtc`) with a `Complete(DateTime)`/
`Verify(DateTime, int?)`-shaped pair of behaviors, matching query 3's own
"open, overdue, nearing due date" framing — an action's own real
lifecycle, not just storage. Audit columns not modeled in Domain, same
restraint as every prior sector.

### Application layer: the atlas's own three named verification queries

- `GetEventWithAlarmsAndFloodQuery` — C.8.5.2 query 1, verbatim.
- `GetEventTimelineQuery` — C.8.5.2 query 2, verbatim.
- `GetOpenIncidentActionsQuery` — C.8.5.2 query 3, verbatim.
- `ReportOperationalEventCommand` — `OperationalEvent`'s defining
  behavior.
- `LinkEventToAlarmCommand`/`LinkEventToFloodCommand` — `EventAlarmLink`/
  `EventFloodLink`'s defining behavior.
- `OpenIncidentCommand` — `Incident`'s defining behavior (from an
  existing `OperationalEvent`).
- `RecordIncidentActionCommand` — `IncidentAction`'s defining behavior.

### Persistence: shares `AlarmManagementDb` — clear-cut, unlike Maintenance's genuine tradeoff

Re-derived per this sector's own facts:

- **Deployment topology**: no independent deployment, same as every
  prior sector.
- **Data sensitivity**: event titles, timelines, and incident records are
  operational, not PII or credential-bearing.
- **FK locality**: `OperationalEvent.UnitId` → `ReactorFleet.Unit`,
  `EventAlarmLink.AlarmEventId` → `AlarmManagement.AlarmEvent`,
  `EventFloodLink.AlarmFloodId` → `AlarmManagement.AlarmFlood` — all
  three targets already live in `AlarmManagementDb`. Unlike Maintenance,
  this sector's *only* Organization-side reference in scope
  (`OperationalEvent.PlantId`, nullable) is a single column on a single
  table — not enough to create a genuine tradeoff the way Maintenance's
  two `WorkOrder` assignment columns did. This is a clear-cut case,
  closer to Instrumentation/DigitalTwin than to Maintenance.

Composed into `Nexus1.ModularRuntime` sharing `AlarmManagementDb` (own
`EventManagement` SQL schema, own migration-history table
`__EFMigrationsHistory_EventManagement`). Real FKs to `ReactorFleet.Unit`,
`AlarmManagement.AlarmEvent`, `AlarmManagement.AlarmFlood` via the
now-three-times-established `ExcludeFromMigrations` shadow-entity
technique — this sector's own local copies include the **first-ever
shadow references into `AlarmManagement`** (`AlarmManagementAlarmEvent
Reference`, `AlarmManagementAlarmFloodReference`), alongside a
`ReactorFleetUnitReference`. `OperationalEvent.PlantId` (→
`Organization.Plant`, `OrganizationDb`) and every `Security.
ApplicationUser` reference stay passport-only, no enforced constraint.

## The Maintenance reconnection — decided explicitly, not left as another open door

Per the user's explicit instruction, this is treated differently from
every prior reversal note (ADR-004's `SiteId`/`LineId`, Instrumentation's
`SignalTag`, DigitalTwin's triple note): the blocked dependency is
resolved by *this very sector*, not a hypothetical future one, so the
decision is made now rather than deferred again.

**Reconnected**: `Maintenance.WorkOrder.OriginOperationalEventId` →
`EventManagement.OperationalEvent(OperationalEventId)` and
`Maintenance.WorkOrder.OriginIncidentActionId` →
`EventManagement.IncidentAction(IncidentActionId)` become real, enforced
foreign keys. Reasoning: both `EventManagement` and `Maintenance` share
`AlarmManagementDb` (this ADR's own persistence decision, made
independently of the reconnection question, happens to land both
sectors in the same physical database), making the change mechanically
small — two new local shadow-entity types in `Nexus1.Maintenance.
Infrastructure` (`EventManagementOperationalEventReference`,
`EventManagementIncidentActionReference`, following the same pattern
Maintenance already uses for its three existing shadow references), one
`WorkOrderConfiguration.cs` update, and one follow-up EF migration on
`Nexus1.Maintenance.Infrastructure` (not touching the already-applied
`InitialMaintenanceSchema` migration). Critically, this directly upgrades
`Maintenance.GetWorkOrdersWithOriginQuery` — which ADR-021 explicitly
built as an *adapted* version of the atlas's own literal C.9.5.2 query 3
specifically because this link didn't exist — into the atlas's actual
query (a real `LEFT JOIN` against real tables), closing a gap ADR-021
itself named as a known, deliberate compromise. This is a quality
correction to already-shipped work, backed by a verification query that
was already in scope, not new speculative scope.

**Still deferred**: `Maintenance.WorkOrderEventLink`. Its exclusion in
ADR-021 rested on *two* independent reasons — the missing
`EventManagement` dependency, and (independently) zero verification-query
consumer among Maintenance's own five named queries. `EventManagement`'s
existence resolves only the first reason; the second still holds
unchanged (none of Maintenance's five verification queries touch
`WorkOrderEventLink`, and this ADR's own EventManagement queries don't
either). Building it now would be scope creep beyond what any atlas
signal justifies — the same restraint this project has applied
consistently to every other query-silent table, not a special case.

**Sequencing**: EventManagement's own build (Domain/Infrastructure/
Application/tests/host verification) is this ADR's primary subject and
lands as its own commit. The Maintenance reconnection is a small,
separate follow-up commit performed after EventManagement's own tables
exist and are migrated — the same "own commit, own moment" discipline
already used for the `.sln` nesting fix, since it touches a different,
previously-shipped sector's Infrastructure layer and migration history,
not EventManagement's own scope.

## Consequences

- `Nexus1.EventManagement.Domain`, `Nexus1.EventManagement.Application`,
  `Nexus1.EventManagement.Infrastructure` — composed into
  `Nexus1.ModularRuntime` only, sharing `AlarmManagementDb` alongside
  `ReactorFleet`/`CorePlatform`/`AlarmManagement`/`Instrumentation`/
  `DigitalTwin`/`Maintenance`.
- Twenty-seven tables remain unbuilt, named explicitly above in ten
  groups.
- `Maintenance.WorkOrder.OriginOperationalEventId`/
  `OriginIncidentActionId` become real FKs in a follow-up commit;
  `Maintenance.WorkOrderEventLink` stays deferred, for an independently
  still-valid reason.
- `Maintenance`'s own `ADR-021` gets a short corrective note recording
  the reconnection, matching this project's convention of updating prior
  ADRs when a real, later-discovered fact changes their stated
  consequences.

## Rejected alternatives

- **Build `NearMiss` alongside `Incident` since C.8.1's design choice
  names both together.** Rejected: `Incident` alone, which query 3
  exercises, already demonstrates the specialization pattern the design
  choice describes; `NearMiss` itself has no verification-query consumer,
  the same bar every other exclusion in this sector was held to.
- **Rebuild `Maintenance.WorkOrderEventLink` now that `EventManagement`
  exists, alongside the `WorkOrder` origin-column reconnection.**
  Rejected: its exclusion never depended solely on the missing
  dependency — the independent "no verification-query consumer" reason
  still applies unchanged.
- **Fold the Maintenance reconnection into EventManagement's own commit.**
  Rejected: it touches a different sector's already-shipped Infrastructure
  layer and needs its own migration and verification pass — mixing it
  into EventManagement's commit would blur what each commit actually
  changed, the same reasoning that kept the `.sln` fix separate from
  DigitalTwin's own commit.

## Evidence required

- Domain unit tests, no persistence, for all six substantive entities'
  creation validation and real behaviors (`IncidentAction`'s
  complete/verify lifecycle).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.EventManagement.Infrastructure`, targeting the
  `EventManagement` SQL schema against the existing `AlarmManagementDb`,
  independent migration history, real foreign keys to `ReactorFleet.Unit`,
  `AlarmManagement.AlarmEvent`, `AlarmManagement.AlarmFlood` — verified
  directly against `sys.foreign_keys` on the live database.
- Component tests against real LocalDB for the seven Application-layer
  operations, including all three atlas verification queries against
  real seeded data spanning `ReactorFleet`, `AlarmManagement`, and
  `EventManagement` migrations together.
- `Nexus1.ArchitectureTests` passing with `Nexus1.EventManagement.*`
  composed into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with an `eventmanagement-db` health check reaching
  `AlarmManagementDb`.
- `.sln` nesting verified directly both before and after (exactly one
  `"Contexts", "Contexts"` entry), per the now-standing discipline.
- **Separately**: the Maintenance reconnection follow-up — a passing
  `Nexus1.Maintenance.UnitTests`/`ComponentTests` re-run, a new
  Maintenance migration reviewed and applied, and a direct
  `sys.foreign_keys` check confirming the two new
  `Maintenance.WorkOrder` → `EventManagement` constraints are live.
