# Evidence: EventManagement (Phase 2, sector 7 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.x, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-022-eventmanagement-phase2-scope-and-persistence.md`. This
report is the real proof: fifteen of forty-two atlas tables modeled in
Domain (the atlas's own three named verification queries), EF Core
persistence sharing `AlarmManagementDb` with three real cross-schema
foreign keys (two of them the first-ever shadow references into
`AlarmManagement` in this codebase), composed into `Nexus1.ModularRuntime`,
`Nexus1.ArchitectureTests` still green with `Nexus1.EventManagement.*`
auto-classified with zero test edits needed, and a correctly-nested `.sln`
solution folder.

## Automated regression: 665/665 passing (was 603/603 before this step)

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
Nexus1.EventManagement.UnitTests                  47/47 passed  (new)
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                12/12 passed
Nexus1.RootCause.ComponentTests                    7/7  passed
Nexus1.AlarmManagement.ComponentTests             22/22 passed
Nexus1.Audit.ComponentTests                        3/3  passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   13/13 passed
Nexus1.CorePlatform.ComponentTests                19/19 passed
Nexus1.Security.ComponentTests                     9/9  passed
Nexus1.Organization.ComponentTests                24/24 passed
Nexus1.ServiceDefaults.ComponentTests             14/14 passed
Nexus1.Instrumentation.ComponentTests              3/3  passed  (see Note below)
Nexus1.DigitalTwin.ComponentTests                 16/16 passed
Nexus1.Maintenance.ComponentTests                 14/14 passed
Nexus1.EventManagement.ComponentTests             15/15 passed  (new)
Nexus1.ArchitectureTests                           6/6  passed
Nexus1.Contracts.ContractTests + Nexus1.DistributedSlice.EndToEndTests   16/16 passed
```

Note: per-project counts above are transcribed from a single serial
`dotnet test Nexus1.Runtime.sln --no-build` run whose console output
interleaves assemblies under VSTest; every block reported `Failed: 0`
and the sum of all "Total tests" blocks (665) matches
603 (prior baseline) + 47 (new UnitTests) + 15 (new ComponentTests)
exactly, cross-checked line by line against the raw log.
`Nexus1.EventManagement.UnitTests` (47/47) and
`Nexus1.EventManagement.ComponentTests` (15/15) were also run standalone
beforehand to isolate them from the interleaved output; both zero-failure
runs matched.

Full solution build: 0 warnings, 0 errors
(`dotnet build Nexus1.Runtime.sln`, ~2m30s, all 60 projects incl. the
seven new EventManagement/test projects).

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.8** (confirmed via the real
  `"C.8.1 Sector purpose"` header, read directly at the atlas's real line
  numbers rather than trusted from the file's garbled table of contents).
  Read in full: C.8.2 (full table list), C.8.3 (lookup shape), C.8.4.2
  (lookup DDL for all nine in-scope lookups plus enough neighbors to
  confirm the shape is uniform — `Code`/`Name`/`Description`/
  `DisplayOrder DEFAULT 0`/`IsActive DEFAULT 1`/`CreatedAtUtc`/
  `ModifiedAtUtc`/`RowVersion`), C.8.4.3/C.8.4.4 (full real DDL for
  `OperationalEvent`, `EventAlarmLink`, `EventFloodLink`,
  `EventTimelineEntry`, `Incident`, `IncidentAction` — every column,
  constraint, and FK read directly, not inferred from the table-list
  summary), C.8.5.2 (all three verification queries, verbatim), C.8.7
  (FK mapping cross-check).
- `AlarmEventConfiguration.cs`/`AlarmFloodConfiguration.cs` and
  `AlarmEventId.cs`/`AlarmFloodId.cs` read directly to confirm the real
  table names (`AlarmManagement.AlarmEvent`/`AlarmManagement.AlarmFlood`)
  and key types (`BIGINT`/`long`) before writing the two new shadow
  reference types — matches ADR-022's own stated verification discipline.
- `ReactorFleetUnitReference.cs` (Maintenance's copy) read directly for
  the shadow-entity technique before writing EventManagement's own local
  copy.
- `.sln` "Contexts" folder grep run both before and after (see below).

## Genuine discrepancies found between the atlas DDL and what was built

1. **Cross-physical-database FKs downgraded to passport-only, as
   expected but confirmed against the real DDL, not assumed.** The
   atlas's own single-database DDL declares real `FOREIGN KEY`
   constraints from `OperationalEvent.ReportedByUserId`/`OwnerUserId`,
   `OperationalEvent.PlantId`, `OperationalEvent.OwningPersonId`,
   `EventTimelineEntry.EnteredByUserId`, `Incident.LeadInvestigatorUserId`,
   and `IncidentAction.VerifiedByUserId` against `Security.ApplicationUser`
   / `Organization.Plant` / `Organization.Person`. All six are downgraded
   to plain passport columns with no `HasOne`/FK in this codebase, per
   the now-established convention (SecurityDb/OrganizationDb are
   different physical databases than this sector's AlarmManagementDb
   home) — not a new finding, but confirmed column-by-column against the
   real DDL rather than assumed from the abbreviated FK-mapping summary.
2. **`EventSourceTypeId` is `NOT NULL`** on `OperationalEvent` — easy to
   miss from the abbreviated table-list summary (which only lists
   `UnitId, EventTypeId, EventStatusId` under "FK"), caught by reading
   the real DDL directly, same discipline as Maintenance's
   `AssetCriticality`/`WorkOrderType` catch. ADR-022 already named this;
   confirmed here against the actual `CREATE TABLE` statement.
3. **No CHECK constraints beyond what ADR-022 already named.** The real
   DDL for all six in-scope substantive tables has no `CHECK` constraint
   beyond nullability and the two real named uniques (`EventCode`,
   `Incident.OperationalEventId`, `Incident.IncidentNumber`,
   `(OperationalEventId, AlarmEventId)`,
   `(OperationalEventId, AlarmFloodId)`) — matches what was built exactly.
4. **An implementation-detail discovery, not an atlas discrepancy**:
   EF Core requires a dependent-side FK property's CLR type to match the
   principal key's CLR type once a `HasConversion` value converter is in
   play. The first migration attempt failed with
   `The relationship from 'IncidentAction' to 'Incident' ... cannot
   target the primary key ... because it is not compatible` because
   `EventAlarmLink.OperationalEventId`, `EventFloodLink.OperationalEventId`,
   `EventTimelineEntry.OperationalEventId`, `Incident.OperationalEventId`,
   and `IncidentAction.IncidentId` were initially modeled as plain `long`
   rather than the strongly-typed `OperationalEventId`/`IncidentId`
   structs. Fixed by retyping those five properties (and the finder LINQ
   queries that compared them) — same-context internal FKs are typed to
   match their target's own strongly-typed Id; cross-context FKs into the
   `AlarmManagement` shadow references (whose keys are plain primitives)
   correctly stay plain `long`. No prior sector's own FK shape happened to
   need a same-context typed-FK-to-typed-PK relationship at this depth
   (link tables into two different aggregates simultaneously), so this is
   the first time this particular EF constraint surfaced in this
   codebase — worth naming for the next sector that builds a link table
   between two of its own aggregates.

## `dotnet ef migrations add`

```
dotnet ef migrations add InitialEventManagementSchema \
  --project src/Contexts/EventManagement/Nexus1.EventManagement.Infrastructure \
  --startup-project src/Contexts/EventManagement/Nexus1.EventManagement.Infrastructure \
  --output-dir Persistence/Migrations
```

Landed at
`src/Contexts/EventManagement/Nexus1.EventManagement.Infrastructure/Persistence/Migrations/20260817034016_InitialEventManagementSchema.cs`
(+ `.Designer.cs`, `EventManagementDbContextModelSnapshot.cs`). Reviewed:
readable table/column/constraint names throughout
(`PK_EventManagement_*`, `FK_EventManagement_*`, `UQ_EventManagement_*`),
`Restrict` on every real FK (no `ON DELETE` clause anywhere in the atlas's
own DDL for this sector, matched exactly), the `Incident.OperationalEventId`
unique index present, and no `CreateTable` emitted for any of the three
`ExternalReferences` shadow entities (`ExcludeFromMigrations` worked as
intended). **Not applied** to the real `AlarmManagementDb` — per
instruction, that verification (plus real host startup, a
`curl /health/ready` check, and a direct `sys.foreign_keys` query) is
done independently after this commit.

## `.sln` "Contexts" folder nesting — before and after

Before (per the task's own pre-check, confirmed): exactly one match,
GUID `{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`.

```
grep -n 'Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Contexts", "Contexts"' Nexus1.Runtime.sln
16:Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Contexts", "Contexts", "{981F0668-8CE2-4D0B-8A12-6A04D22318AC}"
```

After adding the new `EventManagement` solution folder and six new
project entries, nested under that same GUID: still exactly one match,
same GUID.

## What was NOT touched

`src/Contexts/Maintenance/` and `tests/Nexus1.Maintenance.*` — confirmed
via `git status --porcelain` before writing this report: zero matches for
`Maintenance`. The ADR-022-described reconnection of
`Maintenance.WorkOrder.OriginOperationalEventId`/`OriginIncidentActionId`
to real FKs is explicitly deferred to its own separate follow-up commit,
per the ADR's own sequencing section.

## Composition into `Nexus1.ModularRuntime`

`AddEventManagementApplication()`/`AddEventManagementInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing `alarmManagementConnectionString`
variable — no new connection string, no `appsettings.json` change.
`.AddCheck<DbContextHealthCheck<EventManagementDbContext>>("eventmanagement-db")`
added to the health-check chain with a comment referencing ADR-022.
`Nexus1.ModularRuntime` builds clean with all seven contexts composed.
