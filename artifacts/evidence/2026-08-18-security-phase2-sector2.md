# Evidence: Security (Phase 2, sector 2 of 11) — Domain, Application, Infrastructure

Date: 2026-08-18
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-016-security-phase2-scope-and-persistence.md`. This report is
the real proof: nine of twenty-nine atlas tables modeled in Domain (the
RBAC/identity core the atlas itself calls the "authorization backbone"), EF
Core persistence against a new, genuinely separate physical database
(`SecurityDb`), the five Application-layer operations the atlas's own
verification query and table descriptions name as real behavior, composed
into `Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still green, and a
real host startup with a working `security-db` health check.

## Automated regression: 294/294 passing (was 249/249 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.CorePlatform.UnitTests                     46/46 passed
Nexus1.Security.UnitTests                         31/31 passed  (new)
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed
Nexus1.AlarmManagement.ComponentTests              19/19 passed
Nexus1.Audit.ComponentTests                       13/13 passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   16/16 passed
Nexus1.CorePlatform.ComponentTests                 9/9  passed
Nexus1.Security.ComponentTests                    14/14 passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix C.2: **twenty-nine tables** (eight
  lookup, twenty-one substantive) — identity, authorization (two separate
  mechanisms: RBAC and a policy layer), runtime session/login state,
  service/token issuance, and governance access reviews.
- `From_Domain_to_Twin`'s "Generic domains" chapter classifies Security
  explicitly as generic: *"not where NEXUS-1 should invent unusual domain
  language... Use proven patterns and framework support... Keep it boring,
  stable, and explicit."* No aggregate design exists for Security in that
  book at all.
- The atlas's own master dependency map (C.0.10): only `AlarmManagement`,
  `EventManagement`, `Maintenance` depend on Security, and only on
  `Security.ApplicationUser` — *"to record who acknowledged, created,
  approved, exported, closed, or changed something."*
- The atlas's own C.2.8 "authorization backbone" verification query joins
  exactly `ApplicationUser → UserRole → ApplicationRole → RolePermission →
  Permission → PermissionCategory` — RBAC only, no policy table touched.

Full reasoning for what was built vs. deliberately not built is in
ADR-016; not repeated here.

## Domain layer — nine tables, the RBAC/identity core

`Nexus1.Security.Domain`: `UserStatus`, `RoleType`, `PermissionCategory`
(lookups), `ApplicationUser`, `ApplicationRole`, `UserRole`, `Permission`,
`RolePermission`, `UserPreference`. Two entities are composite-keyed plain
classes rather than `Entity<TId>` aggregates (`UserRole`, `RolePermission`)
— the same pattern this codebase's own `InboxReceipt`/`RetryTicket` already
use for tables with no single surrogate identity. Real behaviors matched to
what the atlas actually describes, nothing invented beyond it:
`ApplicationUser.Lock`/`Unlock` (real `LockoutEnabled`/`LockoutEndUtc`/
`AccessFailedCount` columns), `UserRole`/`RolePermission` grant/revoke with
their real `ExpiresAtUtc` checks, `UserPreference.Update`.
`ApplicationUser.PasswordHash` is an opaque, caller-supplied nullable
string — this project does not implement credential hashing (no login
surface exists this phase). `ApplicationUser.IsServiceAccount` is modeled
and flagged (not wired in) as the real hook for Phase 1's `"system:..."`
string literals (`AlarmFloodMessageHandler`'s `OpenedBy`,
`CloseAnalysisCommandHandler`'s `ClosedBy`) — no Phase 1 refactor was made,
per instruction, since no small obviously-correct hook exists (it would add
a cross-context dependency Phase 1 never had). 31 unit tests: creation
validation for all nine entities plus every real behavior (lock/unlock,
grant/revoke, expiry checks, preference update, role hierarchy).

## A genuine correction caught during this step, not after

ADR-015 authorized real cross-schema SQL foreign keys for sectors consuming
CorePlatform reference data, written on the (unstated, unchecked)
assumption that the consumer shares CorePlatform's physical database.
While drafting `UserPreference` (which consumes `CorePlatform.Language`/
`CorePlatform.TimeZone`), and independently deciding Security needs its own
physical database (below), the two decisions were checked against each
other before any migration was generated: **a real SQL `FOREIGN KEY` cannot
span two different physical databases.** The moment Security was given its
own `SecurityDb`, ADR-015's real-FK exception stopped applying to it.
`UserPreference.LanguageId`/`TimeZoneId` are plain passport ints with no
enforced constraint — the correct, checked answer, not the one first
drafted. ADR-016 records this correction directly (both the ADR text and
`UserPreference`'s own doc comments were fixed before the migration was
generated), matching this project's "verification convention": a claim that
prior machinery generalizes must be checked against the actual facts before
being asserted, not assumed.

## EF Core Infrastructure — own physical database, nine configurations, one reviewed migration

`Nexus1.Security.Infrastructure`: `SecurityDbContext` targeting a **new,
genuinely separate physical database (`SecurityDb`)** — not
`AlarmManagementDb`, unlike CorePlatform/ReactorFleet. `ApplicationUser`
genuinely holds credential-adjacent columns (`PasswordHash`,
`SecurityStamp`, `ConcurrencyStamp`, lockout state) even in this trimmed
nine-table scope; ADR-016 records this as a data-*sensitivity* decision,
independent of and distinct from the deployment-*topology* reasoning
ADR-006/ADR-015 used for ReactorFleet/CorePlatform. One
`IEntityTypeConfiguration` per entity, matching the atlas's real column
types/lengths/named constraints, including its `CK_Security_ApplicationUser_AccessFailedCount
>= 0` check constraint and `Restrict` (not EF's default `Cascade`) on every
internal FK — the atlas's own DDL specifies no `ON DELETE` clause on any of
Security's internal FKs, so `NO ACTION`/`Restrict` is the faithful mapping,
the same discrepancy-catch already applied to CorePlatform's FKs in
ADR-015's own evidence report.

Migration: `20260816124526_InitialSecuritySchema`, `Security` schema, own
migration-history table (`__EFMigrationsHistory_Security`), reviewed for
readable names before being accepted.

## Application layer — the atlas's own authorization-backbone query plus real per-table behaviors

Five operations, matched to what the atlas actually names as real
behavior, not CRUD-per-table:

- `GetEffectivePermissionsForUserQuery` — the atlas's own C.2.8
  verification query, verbatim: active-role, active-grant, unexpired
  effective permissions for a user, including explicit denies (the query
  returns `IsGranted`, it does not filter on it — matching the atlas's own
  `SELECT ... rp.IsGranted` shape, proven directly by a dedicated
  component test).
- `AssignRoleToUserCommand` — `UserRole`'s defining behavior.
- `GrantPermissionToRoleCommand` — `RolePermission`'s defining behavior.
- `LockUserCommand`/`UnlockUserCommand` — `ApplicationUser`'s real lockout
  state.
- `UpdateUserPreferenceCommand` — `UserPreference`'s defining behavior,
  upsert (first call creates the one-row-per-user record, later calls
  update it).

14 component tests against real LocalDB, including three that directly
exercise the effective-permissions query's filtering rules: an active
unexpired grant is returned, an expired role assignment excludes its
permissions entirely, and a revoked role-permission grant is still
*returned* (present, `IsGranted = false`) rather than silently dropped —
proving the atlas's "explicit deny is visible, not hidden" shape, not just
assuming it.

## Composed into Nexus1.ModularRuntime

`AddSecurityApplication()`/`AddSecurityInfrastructure(securityConnectionString)`
wired into `Program.cs` with a new `SecurityDb` connection string (own
entry in `appsettings.json`, distinct from every other context's), plus a
`security-db` entry in the health-check chain. `Nexus1.ArchitectureTests`
needed zero code changes — `Nexus1.Security.Domain`/`.Application`/
`.Infrastructure` were classified and enforced automatically by the
existing naming-convention rule. 7/7 passing, confirming no illegal
cross-context references (in particular, `UserPreference`'s
`CorePlatform.Language`/`TimeZone` references stay at the passport-int
level in code, matching the corrected persistence decision above — there
is no `Nexus1.CorePlatform.Domain` reference from `Nexus1.Security.Domain`).

## Owned

- **A real gap, not a code defect, found by actually starting the host**:
  the first real-host run returned `503 Unhealthy` from `/health/ready`.
  Unlike CorePlatform (which shared `AlarmManagementDb`, already created
  and migrated by earlier Phase 1 steps), `SecurityDb` had never been
  physically created on this machine's LocalDB instance — only the
  throwaway per-test databases component tests create existed. `DbContextHealthCheck<TContext>`'s
  `CanConnectAsync()` genuinely fails when the named database in the
  connection string doesn't exist yet, which is correct, honest behavior,
  not a bug in the health check. Fixed by running `dotnet ef database
  update` once against `Nexus1.Security.Infrastructure` to create and
  migrate `SecurityDb` for real, then re-verified `200 Healthy`. This is
  exactly the "verify a real host, don't just trust the build" checkpoint
  discipline doing its job — a build-clean, architecture-tests-green state
  can still hide a genuinely unusable host.
- **A harness-only EF Core translation limit, not a product bug**:
  `EfEffectivePermissionFinder`'s original query chained `.Distinct()`
  directly onto a five-table-join LINQ query projected into the
  `EffectivePermissionDto` record; EF Core's SQL Server provider could not
  translate `.Distinct()` over that projection shape
  (`InvalidOperationException` at query-compile time, not a wrong-result
  bug — caught immediately by the first component test run, before being
  reported as working). Fixed by materializing the joined rows first
  (`ToListAsync()`), then deduping/ordering in memory — a negligible cost
  for a per-user permission list, and the same "materialize, then finish
  client-side" pattern already used elsewhere in this codebase for
  shapes EF's translator can't handle.
- No `src/` files outside the new `Nexus1.Security.*` projects and
  `Nexus1.ModularRuntime`'s composition root (csproj, `Program.cs`,
  `appsettings.json`) were touched.
- `SecurityDb` was left in place after evidence capture — harmless local
  dev state, same reasoning as every prior step.

## Scope explicitly not covered by this step

Per ADR-016, twenty of the atlas's twenty-nine Security tables remain
unbuilt, in five named groups: the policy/ABAC authorization layer
(`Policy`, `PolicyType`, `PolicyPermission`, `RolePolicy`,
`UserPolicyAssignment`, `UserPermissionOverride`); ASP.NET Identity
extension points with no described consumer (`UserClaim`, `RoleClaim`,
`ExternalLogin`); session/login-attempt tracking (`UserSession`,
`LoginAttempt`, `SessionStatus`, `LoginResult`) — no HTTP authentication
surface exists in Phase 2 to ever populate them; machine-client/token
issuance (`ApiClient`, `ApiClientSecret`, `SecurityToken`, `TokenType`) —
same reasoning; and the periodic access-review governance workflow
(`AccessReview`, `AccessReviewItem`, `AccessReviewStatus`). None of these
are silently dropped — each is named in ADR-016 with the specific reason it
was deferred, distinct from the others, not a single blanket "out of
scope."

This closes Security, sector 2 of 11 in Phase 2. Organization is next per
CLAUDE.md §9's ordering.
