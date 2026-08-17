# Evidence: EmergencyPreparedness (Phase 2, sector 10 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.x, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-025-emergencypreparedness-phase2-scope-and-persistence.md`.
This report is the real proof: seventeen of forty-two atlas tables
modeled in Domain (the atlas's own four named verification queries plus
FK-integrity closure), EF Core persistence sharing `AlarmManagementDb`
with three real cross-context foreign keys — including the first shadow
entity in this codebase to target a table built within this same Phase 2
sequence (`RadiationMonitoring.RadiationZone`, sector 9) rather than a V1
or early-Phase-2 context — composed into `Nexus1.ModularRuntime`,
`Nexus1.ArchitectureTests` still green, and a correctly-nested `.sln`
solution folder — verified in the order the architect specified: **build
→ test → real host → health check → this report → commit**.

## Automated regression: 828/828 passing (was 782/782 before this step)

Independently re-run from scratch, serially (`-m:1`), not taken from the
implementation agent's own self-reported run:

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
Nexus1.Maintenance.UnitTests                      47/47 passed
Nexus1.EventManagement.UnitTests                  47/47 passed
Nexus1.Robotics.UnitTests                         48/48 passed
Nexus1.RadiationMonitoring.UnitTests              53/53 passed
Nexus1.EmergencyPreparedness.UnitTests            38/38 passed  (new)
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
Nexus1.Maintenance.ComponentTests                 14/14 passed
Nexus1.EventManagement.ComponentTests             15/15 passed
Nexus1.Robotics.ComponentTests                     8/8  passed
Nexus1.RadiationMonitoring.ComponentTests          8/8  passed
Nexus1.EmergencyPreparedness.ComponentTests        8/8  passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

46 new tests this sector (38 + 8), 782 → 828, independently re-added from
the raw per-project numbers above. Full solution build:
**0 warnings, 0 errors** (`dotnet build Nexus1.Runtime.sln -m:1`, all 92
projects incl. the five new EmergencyPreparedness projects) —
independently re-run after the implementation agent finished, not taken
from its own report.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.14** (confirmed via the real
  `"C.14.1 Purpose and boundary"` header at its real line number, not the
  garbled table of contents — the fifth consecutive sector this
  discipline has been applied to). Read in full: C.14.1 (purpose,
  boundary, backbone position naming all eight dependency contexts),
  C.14.2 (full 42-table list), C.14.3 (all 18 lookup categories),
  C.14.4.2 (lookup DDL, confirmed uniform shape), C.14.4.3 (full
  substantive DDL for `EmergencyPlan`/`EmergencyPlanRevision`/`Exercise`/
  `ExerciseInject`/`ExerciseObservation`/`AssemblyPoint`/
  `EvacuationRoute`/`EvacuationRouteZone`/`EmergencyResource`/
  `ResourceReadinessCheck`, every column/constraint/FK read directly),
  C.14.6 (FK passport map cross-check), C.14.7.3 (all four verification
  queries, verbatim), C.14.8 (honest boundary).
- `From_Domain_to_Twin` re-read directly: unlike RadiationMonitoring/
  EventManagement, this sector **does** carry its own dedicated
  Supporting-domain table row, recovered from the source's own staggered
  PDF-extraction layout (row labels and text cells offset by one
  position — the same class of garbling this session has hit repeatedly)
  rather than assumed absent: *"Connects scenarios, plans, and exercises
  to operational readiness."* / *"Which procedure or exercise is being
  tested?"*
- **Whole-sector FK audit performed first, across the full 42-table
  graph, before any scope trim** — the specific check the architect
  keeps asking for by name. All eight of EmergencyPreparedness's named
  external contexts (`CorePlatform`, `Security`, `Organization`,
  `ReactorFleet`, `AlarmManagement`, `EventManagement`,
  `RadiationMonitoring`, `Robotics`) confirmed to already exist. Zero
  whole-sector gaps — the **third** consecutive Phase 2 sector with a
  clean result on this check (after Robotics and RadiationMonitoring),
  and the **first** sector with zero individual-table gaps anywhere in
  its full FK graph either (every prior sector through
  RadiationMonitoring hit at least one absent `ReactorFleet.Equipment`/
  `EquipmentLocation` target — this sector's own graph simply never
  names either table).
- `Organization.Site`/`Team` and `CorePlatform.Language` (all newly
  relevant external targets) confirmed to exist by reading
  `Nexus1.Organization.Domain`/`Nexus1.CorePlatform.Domain` directly, not
  assumed from the sector count alone.

## Scope: seventeen of forty-two tables, verification-query-driven

Per ADR-025: `PlanStatus`, `RouteStatus`, `ResourceType`,
`ReadinessStatus`, `ExerciseType`, `ExerciseStatus`,
`ObservationSeverity`, `ResourceStatus` (lookups); `EmergencyPlan`,
`EmergencyPlanRevision`, `Exercise`, `ExerciseObservation`,
`EvacuationRoute`, `EvacuationRouteZone`, `AssemblyPoint`,
`EmergencyResource`, `ResourceReadinessCheck` (substantive). Three real
cross-context foreign keys: `EmergencyResource.EngineeringUnitId` →
`CorePlatform.EngineeringUnit` (via the established
`CorePlatformEngineeringUnitReference` shadow entity), and
`AssemblyPoint.RadiationZoneId`/`EvacuationRouteZone.RadiationZoneId` →
`RadiationMonitoring.RadiationZone` (via a newly-built
`RadiationMonitoringRadiationZoneReference` shadow entity — same
technique, new target). No `ReactorFleetUnitReference` needed this
sector — the first plant-operational sector where none of the in-scope
tables carries a `UnitId` column at all.

## Genuine discrepancies and judgment calls found while building

1. **`EfResourceReadinessDashboardFinder` query didn't translate to SQL on
   the first attempt** — a real defect the component tests caught, the
   same class of failure Robotics' `EfLatestHealthSnapshotFinder` hit:
   the natural LINQ shape (correlated-subquery `let` combined with a
   `LEFT JOIN` whose own key was that correlated subquery's result)
   threw `InvalidOperationException: could not be translated`. Fixed by
   nesting the `ReadinessStatus` code lookup as a scalar subquery instead
   of a `LEFT JOIN`. Independently re-verified:
   `Nexus1.EmergencyPreparedness.ComponentTests` (real LocalDB, real
   migrations, no mocks) is green including this handler, confirming the
   fix is real, not just compiling. This is now the third instance of
   this exact "LINQ shape that reads correctly but doesn't translate"
   failure mode across consecutive sectors (Robotics, then this one) —
   worth flagging as a pattern for the eventual M10-style hardening pass,
   though not urgent enough to act on now.
2. **`SiteId`-passport-only DTO adaptation** (queries 1 and 4) —
   `Organization.Site` is passport-only in this codebase (`OrganizationDb`
   is a separate physical database), so `ActiveEmergencyPlanDto`/
   `ResourceReadinessDashboardDto` project the plain `int SiteId` rather
   than a site code, each with a doc comment explaining the substitution.
   Verified directly by reading both DTOs and their finders — the same
   class of adaptation `RadiationMonitoring`'s `OpenDoseAlertDto` made for
   `Organization.Person` in the immediately prior sector, now recurring
   for a second passport-only Organization target.
3. **`RadiationMonitoringRadiationZoneReference`** — verified directly
   against the real `RadiationZoneConfiguration.cs`: table
   `RadiationMonitoring.RadiationZone`, key column `RadiationZoneId`
   (int), `Code` NVARCHAR(80), `ExcludeFromMigrations()` applied
   correctly. No `CreateTable` emitted for it in the generated migration.
4. **No CHECK-constraint discrepancies** beyond what ADR-025 already
   named — `EmergencyPlan`'s effective-window CHECK, `Exercise`'s
   scheduled/actual-window CHECKs, and `AssemblyPoint`'s max-occupancy
   CHECK all match the real DDL exactly.

## `dotnet ef migrations add`

```
dotnet ef migrations add InitialEmergencyPreparednessSchema \
  --project src/Contexts/EmergencyPreparedness/Nexus1.EmergencyPreparedness.Infrastructure \
  --startup-project src/Contexts/EmergencyPreparedness/Nexus1.EmergencyPreparedness.Infrastructure \
  --output-dir Persistence/Migrations
```

Landed at
`src/Contexts/EmergencyPreparedness/Nexus1.EmergencyPreparedness.Infrastructure/Persistence/Migrations/20260817120306_InitialEmergencyPreparednessSchema.cs`
(+ `.Designer.cs`, `EmergencyPreparednessDbContextModelSnapshot.cs`).
Reviewed: readable table/column/constraint names throughout
(`PK_EmergencyPreparedness_*`), the three real FK constraints use the
exact names ADR-025 specified (`FK_AssemblyPoint_RadiationZone`,
`FK_EmergencyResource_EngineeringUnit`,
`FK_EvacuationRouteZone_RadiationZone`), `Restrict` on every real FK, no
`CreateTable` emitted for either shadow entity.

## Real host startup — verified in the order specified, no interruption this time

Applied the migration (`dotnet ef database update`) against the real
`AlarmManagementDb`. Confirmed via direct `sqlcmd`:

- All 17 `EmergencyPreparedness.*` tables exist
  (`INFORMATION_SCHEMA.TABLES`, schema = `EmergencyPreparedness`) —
  exact match to ADR-025's named scope.
- `__EFMigrationsHistory_EmergencyPreparedness` contains exactly one row,
  `20260817120306_InitialEmergencyPreparednessSchema`.
- All three real cross-context `FOREIGN KEY` constraints are live in
  `sys.foreign_keys`: `FK_AssemblyPoint_RadiationZone`,
  `FK_EvacuationRouteZone_RadiationZone` (both →
  `RadiationMonitoring.RadiationZone`), `FK_EmergencyResource_
  EngineeringUnit` (→ `CorePlatform.EngineeringUnit`) — every one
  ADR-025 named, present and correctly targeted. 17 total FKs under the
  `EmergencyPreparedness` schema (3 cross-context + 14 internal
  lookup/aggregate FKs).

Before starting the host, confirmed preconditions directly rather than
assuming them from the prior sector's resolution: Erlang/RabbitMQ
process still running (`Get-Process erl*`), all six shared/owned
databases `ONLINE` (`AuditDb`/`ReportingDb`/`SecurityDb` included —
re-checked given the prior sector's `RECOVERY_PENDING` incident), 2.47 GB
free system memory. Built and started the actual
`Nexus1.ModularRuntime.dll`; `GET /health/ready` returned `200 Healthy`
on the first attempt, with zero `Unhealthy` lines anywhere in the
startup log — a clean run, unlike the prior sector's three attempts.
Host log confirms the `emergencypreparedness-db` health check's own
migration-history query (`SELECT OBJECT_ID(N'[__EFMigrationsHistory_
EmergencyPreparedness]')` / `... FROM [__EFMigrationsHistory_
EmergencyPreparedness]`) executed successfully. Host stopped cleanly
afterward (`Stop-Process` on the PID bound to port 5101).

## `.sln` "Contexts" folder nesting — before and after

Before (confirmed independently): exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`.

After adding the new `EmergencyPreparedness` solution folder
(`{7851F6C0-CAC6-41CE-8ACF-92FC57F59F6A}`) and five new project entries:
still exactly one match, same GUID. `EmergencyPreparedness`'s own folder
maps directly to it; `Nexus1.EmergencyPreparedness.Domain`/`.Application`/
`.Infrastructure` nest under the EmergencyPreparedness folder;
`Nexus1.EmergencyPreparedness.UnitTests`/`.ComponentTests` nest under the
shared `tests` folder (`{DFD64979-71D4-46B5-BF62-217FA110CF39}`),
matching every prior sector's real (verified, not assumed) precedent.

## What was NOT touched

`src/Contexts/RadiationMonitoring/`, `src/Contexts/Robotics/`, and their
test projects — confirmed via `git status --short` before writing this
report: only `Nexus1.Runtime.sln`, `Nexus1.ModularRuntime.csproj`,
`Program.cs`, `docs/adr/ADR-025-...`, and the new
`EmergencyPreparedness` source/test trees appear.
`PlanIncidentLink`/`PlanAlarmLink`/`PlanRobotMissionLink` — the three
link tables into `EventManagement`, `AlarmManagement`, and `Robotics`
that the whole-sector FK audit confirmed are buildable — are
deliberately not implemented this pass, per ADR-025's "buildable, not
verification-query-justified yet" reasoning, recorded there rather than
silently dropped.

## Composition into `Nexus1.ModularRuntime`

`AddEmergencyPreparednessApplication()`/
`AddEmergencyPreparednessInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing
`alarmManagementConnectionString` variable — no new connection string, no
`appsettings.json` change.
`.AddCheck<DbContextHealthCheck<EmergencyPreparednessDbContext>>
("emergencypreparedness-db")` added to the health-check chain.
`Nexus1.ModularRuntime` builds clean with all ten plant-operational
contexts composed (`ReactorFleet`, `CorePlatform`, `AlarmManagement`,
`Instrumentation`, `DigitalTwin`, `Maintenance`, `EventManagement`,
`Robotics`, `RadiationMonitoring`, `EmergencyPreparedness`) sharing
`AlarmManagementDb`, plus `Security`/`Organization`/`Audit`/
`Compliance`/`Reporting` on their own physical databases. This leaves
only **ReinforcementLearning** remaining in Phase 2.
