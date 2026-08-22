# Evidence: DigitalTwin (Phase 2, sector 5 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`, RabbitMQ 4.3.4 (portable, Erlang/OTP 27).

Scope and decisions are recorded in
`docs/adr/ADR-020-digitaltwin-phase2-scope-and-persistence.md`. This report
is the real proof: twenty of forty-two atlas tables modeled in Domain (the
model→version→variable→binding→runtime→snapshot→divergence→review spine),
EF Core persistence sharing `AlarmManagementDb` with four real cross-schema
foreign keys, composed into `Nexus1.ModularRuntime`, `Nexus1.
ArchitectureTests` still green, and a real host startup with a working
`digitaltwin-db` health check.

## A note on this step's process: session crash and recovery

The implementation agent completed its full task, but a session crash
happened before its final report message was delivered — the harness
reported "no completion record found" on restart. **Before assuming
anything was lost or needed redoing, the actual state on disk was checked
directly**: `git status` showed every expected file present and
uncommitted; a full solution build succeeded warning-clean, including
`Nexus1.ModularRuntime` (confirming host composition was also finished);
and the full test suite passed with zero failures anywhere, including 55
new `Nexus1.DigitalTwin.UnitTests` and 11 new
`Nexus1.DigitalTwin.ComponentTests`. The work was genuinely complete —
only the final status message was lost, not the work itself. This report
covers the independent verification performed after confirming that,
not a re-implementation.

## Automated regression: 542/542 passing (was 476/476 before this step)

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
Nexus1.DigitalTwin.UnitTests                      55/55 passed  (new)
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
Nexus1.DigitalTwin.ComponentTests                 11/11 passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors. Architecture tests confirmed
standalone (7/7) as well as within the full serial run.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.6** (confirmed via the real
  `"C.6.1 Sector purpose"` header): **forty-two tables** (fifteen lookup,
  twenty-seven substantive) — C.6.1 calls this "the keystone sector."
- `From_Domain_to_Twin` places DigitalTwin explicitly in the **Core
  domain** — the first Phase 2 sector to get this classification
  (Organization: absent from all three tables; Security: flat generic;
  Instrumentation: supporting), justifying deeper domain investment than
  any prior Phase 2 sector.
- The atlas's own three verification queries (C.6.8) named the
  Application layer's real operations, and the incoming-passports section
  (C.6.7.3) named the same third-occurrence reversal pattern already seen
  for Organization/ADR-004 and Instrumentation/`SignalTag` — three more
  already-built Phase 1 contexts (`AlarmManagement`, `RootCause`,
  `Reporting`) confirmed via `grep` to have zero reference to any
  DigitalTwin table.
- `TwinModelComponent.EquipmentId`/`PlantSystemId`'s own `CHECK`
  constraint (requiring at least one non-null) was checked against this
  codebase's real `ReactorFleetDbContext` **before** writing any code —
  confirmed `Equipment`/`PlantSystem` don't exist (same finding as
  Instrumentation's own correction, but caught proactively this time,
  during ADR drafting rather than during implementation) — leading to
  excluding the table entirely rather than downgrading it, since no
  passport-only version preserves its actual meaning.

Full reasoning for what was built vs. deliberately not built, and the
persistence decision, is in ADR-020; not repeated here.

## Domain layer — nine substantive entities, Core-domain invariants

`Nexus1.DigitalTwin.Domain`: 11 lookups and 9 substantive entities
(`TwinModel`, `TwinModelVersion`, `TwinVariable`, `SignalBinding`,
`TwinRuntimeSession`, `TwinSnapshot`, `TwinSnapshotValue`,
`TwinDivergence`, `TwinDivergenceReview`), each with a `Create` factory
enforcing the atlas's real `CHECK` constraints.

**`TwinDivergence.Create`'s computed `DeltaValue` verified directly by
reading the source, not taken on report**: the factory computes
`deltaValue = measuredValue - modeledValue` internally
(`TwinDivergence.cs:86`) and the private constructor has no independent
`deltaValue`-from-caller path — there is no way to construct a
`TwinDivergence` whose `DeltaValue` disagrees with its own
`MeasuredValue`/`ModeledValue`, exactly as ADR-020 specified, even though
the atlas's own DDL does not mark this column as a SQL computed column
(unlike Organization's `StaffingScenarioGap.GapCount`).

Audit columns not modeled in Domain, same restraint as every prior
sector; several tables in this sector are genuinely leaner than most
(`TwinSnapshot`/`TwinSnapshotValue`/`TwinDivergence`/
`TwinDivergenceReview` only have `CreatedAtUtc`, no
`ModifiedAtUtc`/`ModifiedBy`/`IsDeleted` — matching the atlas's own DDL,
not a simplification).

## EF Core Infrastructure — shares AlarmManagementDb, twenty configurations, four real cross-schema FKs

`Nexus1.DigitalTwin.Infrastructure`: `DigitalTwinDbContext` targeting the
existing `AlarmManagementDb` (own `DigitalTwin` schema, own
migration-history table `__EFMigrationsHistory_DigitalTwin`). Migration
inspected directly: exactly 20 `CreateTable` calls (matching ADR-020's
scope), 27 foreign keys all `Restrict` (zero `Cascade`).

**Cross-context real FKs verified against the live database, not just the
migration file**: after applying the migration to the actual
`AlarmManagementDb`, a direct `sys.foreign_keys` query confirms exactly
four live cross-schema constraints —
`FK_DigitalTwin_TwinModel_Unit` (→ `ReactorFleet.Unit`),
`FK_DigitalTwin_TwinVariable_EngineeringUnit` (→
`CorePlatform.EngineeringUnit`), `FK_DigitalTwin_SignalBinding_Signal`
and `FK_DigitalTwin_TwinDivergence_Signal` (both → `Instrumentation.
Signal`) — using the `ExcludeFromMigrations` shadow-entity technique
Instrumentation introduced (ADR-019), now reused a second time with a
third shadow type (`InstrumentationSignalReference`) added alongside
`ReactorFleetUnitReference`/`CorePlatformEngineeringUnitReference`. Zero
`principalSchema: "Security"` FKs exist — `TwinModelVersion.
ApprovedByUserId`, `TwinRuntimeSession.StartedByUserId`,
`TwinDivergenceReview.ReviewedByUserId` are correctly passport-only ints.

Migration: `20260816195722_InitialDigitalTwinSchema`, generated via
`dotnet ef migrations add InitialDigitalTwinSchema --project src/Contexts/DigitalTwin/Nexus1.DigitalTwin.Infrastructure --startup-project src/Contexts/DigitalTwin/Nexus1.DigitalTwin.Infrastructure --output-dir Persistence/Migrations`.

## Application layer — the atlas's own three named verification queries plus real per-table behaviors

Six operations:

- `GetActiveTwinsForFleetQuery` — atlas C.6.8 query 1, verbatim.
- `TraceModelVariableToSignalQuery` — atlas C.6.8 query 2, verbatim.
- `GetOpenDivergencesQuery` — atlas C.6.8 query 3, verbatim.
- `CaptureTwinSnapshotCommand` — writes one `TwinSnapshot` plus its
  `TwinSnapshotValue` rows in a single operation.
- `RecordTwinDivergenceCommand` — `TwinDivergence`'s defining behavior.
- `ReviewTwinDivergenceCommand` — `TwinDivergenceReview`'s defining
  behavior.

11 component tests against real LocalDB, migrating four DbContexts
(`ReactorFleet`, `CorePlatform`, `Instrumentation`, `DigitalTwin`) into
one throwaway database per test to exercise the real FK constraints
against real seeded upstream rows, not mocks.

## Composed into Nexus1.ModularRuntime

`AddDigitalTwinApplication()`/`AddDigitalTwinInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing shared connection string
(confirmed: no `appsettings.json` changes). `digitaltwin-db` added to the
health check chain, bringing the total to **10** registered checks.

`Nexus1.ArchitectureTests`: 7/7, confirmed standalone.

**Real host startup — a genuine environment blocker hit and resolved
during this step's own verification, not a DigitalTwin defect**: the
first host-startup attempt crashed on an unhandled
`RabbitMQ.Client.Exceptions.BrokerUnreachableException` — RabbitMQ (a
portable process per `docs/runbooks/local-rabbitmq.md`, not a Windows
service, "started as a regular background process, stopped by killing
that process") was not running, almost certainly because the session
crash/restart that interrupted this step also took down whatever process
had been running it. This is unrelated to DigitalTwin's own code — every
context's host dependency on a live broker (ADR-008) predates this
sector. Fixed by following the project's own runbook (`$env:ERLANG_HOME`/
`$env:RABBITMQ_BASE` + `rabbitmq-server.bat`), confirmed via
`rabbitmqctl status` (AMQP listening on 5672) before retrying. Second
attempt: `GET /health/ready` returned `200 Healthy` with all 10 checks,
including `digitaltwin-db`, genuinely passing against `AlarmManagementDb`
with all four real FK constraints live.

## Owned

- The `TwinModelComponent` exclusion (not just a downgrade) is a genuine,
  proactive application of the Equipment/PlantSystem-unavailability
  finding Instrumentation's evidence report first surfaced reactively —
  checked before writing any DigitalTwin code this time, per ADR-020's
  own Context section.
- The `ExcludeFromMigrations` shadow-entity technique is now used in two
  consecutive sectors — worth treating as an established codebase
  pattern, not an ad hoc one-off, if a future sector needs a third
  variant.
- The RabbitMQ-not-running blocker (see above) is recorded here because
  it's a genuine finding about this step's own verification process —
  worth knowing for any future session that hits the same "host crashes
  on startup, but the code is fine" symptom after a machine/session
  restart.
- **Cleanup investigated per explicit request, not a DigitalTwin defect**:
  two orphaned `SecurityComponentTests_*` databases spotted in SSMS were
  investigated before this sector's work began. Checked every context's
  test-database fixture (`AlarmManagement`, `Audit`, `Compliance`,
  `CorePlatform`, `Instrumentation`, `Organization`, `Reporting`,
  `RootCause`, `Security`, `ServiceDefaults`) — all have identical,
  correct `InitializeAsync`/`DisposeAsync` logic calling
  `EnsureDeletedAsync()`. This is not a code gap; the two leftover
  databases (created 2026-08-16, seconds apart) are consistent with an
  interrupted test run rather than a harness defect. Confirmed zero
  active connections via `sys.sysprocesses`, then dropped both; confirmed
  gone. No code change was needed, so this is recorded here rather than
  as its own commit.
- No `src/` files outside the new `Nexus1.DigitalTwin.*` projects and
  `Nexus1.ModularRuntime`'s composition root (csproj, `Program.cs`) were
  touched — confirmed via `git status`. `appsettings.json` was correctly
  left untouched.
- `AlarmManagementDb` gained the `DigitalTwin` schema alongside
  `ReactorFleet`/`CorePlatform`/`AlarmManagement`/`Instrumentation` — left
  in place, harmless local dev state, same reasoning as every prior step.

## Scope explicitly not covered by this step

Per ADR-020, twenty-two of the atlas's forty-two DigitalTwin tables remain
unbuilt, in eight named groups: `TwinModelComponent` (physical-anchor
`CHECK` constraint can't be satisfied — `ReactorFleet.Equipment`/
`PlantSystem` don't exist in this codebase); `TwinParameter`,
`SignalBindingCalibration` (no verification-query consumer);
`TwinSynchronization`/`TwinStateVector` (runtime replay internals); the
entire simulation/what-if capability (`SimulationScenario` through
`WhatIfCaseResult`, 8 tables plus 2 lookups — real, named future value,
but zero current consumer); `TwinHealthCheck`; `TwinModelValidation`/
`TwinValidationMetric` (named outgoing passport to `Compliance.Evidence`,
deferred until `Compliance` actually consumes it); `ModelAssumption`/
`TwinAnnotation` (governance/documentation apparatus). None are silently
dropped — each is named in ADR-020 with the specific reason it was
deferred.

The reversal note for `AlarmManagement`/`RootCause`/`Reporting` (all three
confirmed via `grep` to have zero DigitalTwin reference) is recorded in
ADR-020 but **not** performed here, per instruction — the third
occurrence of this pattern across Phase 2 so far.

This closes DigitalTwin, sector 5 of 11 in Phase 2. Maintenance is next
per CLAUDE.md §9's ordering. Awaiting the next checkpoint instruction.
