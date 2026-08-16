# Evidence: CorePlatform (Phase 2, sector 1 of 11) — Domain, Application, Infrastructure

Date: 2026-08-17
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-015-coreplatform-phase2-scope-and-persistence.md`. This report
is the real proof: all eleven CorePlatform tables modeled in Domain, EF Core
persistence with a reviewed migration, the five Application-layer operations
the atlas itself highlights as CorePlatform's real behavior, composed into
`Nexus1.ModularRuntime`, and `Nexus1.ArchitectureTests` still green with the
new projects in place.

## Automated regression: 249/249 passing (was 194/194 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.CorePlatform.UnitTests                     46/46 passed  (new)
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed
Nexus1.AlarmManagement.ComponentTests              19/19 passed
Nexus1.Audit.ComponentTests                       13/13 passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   16/16 passed
Nexus1.CorePlatform.ComponentTests                 9/9  passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors.

## What was verified against the source material before writing code

Per this project's standing convention, the atlas and domain book were read
directly, not assumed:

- `From_Schema_to_System` Appendix C.1: eleven CorePlatform tables
  (`AppSetting`, `SystemConfiguration`, `FeatureFlag`, `Language`,
  `Localization`, `Country`, `Region`, `TimeZone`, `Calendar`,
  `EngineeringUnit`, `Version`), three internal foreign keys
  (`Localization`→`Language`, `Region`→`Country`, `Calendar`→`TimeZone`),
  six reference tables with real natural keys, and C.1.8's three
  "useful verification queries."
- `From_Domain_to_Twin`: confirmed CorePlatform has no aggregate design in
  that book beyond the bare-identity `EngineeringUnit` bounded-context
  naming-collision example (pp. 24, 45) — there was no gap to fill the way
  ADR-002 filled one for CQRS shape, just a genuinely thin domain to build
  from first principles.

## Domain layer — all eleven tables, business columns only

`Nexus1.CorePlatform.Domain`: eleven entities (`AppSetting`,
`SystemConfiguration`, `FeatureFlag`, `Language`, `Localization`, `Country`,
`Region`, `TimeZoneReference`, `Calendar`, `EngineeringUnit`,
`DeploymentVersion`), each with a `Create` factory enforcing the atlas's own
CHECK constraints and NOT NULL rules as real validation (e.g.
`SystemConfiguration`'s `ISJSON`/schema-version/effective-window checks,
`Calendar`'s 24x7-or-working-hours check, `TimeZoneReference`'s ±840-minute
offset range, `Country`'s fixed ISO-2/ISO-3 lengths). Five real behaviors
beyond creation, matched to the two tables the atlas itself calls out as
runtime-mutable plus the two with a genuine per-row lifecycle:
`AppSetting.UpdateValue`, `FeatureFlag.Enable`/`Disable`/`IsActiveAt`,
`Localization.UpdateValue`, `DeploymentVersion.MarkCurrent`/`MarkNotCurrent`.
46 unit tests, no persistence, covering creation validation and every real
behavior.

Two entities are named differently from their atlas table (documented
in-code, not silent): `TimeZoneReference` (table `CorePlatform.TimeZone`)
and `DeploymentVersion` (table `CorePlatform.Version`) — `System.TimeZone`
and `System.Version` both already exist in the BCL, and this project's
`ImplicitUsings` brings `System` into scope, so either atlas name would be a
genuine `CS0104` compiler ambiguity, not a style choice.

Per ADR-015, `CreatedBy`/`ModifiedAtUtc`/`ModifiedBy`/`IsDeleted`/
`RowVersion` — present on nearly every atlas CorePlatform table — are not
modeled: no behavior in either book attaches to them, and one `CreatedAtUtc`
timestamp (the one audit column this project's other entities already carry
an equivalent of) was judged the right stopping point rather than
duplicating six near-identical properties across eleven entities for zero
domain value.

## EF Core Infrastructure — one configuration per entity, one migration

`Nexus1.CorePlatform.Infrastructure`: `CorePlatformDbContext` (eleven
`DbSet`s), one `IEntityTypeConfiguration` per entity under
`Persistence/Configurations/CorePlatform/`, matching the atlas's real column
types, lengths, and named constraints (`PK_CorePlatform_*`,
`UQ_CorePlatform_*`, `IX_CorePlatform_*`, `FK_CorePlatform_*`) — including
the atlas's own filtered unique indexes (`UX_CorePlatform_Language_Default`
... actually not built, see "Scope not covered" below;
`UX_CorePlatform_Version_Current_Component` is built, filtered on
`IsCurrent = 1`).

**A real discrepancy caught and fixed before generating the migration**: the
atlas's own FK DDL for `Localization`→`Language`, `Region`→`Country`, and
`Calendar`→`TimeZone` has no `ON DELETE` clause, meaning SQL Server's
default `NO ACTION`. EF Core's own default for a required relationship is
`CASCADE`. Left alone, the migration would have made deleting a `Country`
silently cascade-delete every `Region` row referencing it — a behavior the
atlas's own schema does not have. Caught by checking the generated migration
against the atlas DDL rather than trusting EF's default, fixed with
`.OnDelete(DeleteBehavior.Restrict)` on all three relationships, and the
migration regenerated to confirm `ReferentialAction.Restrict` in the output.

ID generation follows the same caller-supplied convention as every other
context (`IIdGenerator`, `ValueGeneratedNever()`) rather than the atlas's own
`IDENTITY(1,1)` — the same adaptation `ReactorFleet.Unit`'s own configuration
already made and explained, applied uniformly here rather than re-decided
per table.

Migration: `20260816114650_InitialCorePlatformSchema`, `CorePlatform`
schema, own migration-history table (`__EFMigrationsHistory_CorePlatform`),
reviewed for readable table/column/constraint names before being accepted.

## Application layer — the atlas's own highlighted operations

Five operations, matched directly to what C.1.8 and C.1.4.2/C.1.4.4 name as
CorePlatform's real behavior, not a CRUD handler per table (ADR-015):

- `GetActiveEngineeringUnitsQuery` — atlas C.1.8's own first verification
  query, verbatim (active units, ordered by `QuantityType`, `DisplayOrder`,
  `Symbol`).
- `ResolveLocalizedTextQuery` — atlas C.1.8's own second verification query,
  verbatim (target language lookup with an English fallback).
- `GetCurrentDeploymentVersionsQuery` — atlas C.1.8's own third verification
  query, verbatim (components where `IsCurrent = 1`).
- `UpdateAppSettingValueCommand` — `AppSetting`'s defining behavior
  (C.1.4.2: "Runtime values that can be changed without redeploying").
- `EvaluateFeatureFlagQuery` — `FeatureFlag`'s defining behavior (C.1.4.4:
  "Switches capabilities on or off... optionally with expiry"), fail-closed
  on an unknown flag code (evaluates `false`, never an error).

9 component tests against real LocalDB (fresh database per test, migrated
and dropped around each run, matching every prior context's own component
test discipline): value updates persisted and readable back with an
independent `DbContext`, unknown keys/codes fail or fail-closed correctly,
the fallback-resolution and active-filtering/ordering logic proven against
real seeded rows.

## Composed into Nexus1.ModularRuntime

`AddCorePlatformApplication()`/`AddCorePlatformInfrastructure(alarmManagementConnectionString)`
wired into `Program.cs` alongside ReactorFleet's own registration (same
physical database, ADR-015), plus a `coreplatform-db` entry in the existing
health-check chain. Verified for real, not assumed: built and ran the actual
host (`dotnet run --project src/Hosts/Nexus1.ModularRuntime`), confirmed
`GET /health/ready` returns `200 Healthy` — the new `CorePlatformDbContext`
health check genuinely connects to the shared database, not just compiles.

`Nexus1.ArchitectureTests` needed **zero code changes** — its
`DependencyLawTests.Classify` method infers layer and context purely from
the `Nexus1.<Context>.<Layer>` naming convention already established, so
`Nexus1.CorePlatform.Domain`/`.Application`/`.Infrastructure` were
classified and enforced automatically. 7/7 passing, confirming CorePlatform
correctly has no illegal cross-context references.

## Owned

- No src/ files outside the new `Nexus1.CorePlatform.*` projects and
  `Nexus1.ModularRuntime`'s composition root were touched.
- `AlarmManagementDb` gained the `CorePlatform` schema alongside its
  existing `ReactorFleet` and `AlarmManagement` schemas — left in place,
  harmless local dev state, same reasoning as every prior step.
- The `Localization`/`Region`/`Calendar` FK `ON DELETE` discrepancy above is
  the one genuine "caught before it became a real bug" finding in this
  step — a real behavioral gap between the atlas's actual schema and what
  EF Core would have generated by default, not a cosmetic naming choice.

## Scope explicitly not covered by this step

Per ADR-015: `SystemConfiguration`, `Language`, `Country`, `Region`,
`TimeZoneReference`, and `Calendar` have no Application-layer command yet —
modeled fully in Domain and Infrastructure, but this slice's Application
layer covers only the five operations named above. The atlas's
`UX_CorePlatform_Language_Default` filtered unique index (single default
language) was not built — no seed data or command in this slice creates
more than one `Language` row, so there is nothing yet for it to guard;
flagged here rather than silently omitted, to be added if/when a future
step actually needs it. No cross-schema foreign keys from other sectors
into CorePlatform exist yet — Security, Organization, and Instrumentation
(the atlas's own named future consumers, C.1.7.2) are sectors 2, 3, and 4 of
Phase 2, not yet built.

This closes CorePlatform, sector 1 of 11 in Phase 2. Security is next per
CLAUDE.md §9's ordering.
