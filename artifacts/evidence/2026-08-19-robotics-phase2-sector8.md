# Evidence: Robotics (Phase 2, sector 8 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.x, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-023-robotics-phase2-scope-and-persistence.md`. This report
is the real proof: fifteen of thirty-eight atlas tables modeled in Domain
(the atlas's own four named verification queries plus FK-integrity
closure), EF Core persistence sharing `AlarmManagementDb` with two real
cross-context foreign keys into `ReactorFleet.Unit`, composed into
`Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still green with
`Nexus1.Robotics.*` auto-classified with zero test edits needed, and a
correctly-nested `.sln` solution folder — verified in the order the
architect specified this time: **build → test → real host → health
check → this report → commit**, not evidence-then-verify.

## Automated regression: 721/721 passing (was 665/665 before this step)

Independently re-run from scratch, serially (`-m:1`, avoiding the
interleaved-console-output failure mode that scrambled EventManagement's
first attempt at this table):

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
Nexus1.Robotics.UnitTests                         48/48 passed  (new)
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
Nexus1.Robotics.ComponentTests                     8/8  passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

`Nexus1.Contracts.ContractTests` and `Nexus1.DistributedSlice.EndToEndTests`
remain pre-existing "no tests" placeholder projects, confirmed by reading
their raw VSTest output directly, not assumed. 56 new tests this sector
(48 + 8), 665 → 721, arithmetic independently re-added from the raw
per-project numbers above, not carried over from the implementation
agent's own count.

Full solution build: **0 warnings, 0 errors**
(`dotnet build Nexus1.Runtime.sln -m:1`, all 82 projects incl. the five
new Robotics projects) — run independently after the implementation
agent finished, not taken from its own report.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.12** (confirmed via the real
  `"C.12.1 Sector purpose"` header at its real line number, not the
  garbled table of contents — the fourth consecutive sector this
  discipline has been applied to). Read in full: C.12.1 (sector purpose
  and design choice), C.12.2 (full 38-table list across six named
  groups), C.12.4.2 (all 15 lookup DDL), C.12.4.3–C.12.4.7 (full
  substantive DDL for all 23 tables, every column/constraint/FK read
  directly), C.12.5.2 (all four verification queries, verbatim), C.12.7
  (FK mapping cross-check), C.12.8 (boundary and next-sector note).
- `From_Domain_to_Twin`'s Supporting-domain table re-read directly and
  quoted verbatim in ADR-023, not recalled from memory: *"Represents
  missions and inspections that may produce evidence... Is the robot
  output evidence or action?"*
- **Whole-sector FK audit performed first, across the full 38-table
  graph, before any scope trim** — the specific check the architect asked
  for by name. Every one of Robotics' six named external contexts
  (`ReactorFleet`, `CorePlatform`, `Security`, `Instrumentation`,
  `Maintenance`, `EventManagement`) confirmed to already exist in this
  codebase. Zero whole-sector gaps — the first Phase 2 sector with a
  clean result on this specific check.
- `Nexus1.ReactorFleet.Domain`/`Nexus1.ReactorFleet.Infrastructure`
  checked directly (not assumed): only `Unit`/`UnitPowerSnapshot` exist;
  `Equipment` and `EquipmentLocation` do not. Confirmed the individual-
  table gap named in ADR-023 (affecting only out-of-scope tables in this
  pass's chosen 15-table cut).
- `ReactorFleetUnitReference.cs` (EventManagement's copy) read directly
  before writing Robotics' own local copy of the shadow entity.
- `.sln` "Contexts" folder grep run both before and after (see below).

## Scope: fifteen of thirty-eight tables, verification-query-driven

Per ADR-023: `RobotType`, `RobotStatus`, `BatteryStatus`,
`CommunicationStatus`, `MissionType`, `MissionStatus`, `MissionPriority`,
`ReadinessStatus` (lookups); `RobotModel`, `Robot`, `RobotHealthSnapshot`,
`Mission`, `MissionEvent`, `MissionReadinessAssessment`,
`MissionReadinessItem` (substantive). Two real cross-context foreign keys
(`Robot.HomeUnitId`, `Mission.UnitId` → `ReactorFleet.Unit`, via a local
`ReactorFleetUnitReference` shadow entity); all `Security.ApplicationUser`
references (`Mission.RequestedByUserId`/`ApprovedByUserId`,
`MissionEvent.RecordedByUserId`,
`MissionReadinessAssessment.AssessedByUserId`) stay passport-only, no
enforced constraint — `SecurityDb` is a separate physical database, same
downgrade every prior sector's Security references has needed.
`Robot.HomeDockingStationId` and `MissionReadinessItem.
MissionChecklistItemId` are omitted from the Domain model entirely (not
even as passport columns) since their target tables are out of this
pass's scope, per ADR-023's explicit reasoning.

## Genuine discrepancies and judgment calls found while building

1. **`EfLatestHealthSnapshotFinder` query didn't translate to SQL on the
   first attempt** — a real defect the component tests caught, not a
   rule/handler-only test artifact. The original LINQ used `GroupBy` +
   `OrderByDescending().First()` + a `Join`, which EF Core rejected at
   runtime with `ProjectionBindingExpression could not be translated`.
   Fixed by switching to a correlated-subquery pattern (`let` +
   `OrderByDescending().FirstOrDefault()`), which translates to
   `OUTER APPLY` — matches the shape the atlas's own query 2 uses.
   Independently re-verified: `Nexus1.Robotics.ComponentTests` (real
   LocalDB, real migrations, no mocks) is green including this handler,
   confirming the fix is real, not just compiling.
2. **Lookup audit-column shadow properties**: Robotics' 15 lookups map
   `ModifiedAtUtc`/`RowVersion` as EF-only shadow properties (not
   Domain-modeled), matching the real C.12.4.2 DDL (which declares both
   columns on every lookup, unlike some prior sectors' lookup DDL). This
   is a genuine atlas-content difference between sectors, not an
   inconsistency with EventManagement's own lookup treatment — confirmed
   by reading the real `RobotType` DDL directly rather than copying
   EventManagement's `EventTypeConfiguration` shape by rote.
3. **`.sln` nesting for `UnitTests`/`ComponentTests` projects**: these
   nest under the shared `tests` solution folder GUID
   (`{DFD64979-71D4-46B5-BF62-217FA110CF39}`), not under Robotics' own
   folder GUID — confirmed by reading EventManagement's real `.sln`
   entries directly (`{A59D166E...}`/`{90666BF1...}` both map to
   `{DFD64979...}`, not to `{31566439...}`) rather than assuming a
   uniform "everything nests under the sector folder" rule. Robotics'
   own `UnitTests`/`ComponentTests` GUIDs
   (`{E8CAF97F...}`/`{4D9C0817...}`) map to the same shared `{DFD64979...}`
   folder, matching the real precedent exactly.
4. **No CHECK-constraint discrepancies** beyond what ADR-023 already
   named — `Mission`'s two date-range CHECKs
   (`PlannedEndUtc >= PlannedStartUtc`, `ActualEndUtc >= ActualStartUtc`)
   and the percentage-range CHECKs on `RobotHealthSnapshot` match the
   real DDL exactly.

## `dotnet ef migrations add`

```
dotnet ef migrations add InitialRoboticsSchema \
  --project src/Contexts/Robotics/Nexus1.Robotics.Infrastructure \
  --startup-project src/Contexts/Robotics/Nexus1.Robotics.Infrastructure \
  --output-dir Persistence/Migrations
```

Landed at
`src/Contexts/Robotics/Nexus1.Robotics.Infrastructure/Persistence/Migrations/20260817063009_InitialRoboticsSchema.cs`
(+ `.Designer.cs`, `RoboticsDbContextModelSnapshot.cs`). Reviewed:
readable table/column/constraint names throughout (`PK_Robotics_*`,
`FK_Robotics_*`, `UQ_Robotics_*`), `Restrict` on every real FK, no
`CreateTable` emitted for the `ReactorFleetUnitReference` shadow entity
(`ExcludeFromMigrations` worked as intended).

## Real host startup — verified before this report was written, in the order specified

Applied the migration (`dotnet ef database update`) against the real
`AlarmManagementDb`. Confirmed via direct `sqlcmd`:

- All 15 `Robotics.*` tables exist (`INFORMATION_SCHEMA.TABLES`, schema =
  `Robotics`) — exact match to ADR-023's named scope, no more, no fewer.
- `__EFMigrationsHistory_Robotics` contains exactly one row,
  `20260817063009_InitialRoboticsSchema`.
- Both real cross-context `FOREIGN KEY` constraints are live in
  `sys.foreign_keys`: `FK_Robotics_Robot_Unit` (`Robot.HomeUnitId` →
  `ReactorFleet.Unit.UnitId`) and `FK_Robotics_Mission_Unit`
  (`Mission.UnitId` → `ReactorFleet.Unit.UnitId`) — the only two
  cross-context FKs this sector's trimmed scope has, both present and
  correctly targeted. 17 total FKs under the `Robotics` schema (2
  cross-context + 15 internal lookup/aggregate FKs).

Then built and started the actual `Nexus1.ModularRuntime.dll` (Erlang/
RabbitMQ process confirmed already running before starting), confirmed
`GET /health/ready` returns `200 Healthy`; the host log shows the
`robotics-db` health check's own migration-history query
(`SELECT OBJECT_ID(N'[__EFMigrationsHistory_Robotics]')` /
`... FROM [__EFMigrationsHistory_Robotics]`) executed successfully as
part of that response — the strengthened `DbContextHealthCheck<T>`
(ADR-018) genuinely evaluating Robotics' own migration state, not merely
`CanConnectAsync()`. Host stopped cleanly afterward
(`Stop-Process` on the PID bound to port 5101; confirmed by a follow-up
`curl` timing out with "Failed to connect").

## `.sln` "Contexts" folder nesting — before and after

Before (confirmed independently, not taken from the implementation
agent's own claim): exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`.

```
grep -n 'Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Contexts", "Contexts"' Nexus1.Runtime.sln
16:Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Contexts", "Contexts", "{981F0668-8CE2-4D0B-8A12-6A04D22318AC}"
```

After adding the new `Robotics` solution folder
(`{8DF2C27E-59D7-4F91-BD88-1316A2A3F15E}`) and five new project entries:
still exactly one match, same GUID. `Robotics`'s own folder maps directly
to it (`{8DF2C27E...} = {981F0668...}`); `Nexus1.Robotics.Domain`/
`.Application`/`.Infrastructure` nest under the Robotics folder;
`Nexus1.Robotics.UnitTests`/`.ComponentTests` nest under the shared
`tests` folder, matching every prior sector's real (not assumed)
precedent.

## What was NOT touched

`src/Contexts/EventManagement/`, `src/Contexts/Maintenance/`, and their
test projects — confirmed via `git status --short` before writing this
report: only `Nexus1.Runtime.sln`, `Nexus1.ModularRuntime.csproj`,
`Program.cs`, `docs/adr/ADR-023-...`, and the new `Robotics`
source/test trees appear. `RobotMaintenanceLink` and
`MissionOperationalEventLink` — the two link tables into `Maintenance`
and `EventManagement` that the whole-sector FK audit confirmed are
buildable — are deliberately not implemented this pass, per ADR-023's
explicit "buildable, not verification-query-justified yet" reasoning,
recorded there rather than silently dropped.

## Composition into `Nexus1.ModularRuntime`

`AddRoboticsApplication()`/`AddRoboticsInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing `alarmManagementConnectionString`
variable — no new connection string, no `appsettings.json` change.
`.AddCheck<DbContextHealthCheck<RoboticsDbContext>>("robotics-db")` added
to the health-check chain. `Nexus1.ModularRuntime` builds clean with all
eight contexts composed (`ReactorFleet`, `CorePlatform`, `AlarmManagement`,
`Instrumentation`, `DigitalTwin`, `Maintenance`, `EventManagement`,
`Robotics`) sharing `AlarmManagementDb`, plus `Security`/`Organization`/
`Audit`/`Compliance`/`Reporting` on their own physical databases.
