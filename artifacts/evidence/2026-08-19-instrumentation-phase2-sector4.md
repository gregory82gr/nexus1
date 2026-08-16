# Evidence: Instrumentation (Phase 2, sector 4 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-019-instrumentation-phase2-scope-and-persistence.md`
(amended in-session with a genuine implementation-time correction, see
"Owned" below). This report is the real proof: fifteen of forty atlas
tables modeled in Domain (the passport-carrying signal registry plus the
atlas's own four named verification queries), EF Core persistence
sharing the existing `AlarmManagementDb` with real cross-schema foreign
keys (not passport-only ints, a first among the four Phase 2 sectors),
composed into `Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still
green, and a real host startup with a working `instrumentation-db` health
check — the first sector built since ADR-018's health-check fix, so also
the first real proof that fix behaves correctly for a *new* schema added
to an *already-migrated* shared database.

## Automated regression: 476/476 passing (was 409/409 before this step)

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
Nexus1.Instrumentation.UnitTests                  52/52 passed  (new)
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
Nexus1.Instrumentation.ComponentTests             15/15 passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors. Run with `-m:1` (serial
per-project execution) for an unambiguous per-project breakdown, same
reasoning as the Organization report. `Nexus1.Contracts.ContractTests`
and `Nexus1.DistributedSlice.EndToEndTests` remain pre-existing "no
tests" placeholder projects, unrelated to this step.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.5** (not C.4 or C.8 — the file's
  own printed table of contents is garbled; the real section number was
  confirmed by reading the actual `"C.5.1 Sector purpose"` header
  directly, recorded in ADR-019 so a future session doesn't have to
  rediscover this): **forty tables** (fourteen lookup, twenty-six
  substantive).
- `From_Domain_to_Twin`'s Chapter 14 classifies Instrumentation as a
  **Supporting domain**, with a real, present design question ("Is the
  tag stable and unambiguous?") — a different signal than Security's flat
  "generic" classification, informing real invariants on `Signal.Tag`
  identity and quality tracking rather than boring pass-through CRUD.
- The atlas's own C.5.7.3 ("Incoming passports from later sectors") names
  five future consumers (`DigitalTwin`, `AlarmManagement`, `RootCause`,
  `ReinforcementLearning`, `Reporting`) that **all** reference
  `Instrumentation.Signal` and nothing else in the sector — the strongest,
  narrowest scope signal of any Phase 2 sector so far.
- The atlas's own four "useful verification queries" (C.5.8) named the
  Application layer's real operations directly, and confirmed by reading
  each query's own `JOIN` list that none touches the excluded
  instrument/sensor/calibration/historian-apparatus tables.
- `Signal.SensorChannelId` is nullable in the atlas's own DDL — schema-
  level confirmation that the physical-hardware chain is genuinely
  optional to signal identity, reinforcing the scope-exclusion signal
  from the queries and passports.
- `AlarmManagement`, `RootCause`, `Reporting` (three of the five named
  future consumers, already built in Phase 1) were checked directly via
  `grep` — none currently has any `SignalTag`/`SignalId`-shaped field.

Full reasoning for what was built vs. deliberately not built, and the
persistence decision, is in ADR-019; not repeated here.

## Domain layer — seven substantive entities, real invariants on identity and quality

`Nexus1.Instrumentation.Domain`: 8 lookups (`SignalType`,
`SignalCategory`, `SignalRole`, `SamplingMode`, `HistorianRetentionClass`,
`SignalQuality`, `MeasurementSource`, `ChannelStatus`) and 7 substantive
entities (`Signal`, `DataAcquisitionNode`, `AcquisitionConnection`,
`AcquisitionPoint`, `SignalMapping`, `Measurement`,
`SignalQualityEvent`), each with a `Create` factory enforcing the atlas's
real `CHECK` constraints. Real behavior verified directly by reading the
source, not taken on report:

- `Signal.Tag` is the real unique business key (matching the book's own
  "is the tag stable and unambiguous?" design question), with `Create`
  enforcing `NormalMax > NormalMin` (when both set) and `ScanRateHz > 0`
  (when set).
- `Measurement.Create` enforces `CK_Instrumentation_Measurement_OneValue`
  — confirmed by reading `Measurement.cs` directly: the factory throws
  `ArgumentException` when both `numericValue` and `textValue` are
  null/empty, matching the atlas's own `CHECK` exactly, not a comment
  claiming it does.
- `SignalQualityEvent` gets an open/close lifecycle (`Create` with
  `EndedAtUtc = null`, then `Close(...)` re-validating `EndedAtUtc >
  StartedAtUtc`), directly serving C.5.9's "a value without quality is
  not evidence" emphasis.
- `SignalMapping`'s `EffectiveFromUtc`/`EffectiveToUtc` gets the same
  time-bounded pattern already used for Organization's
  `DepartmentAssignment`/`TeamMembership`.

Audit columns not modeled in Domain, same restraint as every prior
sector. 52 unit tests: creation validation for all 15 entities plus every
real behavior, including both directions of `Measurement`'s one-value
check (numeric-only accepted, text-only accepted, neither rejected) and
the `SignalQualityEvent` open/close lifecycle.

## A genuine correction caught during implementation — ADR-019 amended in-session

ADR-019 originally claimed every external reference in this sector's
scope would be a real, enforced FK, including `Signal.EquipmentId`/
`PlantSystemId` → `ReactorFleet.Equipment`/`PlantSystem`. While writing
the `Signal` EF configuration, this codebase's actual
`ReactorFleetDbContext` was checked directly and found to expose only
`Unit`/`UnitPowerSnapshot` — `Equipment`/`PlantSystem` were never built in
Phase 1 (`Unit.cs`'s own comment: *"the Schema Atlas's Reactor/Equipment/
etc. tables are deliberately not modeled yet"*, ADR-003). A `FOREIGN KEY`
cannot reference a table that doesn't exist, so `EquipmentId`/
`PlantSystemId` are plain nullable passport ints with no enforced
constraint — not the blanket "every reference is a real FK" the ADR
first claimed. ADR-019's Persistence section was corrected in-session to
record this, matching the "verification convention" already established
by ADR-016/ADR-017's own corrections: a claim about scope must be checked
against the actual code before being asserted, not assumed from the
atlas alone.

A second, related first: no existing context in this codebase had ever
declared a real cross-context FK before this sector (`AlarmManagement.
UnitId` is itself passport-only despite sharing `AlarmManagementDb` with
`ReactorFleet`) — confirmed by `grep` before writing any code. There was
no existing pattern to copy for `Signal.UnitId`/`EngineeringUnitId`'s real
FKs. The technique used: a local, read-only shadow entity type per
external table (`ReactorFleetUnitReference`,
`CorePlatformEngineeringUnitReference`), mapped via `ToTable(...,
ExcludeFromMigrations())` onto the same physical table the owning
context's own migration already created — lets EF declare a genuine
`HasOne`/`WithMany` foreign key without an Infrastructure-layer
`ProjectReference` across contexts, which `Nexus1.ArchitectureTests`'
dependency-law test forbids. Verified independently, not taken on
report: the generated migration's `FOREIGN KEY` statements target
`ReactorFleet.Unit`/`CorePlatform.EngineeringUnit` with no matching
`CreateTable` for either, and — after applying the migration to the real
`AlarmManagementDb` — a direct `sys.foreign_keys` query shows exactly
three real, live cross-schema constraints:
`FK_Instrumentation_DataAcquisitionNode_Unit`,
`FK_Instrumentation_Signal_Unit`, `FK_Instrumentation_Signal_
EngineeringUnit`.

## EF Core Infrastructure — shares AlarmManagementDb, fifteen configurations, one reviewed migration

`Nexus1.Instrumentation.Infrastructure`: `InstrumentationDbContext`
targeting the **existing `AlarmManagementDb`** (own `Instrumentation`
schema, own migration-history table
`__EFMigrationsHistory_Instrumentation`), matching CorePlatform's own
sharing pattern, not Security/Organization's own-database pattern.
Migration inspected directly: exactly 15 `CreateTable` calls (matching
ADR-019's scope precisely), the `Measurement.MeasurementId` primary key
mapped `.IsClustered(false)` — verified in the generated migration
(`.Annotation("SqlServer:Clustered", false)`) — matching the atlas's own
`PRIMARY KEY NONCLUSTERED` for this high-volume fact table, and 6 `CHECK`
constraints across `Signal` (2), `SignalMapping` (1), `Measurement` (1),
`SignalQualityEvent` (1), `AcquisitionConnection` (1).

Migration: `20260816161628_InitialInstrumentationSchema`, generated via
`dotnet ef migrations add InitialInstrumentationSchema --project src/Contexts/Instrumentation/Nexus1.Instrumentation.Infrastructure --startup-project src/Contexts/Instrumentation/Nexus1.Instrumentation.Infrastructure --output-dir Persistence/Migrations`.

## Application layer — the atlas's own four named verification queries plus real per-table behaviors

Six operations:

- `GetActiveHistorizedSignalsForUnitQuery` — atlas C.5.8 query 1, verbatim.
- `GetLatestMeasurementsForTagQuery` — atlas C.5.8 query 2, verbatim (top
  10 most recent measurements for a tag).
- `GetOpenSignalQualityEventsForUnitQuery` — atlas C.5.8 query 3, with a
  documented interpretive addition: the atlas's own SQL for this query
  has no unit filter despite the query's own narrative ("stale or bad
  signals in the current unit"); a `Signal.UnitId` join/filter was added
  so the "ForUnit" operation name actually means something, noted in the
  handler's own doc comment rather than silently deviating from the
  atlas text.
- `GetAcquisitionPathForTagQuery` — atlas C.5.8 query 4, verbatim.
- `RecordMeasurementCommand` — `Measurement`'s defining behavior.
- `OpenSignalQualityEventCommand`/`CloseSignalQualityEventCommand` —
  `SignalQualityEvent`'s defining lifecycle.

15 component tests against real LocalDB, including a dedicated test
proving `GetLatestMeasurementsForTagQuery` returns exactly the most
recent 10 rows in the correct order, and a dedicated test proving
`Measurement.Create` rejects a measurement with neither numeric nor text
value. Component tests seed real `ReactorFleet.Unit` and
`CorePlatform.EngineeringUnit` rows (via `MigrateAsync` against all three
DbContexts in the test fixture) to exercise the real cross-context FK
constraints, not mocked references.

## Composed into Nexus1.ModularRuntime

`AddInstrumentationApplication()`/`AddInstrumentationInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, **reusing the existing shared connection string**
— confirmed directly by reading `Program.cs` and diffing
`appsettings.json` (zero changes, no new connection string needed, since
Instrumentation shares `AlarmManagementDb` with `ReactorFleet`/
`CorePlatform`/`AlarmManagement`). `instrumentation-db` entry added to the
health check chain, bringing the total to **9** registered checks —
confirmed by `grep` count, not assumed.

`Nexus1.ArchitectureTests` needed zero code changes — run standalone,
7/7 passing.

**Real host startup, independently re-verified end-to-end**: applied the
migration (`dotnet ef database update`) against the real
`AlarmManagementDb`; confirmed via direct `sqlcmd` that all 15
`Instrumentation.*` tables now exist (schema count: `AlarmManagement` 3,
`CorePlatform` 11, `Instrumentation` 15, `ReactorFleet` 2 — Instrumentation
is genuinely the largest schema in the shared database now) and that the
three cross-schema `FOREIGN KEY` constraints are live in
`sys.foreign_keys`, *before* ever asking the health check about it. Then
built and ran the actual `Nexus1.ModularRuntime.dll`, confirmed
`GET /health/ready` returns `200 Healthy` with `instrumentation-db`
genuinely present. This is also the first real confirmation that
ADR-018's strengthened `DbContextHealthCheck<T>` behaves correctly on the
*addition* case — a brand-new schema being added to an *already-correctly
-migrated* shared database — not just the *regression* case (re-checking
sectors that were already fine) it was originally verified against.

## Owned

- The genuine ADR-019 correction (`Signal.EquipmentId`/`PlantSystemId`
  cannot be real FKs; ReactorFleet's Phase 1 scope never built
  `Equipment`/`PlantSystem`) is recorded above and directly in ADR-019's
  own text, not silently patched.
- The new cross-context real-FK technique (`ExcludeFromMigrations`
  shadow-entity references) is a genuine first for this codebase — no
  prior sector had a real FK across context boundaries at all, including
  `AlarmManagement`/`ReactorFleet` which have shared `AlarmManagementDb`
  since Phase 1. This is now the reference pattern for any future sector
  that needs a real (not passport-only) cross-context FK within a shared
  database.
- `GetOpenSignalQualityEventsForUnitQuery`'s unit-filter addition beyond
  the atlas's own literal SQL is a documented interpretive call, not a
  silent deviation.
- No `src/` files outside the new `Nexus1.Instrumentation.*` projects and
  `Nexus1.ModularRuntime`'s composition root (csproj, `Program.cs`) were
  touched — confirmed via `git status`. `appsettings.json` was correctly
  left untouched.
- `AlarmManagementDb` gained the `Instrumentation` schema alongside its
  existing `ReactorFleet`/`CorePlatform`/`AlarmManagement` schemas — left
  in place, harmless local dev state, same reasoning as every prior step.

## Scope explicitly not covered by this step

Per ADR-019, twenty-five of the atlas's forty Instrumentation tables
remain unbuilt, in six named groups: the physical-hardware chain
(`Instrument`, `Sensor`, `SensorChannel` — `Signal.SensorChannelId` is
nullable in the atlas's own DDL, confirming this is genuinely optional);
alias/grouping/derivation apparatus (`SignalAlias`, `SignalGroup`,
`SignalGroupMember`, `SignalDependency`, `SignalLineage`); alarm-threshold
apparatus (`SignalLimit`, `SignalDeadband` — `AlarmManagement` doesn't
consume `Instrumentation` at all yet, see below); calibration
(`CalibrationPlan`, `CalibrationRecord`); historian apparatus beyond the
raw fact table (`MeasurementAggregate`, `MeasurementAnnotation`,
`DataGap`, `HistorianRetentionPolicy`, `HistorianImportBatch`,
`HistorianImportBatchItem`, `HistorianBackfillJob`); plus the six lookups
backing only those groups. None are silently dropped — each is named in
ADR-019 with the specific reason it was deferred.

The reversal note for `AlarmManagement`/`RootCause`/`Reporting` (all three
declared by the atlas as future signal consumers, all three confirmed via
`grep` to currently have zero signal reference) is recorded in ADR-019
but **not** performed here, per instruction — the door is technically
open now that `Instrumentation.Signal` exists, left closed unless
explicitly asked, with the book's own `SignalTag`-string-passport pattern
(not a hard `SignalId` FK) noted for whoever eventually does it.

This closes Instrumentation, sector 4 of 11 in Phase 2, and confirms
ADR-018's health-check fix works correctly for new-schema additions, not
just regression re-checks. Awaiting the next checkpoint instruction
before starting sector 5.
