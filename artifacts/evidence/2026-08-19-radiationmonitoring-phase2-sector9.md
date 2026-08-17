# Evidence: RadiationMonitoring (Phase 2, sector 9 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.x, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-024-radiationmonitoring-phase2-scope-and-persistence.md`.
This report is the real proof: twenty of forty-six atlas tables modeled in
Domain (the atlas's own four named verification queries plus FK-integrity
closure), EF Core persistence sharing `AlarmManagementDb` with five real
cross-context foreign keys — the first sector needing both the
`ReactorFleetUnitReference` and `CorePlatformEngineeringUnitReference`
shadow-entity families together — composed into `Nexus1.ModularRuntime`,
`Nexus1.ArchitectureTests` still green with `Nexus1.RadiationMonitoring.*`
auto-classified with zero test edits needed, and a correctly-nested `.sln`
solution folder — verified in the order the architect specified:
**build → test → real host → health check → this report → commit**.

## A genuine interruption during implementation, recorded plainly

The implementation agent hit a session/API limit partway through (right
after finishing the Application layer, before the Infrastructure csproj,
migration, tests, and composition). Rather than assume the work was lost
or silently re-run everything from scratch, it was resumed from its own
transcript with an explicit checklist of what the filesystem already
showed as done vs. still missing. It finished the remaining work in the
background; a subsequent harness restart (unrelated white-screen crash)
raised the same "was this actually finished?" question a second time.
Both times, the answer was determined by direct inspection — `git status`,
a full independent rebuild, and a full independent serial test run —
never assumed either way. Both times the work was genuinely complete.
This is the same crash-recovery discipline DigitalTwin's session used.

## A second genuine interruption: real-host verification blocked by actual database corruption, not a code defect

The first three real-host attempts returned `503 Unhealthy` — but the log
showed **every context's health check failing, not just
`radiationmonitoring-db`** (`security-db`, `maintenance-db`,
`coreplatform-db`, `robotics-db`, all of them). Root-caused, not assumed:
a severe system memory shortage during the heavy build/test cycle
(machine has 7.87 GB total RAM; free memory dropped to ~0.9 GB) caused
`AuditDb`, `ReportingDb`, and `SecurityDb` to end up in SQL Server's
`RECOVERY_PENDING` state — confirmed directly via
`SELECT name, state_desc FROM sys.databases`, not inferred from the
health-check message alone. This is real database corruption from the
earlier memory pressure, unrelated to RadiationMonitoring's own schema or
code. Restarting the LocalDB instance (`sqllocaldb stop`/`start`)
triggered SQL Server's own crash recovery and brought all three databases
back `ONLINE`, confirmed by the same query before retrying the host.
Reported to the architect transparently at the point of being genuinely
blocked, rather than either fabricating a "Healthy" result or silently
retrying indefinitely; the architect freed additional system memory
(closing Visual Studio) before the successful retry.

## Automated regression: 782/782 passing (was 721/721 before this step)

Independently re-run from scratch, serially (`-m:1`):

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
Nexus1.RadiationMonitoring.UnitTests              53/53 passed  (new)
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
Nexus1.RadiationMonitoring.ComponentTests          8/8  passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

61 new tests this sector (53 + 8), 721 → 782, independently re-added from
the raw per-project numbers above. Full solution build:
**0 warnings, 0 errors** (`dotnet build Nexus1.Runtime.sln -m:1`, all 87
projects incl. the five new RadiationMonitoring projects).

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.13** (confirmed via the real
  `"C.13.1 Sector purpose"` header at its real line number, not the
  garbled table of contents). Read in full: C.13.1 (sector purpose and
  design choice), C.13.2 (full 46-table list across eight named groups),
  C.13.4.2 (all 12 lookup DDL), C.13.4.3–C.13.4.5 (full substantive DDL
  for zones, monitors, dosimetry — every column/constraint/FK read
  directly), C.13.5.2 (all four verification queries, verbatim), C.13.7
  (FK mapping cross-check), C.13.8 (boundary and next-sector note).
- `From_Domain_to_Twin` re-checked directly: RadiationMonitoring appears
  only in the Supporting-domain intro prose, not as its own table row —
  same situation EventManagement's ADR-022 found for itself. No
  dedicated classification to lean on; scope derived from the atlas's
  own signals, per that established precedent.
- **Whole-sector FK audit performed first, across the full 46-table
  graph, before any scope trim.** Every one of RadiationMonitoring's
  eight named external contexts (`ReactorFleet`, `CorePlatform`,
  `Instrumentation`, `Security`, `Organization`, `EventManagement`,
  `Robotics`) confirmed to already exist. Zero whole-sector gaps — the
  second consecutive Phase 2 sector with a clean result on this specific
  check (after Robotics).
- `Nexus1.ReactorFleet.Domain` re-checked directly: only
  `Unit`/`UnitPowerSnapshot` exist; `Equipment`/`EquipmentLocation` do
  not. Confirmed the individual-table gaps ADR-024 named
  (`RadiationZone.EquipmentLocationId`, `RadiationMonitor.EquipmentId`).
- `CorePlatformEngineeringUnitReference.cs` (Instrumentation's copy) and
  `ReactorFleetUnitReference.cs` (Robotics' copy) both read directly
  before writing RadiationMonitoring's own local copies of each.

## Scope: twenty of forty-six tables, verification-query-driven

Per ADR-024: twelve lookups (`RadiationZoneType`, `RadiationZoneStatus`,
`RadiationAreaClassification`, `MonitorType`, `MonitorStatus`,
`MeasurementType`, `MeasurementQuality`, `DoseType`, `DosimeterType`,
`DosimeterStatus`, `LimitType`, `AlertStatus`); eight substantive
(`RadiationZone`, `RadiationMonitor`, `RadiationReading`, `Dosimeter`,
`PersonDosimeterAssignment`, `PersonDoseReading`, `DoseLimit`,
`DoseAlert`). Five real cross-context foreign keys:
`RadiationZone.UnitId`/`RadiationMonitor.UnitId` → `ReactorFleet.Unit`;
`RadiationReading.EngineeringUnitId`/`DoseLimit.EngineeringUnitId`/
`PersonDoseReading.EngineeringUnitId` → `CorePlatform.EngineeringUnit`.
`RadiationZone.EquipmentLocationId` and `RadiationMonitor.EquipmentId`
are passport-only (target tables don't exist in this codebase).
`PersonDosimeterAssignment.PersonId` (`Organization.Person`) and
`DoseAlert.AcknowledgedByUserId`/`PersonDosimeterAssignment.
AssignedByUserId` (`Security.ApplicationUser`) stay passport-only, no
enforced constraint — `OrganizationDb`/`SecurityDb` are separate physical
databases, the same downgrade every prior sector's Organization/Security
references has needed.

## Genuine discrepancies and judgment calls found while building

1. **`OpenDoseAlertDto`'s `Organization.Person` adaptation** — the atlas's
   own query 4 projects `Organization.Person.DisplayName` directly
   (valid in its single-database reference design). Since this codebase
   treats `Organization.Person` as passport-only (`OrganizationDb` is a
   separate physical database from this sector's `AlarmManagementDb`
   home), the DTO projects `PersonId` (a plain `int?`) instead of a
   display name, with a doc comment explaining the substitution.
   Verified directly by reading `OpenDoseAlertDto.cs` and
   `EfOpenDoseAlertsFinder.cs` — a real, necessary deviation from the
   atlas's literal query text, not an oversight.
2. **`GetMonitorsWithCalibrationDueQuery`'s "now" comparison** uses this
   codebase's existing `IDateTimeProvider` abstraction
   (`src/BuildingBlocks/Nexus1.BuildingBlocks.Application/
   IDateTimeProvider.cs`), not a direct `DateTime.UtcNow` call — verified
   directly in `EfMonitorsWithCalibrationDueFinder.cs`, matching this
   project's deterministic-clock discipline (CLAUDE.md discipline 7).
3. **Two consecutive genuine interruptions**, both resolved by direct
   inspection rather than assumption (see above) — the session-limit cut
   during implementation, and the `RECOVERY_PENDING` database state
   blocking real-host verification. Neither reflects a defect in
   RadiationMonitoring's own code; both are recorded here for the
   historical trail.
4. **No CHECK-constraint discrepancies** beyond what ADR-024 already
   named — `PersonDosimeterAssignment`'s `ReturnedAtUtc > AssignedAtUtc`
   and `DoseLimit`'s value/period CHECKs match the real DDL exactly.

## `dotnet ef migrations add`

```
dotnet ef migrations add InitialRadiationMonitoringSchema \
  --project src/Contexts/RadiationMonitoring/Nexus1.RadiationMonitoring.Infrastructure \
  --startup-project src/Contexts/RadiationMonitoring/Nexus1.RadiationMonitoring.Infrastructure \
  --output-dir Persistence/Migrations
```

Landed at
`src/Contexts/RadiationMonitoring/Nexus1.RadiationMonitoring.Infrastructure/Persistence/Migrations/20260817102432_InitialRadiationMonitoringSchema.cs`
(+ `.Designer.cs`, `RadiationMonitoringDbContextModelSnapshot.cs`).
Reviewed: readable table/column/constraint names throughout
(`PK_RadiationMonitoring_*`), the five real FK constraints use the exact
names ADR-024 specified (`FK_RadiationZone_Unit`,
`FK_RadiationMonitor_Unit`, `FK_RadiationReading_EngineeringUnit`,
`FK_DoseLimit_EngineeringUnit`, `FK_PersonDoseReading_EngineeringUnit`),
`Restrict` on every real FK, no `CreateTable` emitted for either shadow
entity (`ExcludeFromMigrations` worked as intended for both families in
the same context, confirmed as the first sector to combine them).

## Real host startup — verified after resolving the RECOVERY_PENDING blocker, in the order specified

Applied the migration (`dotnet ef database update`) against the real
`AlarmManagementDb`. Confirmed via direct `sqlcmd`:

- All 20 `RadiationMonitoring.*` tables exist
  (`INFORMATION_SCHEMA.TABLES`, schema = `RadiationMonitoring`) — exact
  match to ADR-024's named scope.
- `__EFMigrationsHistory_RadiationMonitoring` contains exactly one row,
  `20260817102432_InitialRadiationMonitoringSchema`.
- All five real cross-context `FOREIGN KEY` constraints are live in
  `sys.foreign_keys`: `FK_RadiationZone_Unit`, `FK_RadiationMonitor_Unit`
  (→ `ReactorFleet.Unit`), `FK_RadiationReading_EngineeringUnit`,
  `FK_DoseLimit_EngineeringUnit`, `FK_PersonDoseReading_EngineeringUnit`
  (→ `CorePlatform.EngineeringUnit`) — every one ADR-024 named, present
  and correctly targeted. 25 total FKs under the `RadiationMonitoring`
  schema (5 cross-context + 20 internal lookup/aggregate FKs).

Then built and started the actual `Nexus1.ModularRuntime.dll` (Erlang/
RabbitMQ restarted per the runbook, confirmed via `rabbitmqctl status`
before starting). After resolving the `RECOVERY_PENDING` blocker (see
above) and the architect freeing additional system memory, `GET
/health/ready` returned `200 Healthy` with zero `Unhealthy` log lines
across the entire startup — all thirteen registered checks, including
`radiationmonitoring-db`, passed genuinely, not merely absent from a
truncated log. Host stopped cleanly afterward (`Stop-Process` on the PID
bound to port 5101).

## `.sln` "Contexts" folder nesting — before and after

Before (confirmed independently): exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`.

After adding the new `RadiationMonitoring` solution folder
(`{86FB9D1D-5799-4D5C-A5DF-1A6AA4DD6387}`) and five new project entries:
still exactly one match, same GUID. `RadiationMonitoring`'s own folder
maps directly to it; `Nexus1.RadiationMonitoring.Domain`/`.Application`/
`.Infrastructure` nest under the RadiationMonitoring folder;
`Nexus1.RadiationMonitoring.UnitTests`/`.ComponentTests` nest under the
shared `tests` folder (`{DFD64979-71D4-46B5-BF62-217FA110CF39}`),
matching every prior sector's real (verified, not assumed) precedent.

## What was NOT touched

`src/Contexts/Robotics/`, `src/Contexts/EventManagement/`, and their
test projects — confirmed via `git status --short` before writing this
report: only `Nexus1.Runtime.sln`, `Nexus1.ModularRuntime.csproj`,
`Program.cs`, `docs/adr/ADR-024-...`, and the new
`RadiationMonitoring` source/test trees appear. `RadiationEventLink` and
`RadiationRobotMissionLink` — the two link tables into `EventManagement`
and `Robotics` that the whole-sector FK audit confirmed are buildable —
are deliberately not implemented this pass, per ADR-024's "buildable,
not verification-query-justified yet" reasoning, recorded there rather
than silently dropped.

## Composition into `Nexus1.ModularRuntime`

`AddRadiationMonitoringApplication()`/
`AddRadiationMonitoringInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing
`alarmManagementConnectionString` variable — no new connection string, no
`appsettings.json` change.
`.AddCheck<DbContextHealthCheck<RadiationMonitoringDbContext>>
("radiationmonitoring-db")` added to the health-check chain.
`Nexus1.ModularRuntime` builds clean with all nine plant-operational
contexts composed (`ReactorFleet`, `CorePlatform`, `AlarmManagement`,
`Instrumentation`, `DigitalTwin`, `Maintenance`, `EventManagement`,
`Robotics`, `RadiationMonitoring`) sharing `AlarmManagementDb`, plus
`Security`/`Organization`/`Audit`/`Compliance`/`Reporting` on their own
physical databases.
