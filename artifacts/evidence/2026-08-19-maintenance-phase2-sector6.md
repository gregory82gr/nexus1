# Evidence: Maintenance (Phase 2, sector 6 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 (portable, Erlang/OTP 27).

Scope and decisions are recorded in
`docs/adr/ADR-021-maintenance-phase2-scope-and-persistence.md`. This
report is the real proof: sixteen of forty-six atlas tables modeled in
Domain (the atlas's own five named verification queries plus one small
addition for internal FK integrity), EF Core persistence sharing
`AlarmManagementDb` with six real cross-schema foreign keys, composed
into `Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still green, a
real host startup with a working `maintenance-db` health check, and a
correctly-nested `.sln` solution folder — the first sector built since
the dedicated `.sln` nesting fix, so also the first real proof that fix's
discipline holds under a live implementation pass, not just the repair
itself.

## Automated regression: 603/603 passing (was 542/542 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.CorePlatform.UnitTests                     46/46 passed
Nexus1.Security.UnitTests                         31/31 passed
Nexus1.Organization.UnitTests                     97/97 passed
Nexus1.Instrumentation.UnitTests                  52/52 passed
Nexus1.DigitalTwin.UnitTests                      55/55 passed
Nexus1.Maintenance.UnitTests                      47/47 passed  (new)
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed
Nexus1.AlarmManagement.ComponentTests              19/19 passed
Nexus1.Audit.ComponentTests                       13/13 passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   16/16 passed
Nexus1.CorePlatform.ComponentTests                 9/9  passed
Nexus1.Security.ComponentTests                    14/14 passed
Nexus1.Organization.ComponentTests                15/15 passed
Nexus1.ServiceDefaults.ComponentTests              3/3  passed
Nexus1.Instrumentation.ComponentTests             15/15 passed
Nexus1.DigitalTwin.ComponentTests                 11/11 passed
Nexus1.Maintenance.ComponentTests                 14/14 passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors. Architecture tests confirmed
standalone (7/7) as well as within the full serial run.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.9** (confirmed via the real
  `"C.9.1 Sector purpose"` header). The full real atlas sequence through
  this point is now confirmed end-to-end by direct header reads: C.1
  CorePlatform, C.2 Security, C.3 Organization, C.4 ReactorFleet, C.5
  Instrumentation, C.6 DigitalTwin, C.7 AlarmManagement, C.8
  EventManagement, **C.9 Maintenance** — recorded in ADR-021 so a future
  session doesn't re-derive it.
- `From_Domain_to_Twin` classifies Maintenance as a **Supporting domain**
  with the design question "Does history explain this behaviour?" —
  informing the condition/degradation-history-first scope below.
- **C.9.1's own opening sentence names a dependency this project cannot
  satisfy yet**: Maintenance depends on `EventManagement` (atlas C.8),
  which doesn't exist anywhere in this project. This is a new class of
  gap from Instrumentation's/DigitalTwin's `Equipment`/`PlantSystem`
  finding — not a table Phase 1 chose to trim, but an entire sector this
  project hasn't reached yet. `WorkOrderEventLink` (whose only purpose is
  that exact link) was excluded entirely; `WorkOrder.
  OriginOperationalEventId`/`OriginIncidentActionId` kept as honest
  passport-only values with no local target table at all.
- `Maintenance.Asset.EquipmentId`/`SystemId` reference `ReactorFleet.
  Equipment`/`ReactorFleet.System` — the same absent table
  Instrumentation/DigitalTwin call `PlantSystem` (an atlas naming
  inconsistency, not a different table). Both downgraded to passport-only,
  the same finding and treatment as Instrumentation's own correction.
- The atlas's own five verification queries (C.9.5.2) named the
  Application layer's real operations; reading their exact `JOIN` lists
  confirmed none touches `Inspection`/`InspectionFinding` (despite being
  named in C.9.1's own bullet list — the same "sector-purpose-named but
  query-silent" pattern DigitalTwin's simulation capability showed), the
  inventory/spares group, the planning/scheduling group, or the
  evidence/approval group.
- Two easy-to-miss required columns, absent from the atlas's own
  abbreviated table-list FK summary and only visible in the real DDL,
  were caught before coding: `Asset.AssetCriticalityId` (`NOT NULL`) and
  `WorkOrder.WorkOrderTypeId` (`NOT NULL`) — both lookups added to scope.

Full reasoning for what was built vs. deliberately not built, and the
Option A/B persistence tradeoff, is in ADR-021; not repeated here.

## Domain layer — seven substantive entities, history and evidence behavior

`Nexus1.Maintenance.Domain`: 9 lookups and 7 substantive entities
(`Asset`, `AssetComponent`, `AssetCondition`, `AssetConditionMeasurement`,
`DegradationRecord`, `DegradationTrendPoint`, `WorkOrder`), each with a
`Create` factory enforcing the atlas's real constraints.

**`DegradationRecord`'s open/close lifecycle verified directly by reading
the source, not taken on report**: `Create` starts `IsActive = true`/
`ClosedAtUtc = null`; `Close(DateTime)` sets `IsActive = false` and
`ClosedAtUtc` — confirmed in `DegradationRecord.cs`, matching ADR-021
exactly. `AssetCondition.Create` enforces `HealthScorePercent` in
`[0, 100]` when set. Audit columns not modeled in Domain; several tables
in this sector are genuinely leaner than most (`AssetCondition`,
`AssetConditionMeasurement`, `DegradationTrendPoint` don't carry the full
audit-column set — matching the atlas's own DDL, not a simplification).

47 unit tests: creation validation for all 16 entities plus every real
behavior, including both boundary values (0 and 100) of the health-score
check and the degradation open/close lifecycle.

## EF Core Infrastructure — shares AlarmManagementDb, sixteen configurations, six real cross-schema FKs

`Nexus1.Maintenance.Infrastructure`: `MaintenanceDbContext` targeting the
existing `AlarmManagementDb` (own `Maintenance` schema, own
migration-history table `__EFMigrationsHistory_Maintenance`). Migration
inspected directly: exactly 16 `CreateTable` calls, 24 foreign keys all
`Restrict`, 1 `CHECK` constraint (`AssetCondition`'s health-score range).

**Cross-context real FKs verified against the live database, not just the
migration file**: after applying the migration to the actual
`AlarmManagementDb`, a direct `sys.foreign_keys` query confirms exactly
six live cross-schema constraints — `FK_Maintenance_Asset_Unit`,
`FK_Maintenance_WorkOrder_Unit` (both → `ReactorFleet.Unit`),
`FK_Maintenance_AssetConditionMeasurement_EngineeringUnit`,
`FK_Maintenance_DegradationTrendPoint_EngineeringUnit` (both →
`CorePlatform.EngineeringUnit`), `FK_Maintenance_
AssetConditionMeasurement_Signal`, `FK_Maintenance_DegradationTrendPoint_
SourceSignal` (both → `Instrumentation.Signal`) — using the
`ExcludeFromMigrations` shadow-entity technique for the third consecutive
sector, with Maintenance's own local copies (not a reference to
DigitalTwin's or Instrumentation's Infrastructure projects). Zero
`principalSchema: "Organization"` or `"Security"` FKs exist —
`WorkOrder.AssignedTeamId`/`AssignedPersonId` (→ `OrganizationDb`, a
different physical database per the ADR-021 Option A/B tradeoff) and
every `Security.ApplicationUser` reference are correctly passport-only.

Migration: `20260817002208_InitialMaintenanceSchema`, generated via
`dotnet ef migrations add InitialMaintenanceSchema --project src/Contexts/Maintenance/Nexus1.Maintenance.Infrastructure --startup-project src/Contexts/Maintenance/Nexus1.Maintenance.Infrastructure --output-dir Persistence/Migrations`.

## Application layer — the atlas's own five named verification queries (one honestly adapted) plus real per-table behaviors

Eight operations:

- `GetAssetsByUnitQuery` — atlas C.9.5.2 query 1, **adapted**: exposes
  the raw `EquipmentId` passport int instead of a joined `EquipmentCode`,
  since `ReactorFleet.Equipment` doesn't exist in this codebase (verified
  directly: `ReactorFleetDbContext` only exposes `Unit`/
  `UnitPowerSnapshot`). Documented directly in `AssetByUnitDto`'s own doc
  comment, not a silent deviation.
- `GetOpenWorkOrdersByUnitQuery` — atlas query 2, verbatim.
- `GetWorkOrdersWithOriginQuery` — atlas query 3, **adapted**: returns
  `WorkOrderCode`/`Title` plus the raw `OriginOperationalEventId`/
  `OriginIncidentActionId` values, with no join against a local
  `EventManagement` table (none exists). Verified directly: the handler
  performs no join at all against either origin id.
- `GetLatestConditionPerAssetQuery` — atlas query 4, verbatim.
- `GetActiveDegradationCasesQuery` — atlas query 5, verbatim.
- `RecordAssetConditionCommand` — writes one `AssetCondition` plus its
  `AssetConditionMeasurement` rows in one operation.
- `OpenWorkOrderCommand` — `WorkOrder`'s defining behavior.
- `RecordDegradationCommand` — writes one `DegradationRecord` plus its
  initial `DegradationTrendPoint` rows.

14 component tests against real LocalDB, migrating four DbContexts
(`ReactorFleet`, `CorePlatform`, `Instrumentation`, `Maintenance`) into
one throwaway database per test, including a dedicated test proving
`GetActiveDegradationCasesQuery` excludes closed (`IsActive = false`)
records and a dedicated test proving `GetWorkOrdersWithOriginQuery`
returns the raw origin passport ids without attempting any join.

## Composed into Nexus1.ModularRuntime

`AddMaintenanceApplication()`/`AddMaintenanceInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing shared connection string
(confirmed: no `appsettings.json` changes). `maintenance-db` added to the
health check chain, bringing the total to **11** registered checks —
confirmed by `grep` count, not assumed.

`Nexus1.ArchitectureTests`: 7/7, confirmed standalone.

**`.sln` nesting verified directly, both by the implementation pass and
independently afterward**: before adding Maintenance's projects, exactly
one `"Contexts", "Contexts"` solution-folder entry existed
(`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`, re-derived by `grep`, not
assumed from memory). After the edit, still exactly one such entry, and
Maintenance's own new folder (`{5FD7E834-8844-4B48-8A2E-F7F097BD8F45}`)
has a `GlobalSection(NestedProjects)` mapping directly to it — confirmed
independently with the same `grep` used in the prior fix commit. This is
the first sector built since that fix and the first real proof the
corrected discipline holds under a live implementation pass, not just
the repair itself.

**Real host startup, independently re-verified end-to-end**: applied the
migration against the real `AlarmManagementDb`; confirmed via direct
`sqlcmd` that all 16 `Maintenance.*` tables exist (schema count:
`AlarmManagement` 3, `CorePlatform` 11, `DigitalTwin` 20,
`Instrumentation` 15, `Maintenance` 16, `ReactorFleet` 2) and all six
cross-schema FK constraints are live in `sys.foreign_keys`, before ever
asking the health check about it. Built and ran the actual
`Nexus1.ModularRuntime.dll` (RabbitMQ already running from the prior
sector's verification, confirmed via `netstat` before starting), confirmed
`GET /health/ready` returns `200 Healthy` with `maintenance-db` genuinely
present among all 11 checks.

## Owned

- The `EventManagement`-dependency gap (see above) is the first Phase 2
  instance of a dependency on a sector that doesn't exist *at all* yet,
  distinct from the Organization/Instrumentation/DigitalTwin reversal
  notes (those involve Phase 1 contexts that exist but were never wired).
  Recorded in ADR-021 as a natural future step once `EventManagement`
  (atlas C.8) is eventually built.
- The `GetAssetsByUnitQuery` equipment-passport adaptation (verified
  directly by reading `AssetByUnitDto.cs`/`EfAssetsByUnitFinder.cs`) is a
  genuine, documented deviation from the atlas's literal SQL, not a
  silent one.
- No `src/` files outside the new `Nexus1.Maintenance.*` projects and
  `Nexus1.ModularRuntime`'s composition root (csproj, `Program.cs`) were
  touched — confirmed via `git status`. `appsettings.json` was correctly
  left untouched.
- `AlarmManagementDb` gained the `Maintenance` schema alongside
  `ReactorFleet`/`CorePlatform`/`AlarmManagement`/`Instrumentation`/
  `DigitalTwin` — left in place, harmless local dev state, same reasoning
  as every prior step.

## Scope explicitly not covered by this step

Per ADR-021, thirty of the atlas's forty-six Maintenance tables remain
unbuilt, in seven named groups: append-only audit-trail tables
(`AssetStatusHistory`, `WorkOrderStatusHistory` — current state is
already carried on `Asset`/`WorkOrder` themselves); `Inspection`/
`InspectionFinding` (named in C.9.1's bullet list, zero verification-query
consumer); `MaintenanceDocument`/`MaintenanceApproval` (evidence/approval
group); the entire inventory/spares group (`Supplier`, `SupplierPart`,
`SparePart`, `SparePartStock`, `AssetSparePart` — also the group with the
most tangled cross-database pull, `SparePartStock.PlantId` →
`Organization.Plant`); the entire planning/scheduling group
(`MaintenancePlan`, `MaintenancePlanTask`, `MaintenanceSchedule`,
`MaintenanceScheduleOccurrence`, `MaintenanceWindow`); the work-order
substructure (`WorkOrderTask`, `WorkOrderLabour`, `WorkOrderMaterial`,
`WorkOrderEventLink` — the last excluded outright since its entire
purpose requires the not-yet-built `EventManagement`); plus the nine
lookups backing only those groups. None are silently dropped — each is
named in ADR-021 with the specific reason it was deferred.

This closes Maintenance, sector 6 of 11 in Phase 2. EventManagement is
next per CLAUDE.md §9's ordering. Awaiting the next checkpoint
instruction.
