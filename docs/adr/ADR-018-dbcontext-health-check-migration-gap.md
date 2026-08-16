# ADR-018: `DbContextHealthCheck<T>` must verify migrations, not just connectivity

## Status

Accepted. Defect fixed, proven, and re-verified against every existing
context.

## Context

While independently re-checking the foundation Organization (Phase 2,
sector 3) builds on — prompted by a direct question about SSMS's table
list not showing `CorePlatform.*` tables in `AlarmManagementDb` — `sqlcmd`
confirmed the CorePlatform migration
(`20260816114650_InitialCorePlatformSchema`) had never actually been
applied to the live database, despite the original CorePlatform evidence
report's own claim of a verified `200 Healthy` real host at the time (see
`artifacts/evidence/2026-08-19-organization-phase2-sector3.md`'s "Owned"
section for the full finding).

The root defect is in `Nexus1.ServiceDefaults`'s `DbContextHealthCheck<T>`
itself, not anything specific to CorePlatform:

```csharp
var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(...);
```

`CanConnectAsync()` proves the target *database* is reachable. It says
nothing about whether *this context's own schema* exists inside that
database. For a context with its own dedicated physical database
(`RootCauseDb`, `AuditDb`, `ComplianceDb`, `ReportingDb`, `SecurityDb`,
`OrganizationDb`), an unmigrated database is usually also unreachable
(nothing created it), so the old check's failure mode happened to be the
honest one — this is exactly what caught the `SecurityDb` gap in the
Security sector's own evidence report. But for any context **sharing**
another context's physical database (`CorePlatform` and `ReactorFleet`
both live in `AlarmManagementDb`, per ADR-006/ADR-015), the shared
database is reachable as soon as *any* co-tenant context's migration has
run — so a context whose *own* migration was simply never applied reports
a false `Healthy`. This is strictly worse than a loud failure: it is a
health check that actively hides the exact class of defect it exists to
catch.

This is a defect in shared building-block code, not a one-off. It has
been silently present since `DbContextHealthCheck<T>` was introduced
(ADR-007/the Hosts step) and applies to every context that has ever
registered it: `ReactorFleet`, `AlarmManagement`, `RootCause`, `Audit`,
`Compliance`, `Reporting`, `CorePlatform`, `Security`, `Organization` —
nine registrations across two hosts (`Nexus1.ModularRuntime`,
`Nexus1.RootCause.Host`). Only `CorePlatform` happened to actually be
caught in the bad state; the other eight were re-verified below, not
assumed innocent.

## Decision

### Fix: check `GetPendingMigrationsAsync()` after `CanConnectAsync()`

```csharp
var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
if (!canConnect) return HealthCheckResult.Unhealthy(...);

var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
return pendingMigrations.Count == 0
    ? HealthCheckResult.Healthy()
    : HealthCheckResult.Unhealthy($"...missing {pendingMigrations.Count} migration(s): ...");
```

`GetPendingMigrationsAsync()` reads the target database's own
`__EFMigrationsHistory` table for **this specific `TContext`** and
compares it against the migrations compiled into that context's assembly
— exactly the check that would have caught CorePlatform's gap
immediately: `AlarmManagementDb` is reachable, but
`CorePlatformDbContext`'s own migration would show as pending. The check
stays generic and reusable through the existing `TContext` generic
parameter — no per-context special-casing, no new configuration, every
context gets the stronger check automatically through the same
`.AddCheck<DbContextHealthCheck<TContext>>(...)` registration already in
place.

**New dependency, recorded explicitly**: `GetPendingMigrationsAsync` is a
relational-only EF Core API, defined in
`Microsoft.EntityFrameworkCore.Relational` — a package
`Nexus1.ServiceDefaults` did not previously reference (it only referenced
the provider-agnostic `Microsoft.EntityFrameworkCore` core package, to
stay theoretically provider-neutral). Added
`Microsoft.EntityFrameworkCore.Relational` at the same centrally-pinned
`8.0.11` version already used by `Microsoft.EntityFrameworkCore`/
`Microsoft.EntityFrameworkCore.SqlServer` everywhere else in this
solution — not a new package family, just an explicit reference to an
assembly that was already being pulled in transitively by every real
context's own `.SqlServer` reference. Every context this project has ever
built or plans to build under Phase 2 targets SQL Server, so this does
not cross an abstraction boundary the project actually relies on.

### Not done: distinguishing "unreachable" from "reachable but unmigrated" as different health states

Both failure modes return `Unhealthy` with a distinguishing message in
`HealthCheckResult.Description`, not a different `HealthStatus`. `/health/
ready`'s `MapHealthChecks` (ADR-007) only surfaces the aggregate status,
not per-check descriptions, in its default response writer — a richer
per-check JSON response is a real, separate feature (structured health
reporting) this ADR does not add, since nothing in this project's own
Phase 2 scope currently reads it. The description string is still useful
for anyone reading server logs or querying the check directly in a test
(as this ADR's own regression test does), which is why it is worded
specifically rather than reused verbatim from the connectivity case.

## Verification performed

- **New regression test**, `Nexus1.ServiceDefaults.ComponentTests`
  (previously nonexistent — `DbContextHealthCheck<T>` had no test coverage
  at all before this ADR, itself worth naming: shared building-block code
  had less test discipline applied to it than any single context's own
  code). A minimal, standalone `HealthCheckTestDbContext` (decoupled from
  every business context, on purpose, so this test doesn't pull in the
  whole solution) proves three real scenarios against real LocalDB, no
  mocks:
  1. Database does not exist → `Unhealthy` (unchanged behavior).
  2. **Database exists and is reachable, but this context's migration was
     never applied** (created via `EnsureCreatedAsync`, bypassing
     migrations entirely — the same effective end state as CorePlatform's
     real gap) → **`Unhealthy`**, proving the exact failure mode that used
     to pass silently no longer does.
  3. Database exists and the migration was applied via `MigrateAsync` →
     `Healthy`.

  3/3 passing.

- **Full solution regression**: 409/409 passing (406 before this step +
  3 new), zero change to any existing context's test count — the
  strengthened check does not produce new false negatives for any
  already-correctly-migrated context.

- **Real host re-verification, all nine registrations, not assumed from
  the passing test suite**: built and ran the actual
  `Nexus1.ModularRuntime.dll`, confirmed `GET /health/ready` returns
  `200 Healthy` — covering `reactorfleet-db`, `coreplatform-db`,
  `alarmmanagement-db`, `audit-db`, `compliance-db`, `reporting-db`,
  `security-db`, `organization-db` (8 checks) with the strengthened
  pending-migrations logic actually executing against each context's real
  persistent database. Separately built and ran the actual
  `Nexus1.RootCause.Host.dll` (its own independent host, own `rootcause-db`
  check, per ADR-007's two-host topology), confirmed `200 Healthy` there
  too. All nine were genuinely re-checked, not inferred from the fact that
  CorePlatform's own fix already worked.

## Consequences

- Every future context built in the remaining 8 Phase 2 sectors
  (Instrumentation onward) inherits the stronger check automatically —
  no repeated work per sector, and no repeat of the CorePlatform class of
  silent gap for any context sharing a physical database with another.
- `Nexus1.ServiceDefaults` now has a direct `Microsoft.EntityFrameworkCore.
  Relational` package reference and, for the first time, its own test
  project (`Nexus1.ServiceDefaults.ComponentTests`).
- The health check's `Description` string on failure now distinguishes
  "cannot connect" from "reachable but missing N migration(s)" — useful
  for whoever reads logs or a future structured health-reporting surface,
  though nothing currently parses it programmatically.

## Rejected alternatives

- **Fix only CorePlatform's own registration/wiring, treat it as a
  one-off.** Rejected: the defect is in the shared `DbContextHealthCheck<T>`
  class itself; every context using it was equally exposed. Fixing one
  registration would have left the other eight (and every future sector)
  silently vulnerable to the identical failure mode the moment any of them
  ever shares a physical database with a co-tenant whose migration runs
  first.
- **Use `EnsureCreatedAsync` instead of real migrations as the health
  signal.** Rejected: this project's entire persistence discipline is
  migration-based (`dotnet ef migrations add`, reviewed, applied via
  `dotnet ef database update`) — `EnsureCreated` is a different,
  incompatible model EF Core explicitly warns against mixing with
  migrations, and adopting it here would contradict every existing
  context's own persistence ADR.
- **Report a distinct `HealthStatus.Degraded` for "reachable but
  unmigrated," instead of `Unhealthy`.** Rejected: `Degraded` in ASP.NET
  Core's health check model conventionally means "impaired but usable" —
  a context missing its own tables cannot serve any real request, which
  is `Unhealthy` by definition, not a degraded-but-functioning state.

## Evidence required

Captured directly in this ADR rather than a separate evidence report,
since this is infrastructure shared across every sector rather than a new
sector of its own: the three-scenario regression test (3/3), the full
solution regression (409/409), and the real dual-host re-verification
above.
