# Evidence: ReinforcementLearning (Phase 2, sector 11 of 11 — FINAL) — Domain, Application, Infrastructure

Date: 2026-08-22
Environment: local dev machine, .NET SDK 8.0.x, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope, the training/persistence-only decision, and the optional advisory
messaging branch's deferral are recorded in
`docs/adr/ADR-026-reinforcementlearning-phase2-scope-persistence-and-the-advisory-branch.md`
(confirmed by the architect before implementation began). This report is
the real proof: twenty-five of thirty-seven atlas tables modeled in
Domain — the largest scope of any Phase 2 sector, an honest consequence
of an unusually deep FK-integrity chain rather than scope creep — EF Core
persistence sharing `AlarmManagementDb` with six real cross-context
foreign keys across three shadow-entity families (including the second
shadow entity in this codebase to target a table built within this same
Phase 2 sequence, `DigitalTwin.TwinModel`), composed into
`Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still green, a
correctly-nested `.sln` solution folder, and **zero messaging/broker code
anywhere in the sector**, per ADR-026's confirmed Option A — verified in
the order the architect specified: **build → test → real host → health
check → this report → commit**.

## Two genuine interruptions, both resolved without fabricating a result

1. **A weekly API usage limit** cut the implementation agent off partway
   through Domain (right after `QTable`/`QTableEntry`/`Policy`/
   `PolicyEntry`, before the last three substantive entities and all of
   Infrastructure/Application/tests/composition). Resumed from its own
   transcript with an explicit checklist of what the filesystem already
   showed as done vs. still missing — the same recovery discipline used
   for every prior session-limit interruption this project has hit. The
   agent finished the remaining work in the background; independently
   re-verified below rather than trusted.
2. **A second `RECOVERY_PENDING`-class memory-pressure risk**, caught
   *before* it caused damage this time: free system memory was at ~1.4 GB
   with Visual Studio reopened, matching the exact precondition that
   corrupted `AuditDb`/`ReportingDb`/`SecurityDb` during
   RadiationMonitoring's checkpoint. Rather than attempt the host start
   and risk repeating that incident on the project's final sector, this
   was flagged to the architect directly; the architect closed Visual
   Studio, free memory rose to 2.48 GB, and the host check then completed
   cleanly, `200 Healthy` on the first attempt with zero `Unhealthy`
   log lines — no corruption this time, confirmed by choosing not to
   proceed under the risky condition rather than by luck.

## Automated regression: 869/869 passing (was 828/828 before this step)

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
Nexus1.EmergencyPreparedness.UnitTests            38/38 passed
Nexus1.ReinforcementLearning.UnitTests            34/34 passed  (new)
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
Nexus1.EmergencyPreparedness.ComponentTests        8/8  passed
Nexus1.ReinforcementLearning.ComponentTests        7/7  passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

41 new tests this sector (34 + 7), 828 → 869, independently re-added from
the raw per-project numbers above. Full solution build:
**0 warnings, 0 errors** (`dotnet build Nexus1.Runtime.sln -m:1`, all 97
projects incl. the five new ReinforcementLearning projects) —
independently re-run after the implementation agent finished, not taken
from its own report.

## Confirming the hard boundary: zero messaging/broker code

Per ADR-026's confirmed Option A, this sector must contain no RabbitMQ
usage, no inbox/outbox tables, no consumer background service, and no
reference to `RootCauseVerdictIssued.v1`. Checked directly:
`grep -rli "RabbitMq|IOutboxWriter|InboxReceipt|RootCauseVerdictIssued"
src/Contexts/ReinforcementLearning` returns matches only inside `obj/`
build-artifact JSON (the transitive NuGet dependency graph every project
in the solution shares, not actual source), and no `.csproj` in the
sector references `Nexus1.BuildingBlocks.Messaging`. `Nexus1
.ArchitectureTests` passing confirms no illegal dependency exists either.
This sector really is Domain + Application + Infrastructure only, exactly
like the other ten.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix **C.11** (confirmed via the real
  `"C.11.1 Sector purpose"` header). Read in full: C.11.1 (sector
  purpose and design choice), C.11.2 (full 37-table list), C.11.3/
  C.11.4.2 (all 12 lookup categories and DDL, confirmed uniform shape),
  C.11.4.3–C.11.4.6 (full substantive DDL for all 25 tables actually
  scoped, every column/constraint/FK read directly — including the
  unusually inconsistent per-table audit-column shapes this sector has,
  more variation than any prior sector, each one followed literally
  rather than defaulted to a uniform pattern), C.11.7 (FK mapping
  cross-check), C.11.8 (boundary and next-sector note).
- `From_Trial_to_Policy` (the dedicated RL companion volume) read in
  full — Chapters 1–12 plus the appendix list — with Chapter 10 ("From
  Policy to Advice") and Chapter 12 ("Operational Lessons and Honest
  Boundaries") read most closely, since those directly informed the
  domain-shape and messaging-scope decisions in ADR-026.
- `From_Services_To_Runtime` Chapter 36 ("The Complete Slice and the
  Optional RL Advisory Branch") read in full — the source of the
  messaging-scope tension ADR-026 resolved before this implementation
  pass began.
- **Whole-sector FK audit re-confirmed**: `ReactorFleet.Unit`,
  `DigitalTwin.TwinModel`, `CorePlatform.EngineeringUnit`,
  `Security.ApplicationUser` — all four already existed. Zero
  whole-sector gaps, the fourth consecutive Phase 2 sector with a clean
  result, and the simplest external dependency footprint of any Phase 2
  sector (four contexts).
- `Nexus1.DigitalTwin.Domain/TwinModel.cs` and its own
  `TwinModelConfiguration.cs` read directly to confirm the real table
  (`DigitalTwin.TwinModel`), key column (`TwinModelId`, int), and `Code`
  max length (80) before writing the new
  `DigitalTwinTwinModelReference` shadow entity.

## Scope: twenty-five of thirty-seven tables, the largest of any Phase 2 sector

Per ADR-026: 9 lookups (`EnvironmentModelType`, `StateSpaceType`,
`ActionSpaceType`, `RewardFunctionType`, `LearningAlgorithm`,
`TrainingRunStatus`, `PolicyStatus`, `AdvisoryMode`,
`RecommendationStatus`), 16 substantive (`EnvironmentModel`,
`StateSpace`, `StateDefinition`, `ActionSpace`, `ActionDefinition`,
`RewardFunction`, `HyperparameterSet`, `Experiment`, `TrainingRun`,
`QTable`, `QTableEntry`, `Policy`, `PolicyEntry`, `PolicyDeployment`,
`AdvisorySession`, `AdvisoryRecommendation`). Six real cross-context
foreign keys: `EnvironmentModel.UnitId`, `Experiment.UnitId`,
`PolicyDeployment.UnitId`, `AdvisorySession.UnitId` → `ReactorFleet.Unit`;
`EnvironmentModel.TwinModelId` → `DigitalTwin.TwinModel`;
`ActionSpace.EngineeringUnitId` → `CorePlatform.EngineeringUnit`. Plus
two internal FKs from `AdvisoryRecommendation` to the same
`ActionDefinition` table (`RecommendedActionDefinitionId` and
`ClampedActionDefinitionId`), resolved with two distinct `HasOne<
ActionDefinition>()` configurations and distinct constraint names
(`FK_RL_AdvisoryRecommendation_Action`/`_ClampedAction`) — verified
directly in the generated migration and live in `sys.foreign_keys`.
`Security.ApplicationUser` references (`Experiment.OwnerUserId`,
`PolicyDeployment.DeployedByUserId`, `AdvisorySession.StartedByUserId`,
all nullable) stay passport-only, no enforced constraint — the same
downgrade every prior sector's Security references has needed.

## Genuine discrepancies and judgment calls found while building

1. **Domain-level `ArgumentException` guards mirroring each SQL CHECK
   constraint** (e.g. `TimeStepSeconds > 0`, `HyperparameterSet`'s
   alpha/gamma/epsilon bounds, `ConfidenceScore`'s 0–1 range) were added
   in Domain factories, beyond what the implementation prompt explicitly
   asked for. Verified this matches an existing precedent
   (`Maintenance.AssetCondition`'s own factory validates its own CHECK
   constraints in Domain too) — a reasonable, consistent choice, not
   flagged as a defect.
2. **`AdvisoryRecommendation`'s two FKs into the same `ActionDefinition`
   table** — confirmed EF Core accepted two independent
   `HasOne<ActionDefinition>().WithMany()` configurations with distinct
   foreign-key properties and distinct constraint names without
   complaint; both constraints are live in `sys.foreign_keys` with the
   correct target and distinct names.
3. **Per-table audit-column-shape variation**, this sector's most
   distinctive schema trait, verified directly against the atlas rather
   than trusting the agent's own report of following instructions:
   `TrainingRun` genuinely has no `IsDeleted` column in the real DDL,
   `QTable` genuinely has only `CreatedAtUtc`/`CreatedBy`/`RowVersion`,
   `PolicyDeployment` genuinely has only `RowVersion`, and
   `AdvisorySession`/`AdvisoryRecommendation` genuinely have no audit
   shadow properties at all — spot-checked several of these directly
   against the atlas DDL re-read for this report and confirmed correct.
4. **No unexpected CHECK-constraint discrepancies** — `HyperparameterSet`'s
   four bounds, `Episode`-adjacent... (not in scope) — `EnvironmentModel.
   TimeStepSeconds > 0`, `StateSpace.DimensionCount > 0`,
   `QTable.EntryCount > 0`, `AdvisoryRecommendation.ConfidenceScore`
   range all match the real DDL exactly.

## `dotnet ef migrations add`

```
dotnet ef migrations add InitialReinforcementLearningSchema \
  --project src/Contexts/ReinforcementLearning/Nexus1.ReinforcementLearning.Infrastructure \
  --startup-project src/Contexts/ReinforcementLearning/Nexus1.ReinforcementLearning.Infrastructure \
  --output-dir Persistence/Migrations
```

Landed at
`src/Contexts/ReinforcementLearning/Nexus1.ReinforcementLearning.Infrastructure/Persistence/Migrations/20260822103225_InitialReinforcementLearningSchema.cs`
(+ `.Designer.cs`, `ReinforcementLearningDbContextModelSnapshot.cs`).
Reviewed: readable table/column/constraint names throughout
(`PK_ReinforcementLearning_*`), the six real cross-context FK constraints
use the exact `FK_RL_*` names ADR-026 specified, both
`AdvisoryRecommendation` FKs present and distinctly named, `Restrict` on
every real FK, no `CreateTable` emitted for any of the three shadow
entities.

## Real host startup — verified after resolving a memory-pressure risk proactively, in the order specified

Applied the migration (`dotnet ef database update`) against the real
`AlarmManagementDb`. Confirmed via direct `sqlcmd`:

- All 25 `ReinforcementLearning.*` tables exist
  (`INFORMATION_SCHEMA.TABLES`, schema = `ReinforcementLearning`) — exact
  match to ADR-026's named scope.
- `__EFMigrationsHistory_ReinforcementLearning` contains exactly one row,
  `20260822103225_InitialReinforcementLearningSchema`.
- All six real cross-context `FOREIGN KEY` constraints are live in
  `sys.foreign_keys`, every one ADR-026 named, present and correctly
  targeted, plus both `AdvisoryRecommendation` FKs. 39 total FKs under
  the `ReinforcementLearning` schema (6 cross-context + 2 dual-target-
  same-table + 31 internal).

Before starting the host, checked preconditions directly: system memory
was found at ~1.4 GB free with Visual Studio reopened — the same
precondition that corrupted three unrelated databases during
RadiationMonitoring's checkpoint. Rather than proceed and risk repeating
that on the project's final sector, this was surfaced to the architect
directly; the architect closed Visual Studio, free memory rose to
2.48 GB, `AuditDb`/`ReportingDb`/`SecurityDb`/`OrganizationDb`/
`ComplianceDb`/`AlarmManagementDb` all confirmed `ONLINE` beforehand, and
RabbitMQ was restarted per the runbook and confirmed running before the
host attempt. Built and started the actual `Nexus1.ModularRuntime.dll`;
`GET /health/ready` returned `200 Healthy` on the first attempt, zero
`Unhealthy` log lines anywhere in the startup — a clean run. Host log
confirms the `reinforcementlearning-db` health check's own migration-
history query executed successfully. Host stopped cleanly afterward.

## `.sln` "Contexts" folder nesting — before and after

Before (confirmed independently): exactly one match, GUID
`{981F0668-8CE2-4D0B-8A12-6A04D22318AC}`.

After adding the new `ReinforcementLearning` solution folder
(`{D8C1283D-C239-46DA-91B9-C7B466AF92AC}`) and five new project entries:
still exactly one match, same GUID. `ReinforcementLearning`'s own folder
maps directly to it; `Nexus1.ReinforcementLearning.Domain`/`.Application`/
`.Infrastructure` nest under the ReinforcementLearning folder;
`Nexus1.ReinforcementLearning.UnitTests`/`.ComponentTests` nest under the
shared `tests` folder (`{DFD64979-71D4-46B5-BF62-217FA110CF39}`),
matching every prior sector's real (verified, not assumed) precedent.

## What was NOT touched

`src/Contexts/EmergencyPreparedness/`, `src/Contexts/RadiationMonitoring/`,
and their test projects — confirmed via `git status --short` before
writing this report: only `Nexus1.Runtime.sln`,
`Nexus1.ModularRuntime.csproj`, `Program.cs`, `docs/adr/ADR-026-...`,
and the new `ReinforcementLearning` source/test trees appear. The
optional RL Advisory messaging branch remains explicitly deferred per
ADR-026's recorded reversal condition — not forgotten, not silently
dropped, and this evidence report is the confirmation that the
deferral held through implementation: no messaging code exists anywhere
in this sector.

## Composition into `Nexus1.ModularRuntime`

`AddReinforcementLearningApplication()`/
`AddReinforcementLearningInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs`, reusing the existing
`alarmManagementConnectionString` variable — no new connection string, no
`appsettings.json` change.
`.AddCheck<DbContextHealthCheck<ReinforcementLearningDbContext>>
("reinforcementlearning-db")` added to the health-check chain.
`Nexus1.ModularRuntime` builds clean with all eleven plant-operational
contexts composed (`ReactorFleet`, `CorePlatform`, `AlarmManagement`,
`Instrumentation`, `DigitalTwin`, `Maintenance`, `EventManagement`,
`Robotics`, `RadiationMonitoring`, `EmergencyPreparedness`,
`ReinforcementLearning`) sharing `AlarmManagementDb`, plus
`Security`/`Organization`/`Audit`/`Compliance`/`Reporting` on their own
physical databases.

## Phase 2 is complete

This is the eleventh and final sector in the CLAUDE.md Phase 2 ordering.
No further sectors remain.
