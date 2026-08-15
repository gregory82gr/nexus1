# Evidence: EF Core persistence for ReactorFleet, AlarmManagement, RootCause (§5 step 3)

Date: 2026-08-15
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11 (packages and
local `dotnet-ef` tool pinned to match — `.config/dotnet-tools.json`), SQL
Server LocalDB `mssqllocaldb` (already present on this machine).

## Built

`dotnet build Nexus1.Runtime.sln` — all 25 projects (the 22 from the skeleton
plus `Nexus1.Contracts.ReactorFleet`, now with real `Infrastructure`
persistence content in all three contexts) compile clean:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Migrations generated

| Context | Migration | Schema |
|---|---|---|
| ReactorFleet | `InitialReactorFleetSchema` | `ReactorFleet` |
| AlarmManagement | `InitialAlarmManagementSchema` | `AlarmManagement` |
| RootCause | `InitialRootCauseSchema` | `RootCause` |

Each migration was reviewed by hand before being accepted. One real defect
was caught and fixed during review, not after: the first generated RootCause
migration had the `AnalysisHypothesis.RootCauseAnalysisId` and
`HypothesisEvidence.AnalysisHypothesisId` shadow foreign keys as **nullable**
(`bigint`/`int` `NULL`), because the Fluent API relationships hadn't been
marked `.IsRequired()`. Since a hypothesis or evidence row can't exist
without its parent in this aggregate, the migration was removed
(`dotnet ef migrations remove`), the configuration fixed, and regenerated —
the corrected migration shows both FK columns as `nullable: false`.

## Migrations applied to a real database — not just generated

`dotnet ef database update` was run for all three contexts against the
LocalDB instance already present on this machine (`mssqllocaldb`), not
skipped as "can't verify":

```
ReactorFleet:      Applying migration '...InitialReactorFleetSchema'. Done.
AlarmManagement:   Applying migration '...InitialAlarmManagementSchema'. Done.
RootCause:         Applying migration '...InitialRootCauseSchema'. Done.
```

Verified against the real databases with `sqlcmd`, confirming ADR-006's
design took effect exactly as decided, not just as configured in code:

- `sys.databases` shows exactly two physical databases:
  **`AlarmManagementDb`** and **`RootCauseDb`** — ReactorFleet did not get
  its own database.
- `AlarmManagementDb` contains **two schemas**, `AlarmManagement` (3 tables)
  and `ReactorFleet` (2 tables), each with its **own** migrations-history
  table (`__EFMigrationsHistory_AlarmManagement`,
  `__EFMigrationsHistory_ReactorFleet`) — confirming independent migration
  histories despite the shared physical database.
- `AlarmManagementDb` has **zero foreign keys** (`sys.foreign_keys` count =
  0) — confirming no cross-schema FK was introduced between `ReactorFleet.*`
  and `AlarmManagement.*` at the database level, matching the passport-only
  discipline already enforced in code.
- `RootCauseDb` has its own separate `RootCause` schema (3 tables) and its
  own migrations-history table, with two foreign keys
  (`AnalysisHypothesis`→`RootCauseAnalysis`,
  `HypothesisEvidence`→`AnalysisHypothesis`), both `NO_ACTION` — the SQL
  Server realization of `DeleteBehavior.Restrict`.

Both databases were dropped after verification (`dotnet ef database drop
--force`) — they were created only to prove the migrations actually apply
and produce the intended structure, not to persist as local dev state.

## Regression check

`dotnet test Nexus1.Runtime.sln` after all persistence work: still **44
tests passing** (9 RootCause + 12 ReactorFleet + 16 AlarmManagement + 7
architecture), unchanged from before this step — confirming the persistence
layer was added without touching domain-layer behavior.

## Owned

- Mapping decisions not dictated by either source, made explicitly rather
  than silently: strongly-typed IDs use `ValueGeneratedNever()` (domain
  factories always require a caller-supplied id, diverging from the atlas's
  literal `IDENTITY` declaration — noted in code comments, not hidden);
  enums (`AlarmSeverity`, `AlarmState`, `AlarmFloodStatus`, `AnalysisStatus`,
  `HypothesisStatus`) persist as `nvarchar` via `HasConversion<string>()`
  rather than as FKs to the atlas's full lookup tables, consistent with
  ADR-004/ADR-005's decision not to model those lookup tables yet;
  `AlarmFlood.MemberAlarmEventIds` is `Ignore`d, matching ADR-004's explicit
  deferral of the `AlarmFloodMember` join table.
- Not verified in this environment: connection pooling, concurrency
  (`RowVersion`/optimistic concurrency tokens aren't mapped yet — the atlas's
  lookup tables have them, the substantive tables built so far don't need
  them until real concurrent writers exist), and production connection
  string management (the `IDesignTimeDbContextFactory` implementations use a
  hardcoded LocalDB connection string, explicitly for design-time tooling
  only — real runtime wiring is Host-layer work, §5 step 6, not built yet).
