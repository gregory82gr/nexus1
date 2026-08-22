# ADR-028: A scoped SQL login as the primary defense for passport-only cross-context references

## Status

Accepted.

## Context

Every sector since Instrumentation (ADR-019) has used two different
mechanisms for a cross-context reference, chosen by whether the target
lives in the same physical database:

- **Same physical database** (e.g. `Robotics.Unit` → `ReactorFleet.Unit`,
  both in `AlarmManagementDb`): a real SQL `FOREIGN KEY` via a local
  `ExcludeFromMigrations` shadow entity. The database itself enforces
  referential integrity.
- **Different physical database** (e.g. `EmergencyPreparedness.
  EmergencyPlan.SiteId` → `Organization.Site`, in `OrganizationDb`;
  `AdvisoryRecommendation`-adjacent `*UserId` → `Security.
  ApplicationUser`, in `SecurityDb`): a plain passport `int`/`int?`
  column with no enforced constraint. SQL Server cannot declare a
  cross-database `FOREIGN KEY`, so nothing at the database level stops
  this column from holding a value that doesn't exist on the other side.

This second pattern recurs across nearly every sector built in Phase 2 —
`Organization`→`Security`, `RadiationMonitoring`→`CorePlatform`/
`Organization`/`Security`, `EmergencyPreparedness`→`Organization`/
`Security`, `ReinforcementLearning`→`Security` — and today its only
protection is application-layer discipline: the Application layer's own
command handlers are trusted to only ever write a passport value that
came from a real lookup elsewhere, and nothing else enforces that trust.

Three ways to strengthen this were discussed:

1. **A scoped database login** — restrict what the running application's
   own database credentials can do, independent of any C# code path.
2. **An existence check at write time** — an Application-layer or
   Infrastructure-layer check that queries the owning context (in-process,
   since it's the same host) before accepting a passport value.
3. **A periodic reconciliation job** — a background process that
   periodically scans passport columns for values that no longer resolve
   anywhere and reports them.

Option 1 was identified as effectively free architecturally: it changes
zero application code, adds no runtime query, and doesn't touch the
Domain/Application/Infrastructure layering this project has spent eleven
sectors keeping clean. It also matches something already true of this
codebase's own discipline elsewhere — `Nexus1.ArchitectureTests` already
enforces dependency-direction rules structurally rather than trusting
convention; a scoped login does the same thing one layer down, at the
database boundary rather than the assembly-reference boundary.

## Decision

**Implement Option 1 now: a dedicated, schema-scoped SQL login,
`nexus1_app`, replacing the developer's own Windows-integrated
(`Trusted_Connection=True`) connection in both hosts' `appsettings.json`.**
Full setup, verification commands, and the exact `GRANT` statements are in
`docs/runbooks/local-scoped-sql-login.md`.

### What this login can and cannot do — verified directly, not assumed

- **Not sysadmin, not `db_owner`.** `IS_SRVROLEMEMBER('sysadmin')` and
  `IS_MEMBER('db_owner')` both return `0` when connected as `nexus1_app`.
- **No DDL of any kind.** `CREATE TABLE` fails with `Msg 262: CREATE
  TABLE permission denied in database`. Schema changes remain the
  developer's own responsibility via `dotnet ef database update`, which
  uses a separate, unaffected `Trusted_Connection` in each
  `*DbContextFactory.cs` — this login only ever runs at application
  runtime, never at migration time.
- **`SELECT`/`INSERT`/`UPDATE`/`DELETE` on exactly the schemas each
  database's own contexts write** — granted per schema (`GRANT ... ON
  SCHEMA::<Name>`), not via `db_datawriter` (which would apply
  database-wide, including any future schema added without an explicit
  grant decision). `SELECT`-only on `dbo`, where every
  `__EFMigrationsHistory_*` table lives — needed for `DbContextHealthCheck<T>`'s
  pending-migration check, never written by the running app.
- **No access to any database it has no `USER` mapping in.** A login
  without a database-level user cannot connect to that database at all —
  the standard SQL Server model, no exception carved out here.

### What this actually defends against

This does **not** make a cross-database passport reference type-checked
or referentially enforced the way a same-database `FOREIGN KEY` is — no
mechanism at the SQL-permission level can validate that
`EmergencyPlan.SiteId = 47` corresponds to a real row in a different
physical database. What it *does* provide, and what makes it worth doing
regardless: **the blast radius of any bug, typo, or future careless
change is now bounded by the database's own permission model, not merely
by every developer remembering the bounded-context rule correctly every
time.** A defect that today would silently succeed — a stray migration
accidentally pointed at the wrong database, a copy-pasted connection
string reused where it shouldn't be, a future feature branch that
forgets a context boundary and tries to `INSERT` into another context's
table directly instead of going through a passport — now fails loudly
with a permission error instead of either silently corrupting data or
silently succeeding somewhere it structurally shouldn't. This is the same
"loud failure over silent corruption" instinct behind
`DbContextHealthCheck<T>`'s own strengthening (ADR-018) and the
"Inconclusive, never a silent Pass" discipline this project applies
everywhere else — applied here to the database permission boundary
instead of a health check or a verdict.

### The other two options remain deferred, not rejected

- **Existence check at write time.** Deferred — it would add a real
  runtime query (and, for a cross-database reference, either a second
  `DbContext`/connection the writing context's own Infrastructure layer
  would need to open, or an application-layer service call this
  project's "no messaging until there's a real reason" discipline
  (ADR-027, ADR-026) would also have to weigh) for every write that
  touches a passport column, with no present defect this project has
  actually hit to justify it. Revisit if a real cross-database data-
  integrity defect is ever found in practice (not hypothesized) that the
  scoped login alone did not prevent or surface.
- **Periodic reconciliation job.** Deferred — a genuinely useful
  detection mechanism for *existing* drift, but it is itself a new piece
  of infrastructure (a scheduled job, its own store for findings, its own
  alerting) this project has no present operational need for; nothing
  today runs long enough or accumulates enough data for stale passport
  values to be a realistic concern yet. Revisit once a Phase 2 context
  has run long enough in a real (non-LocalDB-dev) environment that
  reference drift becomes a plausible, not merely theoretical, risk.

Both remain named, not silently dropped — the same "record the deferred
option and its reversal condition" discipline this project has used for
MediatR, the Query BFF, and the RL advisory branch, applied here to
security/data-integrity hardening options instead of a feature scope
decision.

## Consequences

- `src/Hosts/Nexus1.ModularRuntime/appsettings.json` and
  `src/Hosts/Nexus1.RootCause.Host/appsettings.json` now use
  `User Id=nexus1_app;Password=...` instead of `Trusted_Connection=True`
  for every connection string. `appsettings.Development.json` in both
  hosts carries no connection-string overrides, so this is the sole
  runtime connection configuration.
- No `*DbContextFactory.cs` was touched — every one of them keeps its own
  hardcoded `Trusted_Connection=True` design-time connection string,
  preserving the existing separation between "who can change schema"
  (the developer) and "what the running app can do" (now `nexus1_app`).
- No C# code changed. No new project, no new dependency, no
  `Nexus1.ArchitectureTests` rule. This really was the "free" option.
- A new local setup step (`docs/runbooks/local-scoped-sql-login.md`)
  joins RabbitMQ and the OTel collector as something a fresh environment
  must stand up before the real hosts can start — the login must exist
  before either host's `appsettings.json` connection strings will work.
- Adding a new schema to an existing shared database, or a new database
  entirely, now requires one additional step per future sector: add the
  matching `GRANT` to the runbook's script and re-run it. A small,
  named, mechanical addition — not a redesign, and not something any
  future sector's own ADR needs to re-litigate.

## Rejected alternatives

- **`db_datareader`/`db_datawriter` database roles instead of per-schema
  `GRANT`s.** Considered — rejected because those roles apply to every
  schema in a database, including any added later without an explicit
  decision. Per-schema grants mean a future schema starts with *no*
  access until someone deliberately grants it — fail-closed by
  construction, matching this project's own "fail closed on an unknown
  profile" precedent (`From_Services_To_Runtime` Ch.36's `SliceProfile`
  gate) rather than fail-open.
- **A distinct login per context** (e.g. `nexus1_robotics_app`,
  `nexus1_reactorfleet_app`) instead of one shared `nexus1_app` login
  used across all contexts sharing a database. Rejected for this pass —
  since `Nexus1.ModularRuntime` composes all eleven Phase 2 contexts (plus
  ReactorFleet/CorePlatform/AlarmManagement) into one process using one
  connection string per *database*, not per *context*, per-context
  logins would require either per-context connection strings (a larger,
  riskier change touching every `ServiceCollectionExtensions.
  Add*Infrastructure` call) or would provide no real additional isolation
  beyond what per-schema grants already give, since every context in a
  shared database is reached through the same host process regardless.
  Worth revisiting only if a future sector's own persistence decision
  ever moves to one-connection-string-per-context.

## Evidence required

- `IS_SRVROLEMEMBER('sysadmin', 'nexus1_app')` and `IS_MEMBER('db_owner')`
  (connected as `nexus1_app`) both return `0`, checked directly.
- `CREATE TABLE` as `nexus1_app` fails with a permission error, checked
  directly.
- A real `INSERT`/`SELECT` against an owned schema succeeds as
  `nexus1_app` (a data-constraint error on an incomplete test row counts
  as proof the permission check itself passed, since SQL Server only
  reaches the constraint check after the permission check succeeds).
- Full regression suite green with the new connection strings in place
  (869/869 — unaffected by this change, since component tests use their
  own throwaway per-test databases under the developer's own credentials,
  not `appsettings.json`).
- Both real hosts (`Nexus1.ModularRuntime`, `Nexus1.RootCause.Host`)
  started against the real LocalDB instance with the new connection
  strings; `GET /health/ready` returns `200 Healthy` on both; zero
  `Unhealthy` log lines; `sys.dm_exec_sessions` confirms active sessions
  under `login_name = 'nexus1_app'` against every database each host
  actually uses (`AlarmManagementDb`/`AuditDb`/`ComplianceDb`/
  `OrganizationDb`/`ReportingDb`/`SecurityDb` for `ModularRuntime`;
  `RootCauseDb` for `RootCause.Host`) — not merely a `200` response, but
  confirmation the new login was genuinely the one used, not a silent
  fallback.
