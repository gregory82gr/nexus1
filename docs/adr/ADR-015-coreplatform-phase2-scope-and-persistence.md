# ADR-015: CorePlatform (Phase 2, sector 1) — scope, domain shape, and persistence

## Status

Accepted.

## Context

Phase 2 (CLAUDE.md §9) starts with CorePlatform, the first of the eleven
remaining Schema Atlas sectors. Verified directly against the source
material before writing any code, per this project's own convention:

- `From_Schema_to_System` Appendix C.1 gives CorePlatform's real shape:
  **eleven tables** — `AppSetting`, `SystemConfiguration`, `FeatureFlag`,
  `Language`, `Localization`, `Country`, `Region`, `TimeZone`, `Calendar`,
  `EngineeringUnit`, `Version` — three with internal foreign keys
  (`Localization`→`Language`, `Region`→`Country`, `Calendar`→`TimeZone`),
  the rest independent roots. The book's own framing (C.1.1): *"CorePlatform
  is a support sector, not a reactor sector... The Core Platform is
  deliberately modest. Its value is that every other sector can stop
  hardcoding the same facts."*
- `From_Domain_to_Twin` does **not** give CorePlatform a DDD aggregate
  design the way it does for ReactorFleet/AlarmManagement/RootCause. Its
  only CorePlatform content (pp. 24, 45) is the bare-identity
  `EngineeringUnit` class (`EngineeringUnitId`, `Symbol`, `QuantityKind`),
  used twice purely as a **bounded-context naming-collision teaching
  example** (`ReactorFleet.Unit` vs `CorePlatform.EngineeringUnit` vs
  `Organization.Unit` — one English word, three contexts). There is no gap
  to fill here the way ADR-002 filled one for CQRS shape — the book simply
  never designs this sector's behavior, because it doesn't have much:
  CorePlatform is reference/lookup and versioned-configuration data, not a
  decision-making domain.

Two restraint questions had to be answered before writing code, per
CLAUDE.md §9's "same restraint principle" instruction, rather than decided
silently.

## Decision

### Scope: all eleven tables, not a further-reduced slice

Unlike ReactorFleet's Schema Atlas presence (ADR-003 found a large set of
reactor-internals tables — `FuelAssembly`, `ControlRod`, `SteamGenerator`,
etc. — with **zero mentions anywhere else in the book**, a clean signal to
cut them), CorePlatform's eleven tables have no such discardable subset.
Every table is individually named and given real purpose in C.1.1–C.1.3
("Configuration values live here instead of inside application constants,"
"Countries, regions, time zones, and calendars live here instead of being
repeated in Organization, Security, Reporting, and Emergency
Preparedness," etc.), and C.1.7.2's "incoming passports from later
sectors" table cross-references six of them from named consumers
(`Security.UserPreference`, `Organization.Site`/`Plant`,
`Instrumentation.Signal`, `AlarmManagement.AlarmDefinitionText`,
`Reporting.ReportSchedule`, `Audit.AuditEntry`) — real, atlas-declared
future consumers, not speculative ones this project invented. The
book's own "eleven tables" is already the minimal, deliberately-modest
shape; there is nothing left in CorePlatform to further restrain the way
ADR-003 restrained ReactorFleet.

**All eleven tables are modeled.**

### Domain shape: business columns modeled; generic audit/versioning columns are not

Every CorePlatform table carries a near-identical set of CRUD
provenance/soft-delete columns: `CreatedAtUtc`, `CreatedBy`,
`ModifiedAtUtc`, `ModifiedBy`, `IsDeleted`, `RowVersion`. Checked against
the atlas and the domain book for any attached business rule (the same
check ADR-003/004/005 already applied to their own sectors' columns): none
exists. No CorePlatform behavior in either book depends on "who created
this row" or "was this row soft-deleted" — unlike, say,
`RootCauseAnalysis.ClosedAtUtc`, which genuinely gates `EnsureOpen()`.
Modeling six near-identical audit properties on eleven entities (66
properties) with zero behavior attached would be repetition for its own
sake, not domain modeling — the same restraint this project already
applied when cutting atlas columns that carry no real behavior.

**Decision**: each entity models its own distinguishing business columns
(`Key`/`Value`/`ValueType` for `AppSetting`, `Code`/`IsEnabled` for
`FeatureFlag`, etc.) plus a single `CreatedAtUtc` timestamp (genuinely
useful provenance, low cost, and the one audit column this project's
existing entities already model an equivalent of — `OpenedAtUtc`,
`RaisedAtUtc`, etc.). `CreatedBy`, `ModifiedAtUtc`, `ModifiedBy`,
`IsDeleted`, and `RowVersion` are **not** modeled in Domain and **not**
added to the EF migration — they are ops/audit-trail columns this Phase 2
slice has no consumer or behavior for, not silently-dropped real facts.

### ID generation: same caller-supplied convention as every other context

The atlas declares every CorePlatform PK as `IDENTITY(1,1)`.
`ReactorFleet.Unit` already established this project's answer to that
exact situation (`UnitConfiguration`'s own comment): the atlas's
`IDENTITY` declaration is not used because every aggregate factory in this
codebase requires a caller-supplied id (via `IIdGenerator`), for
consistency across contexts, not decided per table. CorePlatform follows
the same convention — `Create(...)` methods take an id parameter,
`ValueGeneratedNever()` in the EF configuration — rather than introducing
a second id-generation strategy (real SQL `IDENTITY`) just for this one
sector.

### Application layer: the atlas's own highlighted operations, not CRUD-per-table

CLAUDE.md §9 scopes Phase 2 sectors to "Application (core commands/
queries)," not full CRUD for every table — the same restraint already
applied to ReactorFleet (one command, not five) and RootCause (five
commands for a five-table-equivalent aggregate, not one per column).
CorePlatform's own C.1.8 ("Useful verification queries") names exactly
three read operations the atlas considers the real point of this sector:
active engineering units, localized-text resolution with an English
fallback, and currently-deployed component versions. Two tables are
explicitly described as *"mutable... changed without redeploying"*
(`AppSetting`) and *"switches capabilities on or off"* (`FeatureFlag`) —
real runtime behavior, not static reference data. **The Application layer
covers exactly these five operations** — `GetActiveEngineeringUnitsQuery`,
`ResolveLocalizedTextQuery`, `GetCurrentVersionsQuery`,
`UpdateAppSettingValueCommand`, `EvaluateFeatureFlagQuery` — not a
generic CRUD handler set for all eleven tables. The other six tables
(`SystemConfiguration`, `Language`, `Localization`'s own creation,
`Country`, `Region`, `TimeZone`, `Calendar`) are modeled fully in Domain
and Infrastructure (real entities, real EF configuration, real
constraints) but have no Application-layer command yet — they are seeded/
reference data with no atlas-described runtime mutation behavior in this
slice, the same way `Country`/`Region`/`TimeZone` don't need an "update"
command until something in this project actually edits them.

### Persistence: shares AlarmManagement's physical database, own `CorePlatform` schema

ADR-006 already answered this exact question for ReactorFleet and the
reasoning transfers directly: CorePlatform is not independently deployed —
per CLAUDE.md §9 it lives inside `Nexus1.ModularRuntime`, same process,
same deployment unit as ReactorFleet and AlarmManagement. The book's
DB-per-service argument (deployment isolation, independent scaling,
independent failure domains) doesn't apply to a context with no
independent deployment. **CorePlatform gets its own `CorePlatformDbContext`,
its own `CorePlatform` SQL schema, its own migration-history table
(`__EFMigrationsHistory_CorePlatform`), sharing the same physical database
already used for ReactorFleet and AlarmManagement** (referred to as
`AlarmManagementDb` per existing precedent).

**Where this genuinely differs from ADR-006's reasoning, flagged as
asked**: `AuditDb`/`ComplianceDb`/`ReportingDb`'s isolation is about
*independently-deployed bounded contexts* that must never share tables or
real foreign keys — cross-context references stay at the passport-ID
level even when (as ReactorFleet/AlarmManagement do) they happen to share
a physical database for deployment convenience. CorePlatform is a
different kind of thing: shared reference/lookup data with no competing
business invariants of its own, and the atlas's own C.1.7.2 explicitly
designs **real, physical cross-schema foreign keys** from consuming
sectors into CorePlatform tables (`Organization.Site.CountryId` →
`CorePlatform.Country.CountryId`, `Instrumentation.Signal.EngineeringUnitId`
→ `CorePlatform.EngineeringUnit.EngineeringUnitId`, etc.) — not
passport-only references. **Decision**: later Phase 2 sectors that
consume CorePlatform reference data (Security, Organization,
Instrumentation, and others per C.1.7.2) MAY declare real EF Core foreign
keys into CorePlatform's tables when they are built, the same way the
atlas itself does — this is a shared-kernel/generic-subdomain
relationship, not a bounded-context boundary, and does not reopen or
weaken the passport-ID-only rule that still governs relationships
*between* genuine bounded contexts (ReactorFleet↔AlarmManagement,
AlarmManagement↔RootCause, and so on). Nothing in this step adds such an
FK yet — CorePlatform has no consumer built yet — this is a forward
decision recorded now so the next sector that needs it doesn't have to
re-litigate the question.

## Consequences

- `Nexus1.CorePlatform.Domain`, `Nexus1.CorePlatform.Application`,
  `Nexus1.CorePlatform.Infrastructure` — three new projects, same shape as
  every other context, composed into `Nexus1.ModularRuntime` only (no
  independent host, per CLAUDE.md §9).
- One migration, `CorePlatform` schema, eleven tables, targeting the same
  physical database as `ReactorFleet`/`AlarmManagement`.
- Six of eleven tables have no Application-layer command in this step —
  named explicitly above, not silently absent.
- Future Phase 2 sectors consuming CorePlatform reference data are
  authorized (not merely permitted by omission) to use real cross-schema
  foreign keys, a documented exception to the passport-ID-only rule that
  still governs bounded-context-to-bounded-context relationships.

## Rejected alternatives

- **Give CorePlatform its own physical database.** Rejected for the same
  reason ADR-006 rejected it for ReactorFleet: over-provisions deployment
  isolation for a context with no independent deployment.
- **Model the full CRUD audit trail (`CreatedBy`/`ModifiedAtUtc`/
  `ModifiedBy`/`IsDeleted`/`RowVersion`) on every entity for schema
  fidelity.** Rejected: no attached behavior in either book: six
  near-identical properties × eleven entities for zero domain value is the
  exact repetition this project's restraint discipline already argues
  against.
- **Build a CRUD command/query per table (44 handlers).** Rejected:
  CLAUDE.md §9 scopes Phase 2 to "core commands/queries," and the atlas
  itself names the real ones (C.1.8) — building the rest speculatively
  before any consumer needs them repeats the "reactor internals" mistake
  ADR-003 already flagged, just at the Application layer instead of
  Domain.

## Evidence required

- Domain unit tests, no persistence, for all eleven entities' creation
  validation and the five real behaviors (`AppSetting.UpdateValue`,
  `FeatureFlag.Enable`/`Disable`, `Localization.UpdateValue`,
  `Version.MarkCurrent`/`MarkNotCurrent`).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.CorePlatform.Infrastructure`, targeting the `CorePlatform` SQL
  schema, independent migration history from `ReactorFleet`/
  `AlarmManagement`.
- Component tests against real LocalDB for the five Application-layer
  operations.
- `Nexus1.ArchitectureTests` passing with `Nexus1.CorePlatform.*` composed
  into `Nexus1.ModularRuntime` and correctly classified by the existing
  dependency-law tests.
