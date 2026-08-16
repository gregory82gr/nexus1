# ADR-016: Security (Phase 2, sector 2) — scope, domain shape, and persistence

## Status

Accepted.

## Context

Phase 2's second sector. Verified directly against the source material
before writing any code:

- `From_Schema_to_System` Appendix C.2: **twenty-nine tables** (eight lookup,
  twenty-one substantive) — identity (`ApplicationUser`, `ApplicationRole`,
  `UserRole`, `UserClaim`, `RoleClaim`, `ExternalLogin`), authorization
  (`Permission`, `Policy`, `RolePermission`, `PolicyPermission`,
  `RolePolicy`, `UserPolicyAssignment`, `UserPermissionOverride`),
  preference (`UserPreference`), runtime auth state (`UserSession`,
  `LoginAttempt`), service/token (`ApiClient`, `ApiClientSecret`,
  `SecurityToken`), and governance (`AccessReview`, `AccessReviewItem`),
  plus eight typed lookup tables backing them.
- `From_Domain_to_Twin`'s Chapter "Generic domains" classifies Security
  explicitly: *"Generic domains are capabilities many systems need:
  identity, authorization, audit storage, localization, feature flags,
  reporting exports, and general settings... they are not where NEXUS-1
  should invent unusual domain language unless there is a clear reason."*
  Its own guidance column for Security/generic domains: *"Use proven
  patterns and framework support... Standardize it and make it hard to
  bypass... Keep it boring, stable, and explicit."* No aggregate design or
  code sample exists for Security anywhere in that book — CorePlatform's
  `EngineeringUnit` bounded-context example is the closest analogue, and it
  wasn't specific to Security either. This is confirmation, not a gap:
  Security's domain shape should be a faithful, boring RBAC/identity model,
  not an invented workflow.
- The atlas's own master dependency map (C.0.10) names which sectors
  actually reference Security: `AlarmManagement`, `EventManagement`, and
  `Maintenance`. The atlas is explicit about *why*: **"Several later
  sectors will depend on `Security.ApplicationUser` to record who
  acknowledged, created, approved, exported, closed, or changed
  something."** No sector's dependency is described in terms of sessions,
  tokens, API clients, or access-review campaigns — those are named and
  well-specified in the atlas, but nothing in this project's own
  dependency graph consumes them.
- The atlas's own "useful verification query" for Security (C.2.8, "verify
  that the sector behaves like an authorization backbone") is *effective
  permissions for one user from active roles* — it joins exactly
  `ApplicationUser` → `UserRole` → `ApplicationRole` → `RolePermission` →
  `Permission` → `PermissionCategory`. It does not touch `Policy` or any
  policy-assignment table. The atlas's own definition of "the
  authorization backbone" is the RBAC core, not the policy layer on top of
  it.
- The atlas's own closing note (C.2.9, "Honest boundary"): *"This schema is
  an enterprise demonstrator security model. It is not a complete
  production identity server... Passwords, secrets, and tokens are
  represented only as hashes; plaintext secret material never belongs in
  the database."*

Two restraint questions, per CLAUDE.md §9 and the same discipline applied
to CorePlatform (ADR-015).

## Decision

### Scope: nine of twenty-nine tables — the RBAC/identity core the atlas itself calls the "authorization backbone"

Unlike CorePlatform (where every table had a named present-or-near-future
consumer and nothing was cut), Security genuinely splits into a load-bearing
core and speculative-for-this-project apparatus, using the atlas's own
signals rather than this project's guess:

**Built** — `UserStatus`, `RoleType`, `PermissionCategory` (lookups),
`ApplicationUser`, `ApplicationRole`, `UserRole`, `Permission`,
`RolePermission`, `UserPreference`.

**Not built, with reasoning per group** (twenty tables):

- **`PolicyType`, `Policy`, `PolicyPermission`, `RolePolicy`,
  `UserPolicyAssignment`, `UserPermissionOverride`** (six tables) — a
  second, more elaborate ABAC-flavored authorization mechanism layered on
  top of RBAC. Zero named consumer anywhere in the atlas's dependency map,
  and the atlas's own "authorization backbone" verification query uses
  only role→permission, confirming RBAC alone is what this project's
  authorization actually needs right now.
- **`UserClaim`, `RoleClaim`, `ExternalLogin`** (three tables) — described
  in the atlas only as ASP.NET Identity "extension point[s]," with no
  behavior or consumer named anywhere. Infrastructure for infrastructure's
  sake at this phase.
- **`UserSession`, `LoginAttempt`** plus their **`SessionStatus`,
  `LoginResult`** lookups (four tables) — real login-flow state, but this
  project has no HTTP authentication surface in Phase 2 (CLAUDE.md §9:
  explicitly no HTTP surface for any of the eleven sectors; ADR-007 already
  deferred the Query BFF in Phase 1) — nothing exists yet that could ever
  create a session or record a login attempt. Building this now would be
  the exact "provisioning infrastructure for a boundary that doesn't exist
  yet" mistake ADR-006 already named and avoided for ReactorFleet's
  database isolation.
- **`ApiClient`, `ApiClientSecret`, `SecurityToken`** plus the
  **`TokenType`** lookup (four tables) — machine-client/token issuance;
  same "no auth surface exists to issue or validate anything against" reasoning.
- **`AccessReview`, `AccessReviewItem`** plus the **`AccessReviewStatus`**
  lookup (three tables) — a periodic governance campaign workflow with no
  described trigger or consumer anywhere in this project.

This is a different restraint reasoning than ADR-003's ReactorFleet cut
(which excluded tables absent everywhere else in the book) and different
again from ADR-015's CorePlatform decision (which kept everything because
every table had a named consumer) — here, every excluded table *is*
well-specified in the atlas, but none has a real consumer this project
either has today or will build in the confirmed Phase 2 order, and adding
the machinery now would be pure speculation the project's own restraint
discipline argues against paying for early.

### Domain shape: boring and faithful, per the book's own instruction

Each of the nine entities gets a `Create` factory enforcing the atlas's
real constraints (`ApplicationRole`'s self-referencing `ParentRoleId`
hierarchy, `UserRole`/`RolePermission`'s `ExpiresAtUtc > AssignedAtUtc`/
`GrantedAtUtc` checks, `ApplicationUser`'s `AccessFailedCount >= 0`,
`UserPreference`'s `Theme IN ('Light','Dark','System')`) plus a small
number of real, atlas-described state transitions — no invented workflow
beyond what the tables' own columns already describe:
`ApplicationUser.Lock`/`Unlock` (the real `LockoutEnabled`/
`LockoutEndUtc`/`AccessFailedCount` columns), `UserRole`/`RolePermission`
grant/revoke, `UserPreference.Update`. `ApplicationUser.PasswordHash` is
modeled as an opaque, caller-supplied nullable string — this project does
not implement credential hashing or verification in this phase (no login
surface exists to call it), matching the atlas's own scope boundary
("represented only as hashes... plaintext secret material never belongs
in the database") without taking on a real authentication-library decision
CLAUDE.md would require an explicit ask for. Same audit-column restraint
as ADR-015: `CreatedBy`/`ModifiedAtUtc`/`ModifiedBy`/`IsDeleted`/
`RowVersion` are not modeled in Domain (no attached behavior), one
`CreatedAtUtc` kept.

`ApplicationUser.IsServiceAccount` is kept and modeled — not because
anything currently sets it, but because it is the atlas-provided,
purpose-built distinction between a human login and a service/automation
identity, which is exactly what Phase 1's ad-hoc `"system:alarm-flood-consumer"`
string literals (`AlarmFloodMessageHandler`'s `OpenedBy` value,
`CloseAnalysisCommandHandler`'s `ClosedBy` caller value) are standing in
for. **Flagged, not acted on**: once Security exists, those Phase 1 string
literals are candidates to eventually resolve to a real
`ApplicationUser.IsServiceAccount = true` row instead of a bare string.
This ADR does not refactor Phase 1 to do that now — there is no small,
obviously-correct hook available (RootCause and AlarmManagement have no
dependency on Security in the atlas's own dependency map, so wiring it in
now would be a new cross-context dependency Phase 1 never had, not a
one-line change) — but it is recorded here so a future session doesn't
have to rediscover the connection.

### Application layer: the atlas's own "authorization backbone" query plus the real per-table behaviors

Five operations, the same "atlas-named, not CRUD-per-table" discipline as
ADR-015:

- `GetEffectivePermissionsForUserQuery` — the atlas's own C.2.8
  verification query, verbatim (active-role, active-grant, unexpired
  permissions for a user).
- `AssignRoleToUserCommand` — `UserRole`'s defining behavior.
- `GrantPermissionToRoleCommand` — `RolePermission`'s defining behavior.
- `LockUserCommand`/`UnlockUserCommand` — `ApplicationUser`'s real
  lockout state, one command pair.
- `UpdateUserPreferenceCommand` — `UserPreference`'s defining behavior,
  consuming `CorePlatform.Language`/`CorePlatform.TimeZone` **by passport
  id only, not a real FK** — see the persistence decision below for why
  this is a genuine downgrade from what ADR-015 authorized, caught before
  the migration was written rather than after.

### Persistence: **own physical database (`SecurityDb`)** — not shared, unlike CorePlatform

ADR-015's reasoning for sharing `AlarmManagementDb` was deployment-topology
driven: no independent deployment exists yet, so DB-per-service isolation
would provision infrastructure for a boundary that doesn't exist. That
reasoning does not mechanically transfer here, and re-checking it rather
than reusing the prior answer matters: **even in this project's trimmed
nine-table scope, `Security.ApplicationUser` genuinely holds
credential-adjacent columns** — `PasswordHash`, `SecurityStamp`,
`ConcurrencyStamp`, lockout state. This is a data-*sensitivity* question,
not a deployment-*topology* question, and the two axes can point different
directions. They do here: Security stays composed in-process into
`Nexus1.ModularRuntime` (same as CorePlatform, no independent deployment),
**but gets its own physical database**, matching the existing precedent
this project already uses for its genuinely separate, independently-owned
contexts (`RootCauseDb`, `AuditDb`, `ComplianceDb`, `ReportingDb` are all
already distinct physical databases on the same local SQL Server instance
— Security joins that group, not `AlarmManagementDb`'s shared-foundation
group).

**Honest limits of this decision, stated plainly**: this is a local LocalDB
demonstrator. A separate `Database=SecurityDb;` connection string on the
same developer machine's SQL Server instance is a structural boundary
(separate schema, separate migration history, separate connection string
ready to point at a genuinely separate protected instance later) — it is
not encryption at rest, not a separate credential vault, and not a claim
that this makes stored password hashes actually secure in this
environment. The isolation is architectural preparation, not a security
control by itself, and this ADR does not claim otherwise.

Own `SecurityDbContext`, own `Security` SQL schema, own migration-history
table (`__EFMigrationsHistory_Security`), same caller-supplied-ID
convention as every other context (`IIdGenerator`, `ValueGeneratedNever()`).

**A consequence of this decision, caught before writing the migration, not
after**: ADR-015 authorized real cross-schema SQL foreign keys for sectors
consuming CorePlatform reference data, on the assumption that the consuming
sector shares CorePlatform's physical database (as `ReactorFleet` already
does). A real `FOREIGN KEY` constraint cannot span two different physical
SQL Server databases — so the moment Security is given its own `SecurityDb`
(this section's own decision), `UserPreference.LanguageId`/`TimeZoneId`
can no longer take the real-FK exception ADR-015 described; they fall back
to plain passport ints with no enforced constraint, the same as any
genuine cross-*database* reference elsewhere in this project. ADR-015's
exception still holds for sectors that share CorePlatform's own database
(`AlarmManagementDb`) — the exception was never wrong, but applying it to
Security without checking where Security would actually live was a mistake
this ADR corrects in the same drafting pass, before any code exists to make
it costly to fix.

## Consequences

- `Nexus1.Security.Domain`, `Nexus1.Security.Application`,
  `Nexus1.Security.Infrastructure` — composed into `Nexus1.ModularRuntime`
  only (no independent host), but against a new, fourth-ever-separate
  physical database (`SecurityDb`) for this project, requiring one new
  connection string.
- Twenty tables remain unbuilt, named explicitly above — real residuals of
  the full atlas sector, not silently declared out of scope.
- A real, load-bearing hook for replacing Phase 1's `"system:..."` string
  literals now exists (`ApplicationUser.IsServiceAccount`) but is not wired
  in — flagged for a future, deliberate decision, not a silent TODO.
- Future sectors that reference `Security.ApplicationUser` (per the
  atlas's own dependency map: `AlarmManagement`, `EventManagement`,
  `Maintenance` — the latter two still ahead in the Phase 2 order) will
  reference it **by passport id only, not a real FK** — those sectors
  compose into `Nexus1.ModularRuntime` sharing `AlarmManagementDb`, a
  different physical database from `SecurityDb`, so ADR-015's real-FK
  exception does not extend here (see the persistence decision above). A
  real FK into `Security.ApplicationUser` would only become possible for a
  sector that also chose to live in `SecurityDb` — no such sector is
  planned.

## Rejected alternatives

- **Share `AlarmManagementDb`, matching CorePlatform's ADR-015
  precedent exactly.** Rejected: the precedent's own reasoning
  (deployment topology) doesn't cover the actual reason to isolate here
  (data sensitivity) — applying it anyway would be pattern-matching on the
  previous decision's shape rather than re-deriving the right answer for
  this sector's own facts, exactly the kind of unchecked "generalizes with
  zero changes" mistake this project's verification convention already
  warns against.
- **Build all twenty-nine tables for full atlas fidelity.** Rejected:
  unlike CorePlatform, most of the excluded twenty have no named consumer
  anywhere in this project's actual or confirmed-future dependency graph;
  building them now is exactly the ReactorFleet-physics-internals mistake
  ADR-003 already avoided, just relocated to a different sector.
- **Implement real password hashing/verification now, since
  `PasswordHash` is in scope.** Rejected: no login surface exists to call
  it, and choosing a hashing strategy (ASP.NET Core Identity's own hasher,
  BCrypt, PBKDF2) is a real dependency/architecture decision CLAUDE.md
  requires an explicit ask for — out of scope for a Domain/Infrastructure
  slice with no HTTP surface.
- **Refactor Phase 1's `OpenedBy`/`ClosedBy` string literals to reference
  `Security.ApplicationUser` now.** Rejected per the user's own explicit
  instruction: no small, obviously-correct hook exists (would add a new
  cross-context dependency AlarmManagement/RootCause never had in the
  atlas's own dependency map) — flagged, not acted on.

## Evidence required

- Domain unit tests, no persistence, for all nine entities' creation
  validation and the real behaviors (lock/unlock, grant/revoke, preference
  update).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.Security.Infrastructure`, targeting the `Security` SQL schema
  against a new `SecurityDb` physical database, independent migration
  history.
- Component tests against real LocalDB (`SecurityDb`) for the five
  Application-layer operations, including the atlas's own effective-
  permissions verification query proven against real seeded roles/grants.
- `Nexus1.ArchitectureTests` passing with `Nexus1.Security.*` composed
  into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with a `security-db` health check reaching the new
  physical database.
