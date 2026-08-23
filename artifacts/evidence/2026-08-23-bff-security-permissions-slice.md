# Evidence: BFF tenth vertical slice — Security, effective permissions (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a tenth vertical slice, composed alongside all
nine existing contexts/composition in the same host:

- `GET /api/v1/security/users/{id}/permissions` — the closest honest thing
  to a "Zone Access" screen this domain model actually supports (see the
  named gap below).

No new ADR — recorded inline, same as every slice since ADR-030.

## 1. What Security's Application layer already exposed

A complete RBAC surface: `GetEffectivePermissionsForUserQuery` (atlas
C.2.8's "authorization backbone" verification query — effective permissions
for one user from active, non-expired roles), `AssignRoleToUserCommand`,
`GrantPermissionToRoleCommand`, `LockUserCommand`/`UnlockUserCommand`,
`UpdateUserPreferenceCommand`. `GetEffectivePermissionsForUserQueryHandler`
was **already fully built** — this slice needed **zero new Application or
Infrastructure-layer code in Security**, the same "reuse as-is" shape as
the Overview slice, not the "add a sibling query" shape most other slices
needed.

## 2. Hosted-service check

Read `Security.Infrastructure`'s `ServiceCollectionExtensions` directly:
zero `AddHostedService<...>()` calls. Confirmed by reading the file, not
assumed.

## 3. The named gap: Security has no "Zone Access" concept at all

Checked before assuming the task's framing was correct, per its own
explicit instruction. **Security's entire domain model is application-level
RBAC — there is no physical/zone access concept anywhere in this schema.**
`ApplicationUser` is a login-capable account (ASP.NET Identity-shaped,
integer-keyed); `Permission`'s own doc comment names its actual subject
matter directly: "such as alarm acknowledgement, report export, or security
administration" — software actions, not physical doors or areas.
`PermissionCategory`'s doc comment: "platform, alarm, twin, reporting,
audit, security" — again, software subsystems, not physical zones. There is
no `Zone`, `Badge`, `AccessPoint`, or anything resembling a physical-access
concept in `Nexus1.Security.Domain` at all.

(Physical zone data does exist in this codebase — `RadiationZone.IsEntryControlled`/
`RequiresDosimeter` in RadiationMonitoring — but that is a different context
entirely, already exposed by the RadiationMonitoring slice's own endpoint,
and it isn't an *access-control* concept either, just a zone classification
flag.)

**Decision**: rather than fabricate a "Zone Access" endpoint out of RBAC
data mislabeled as physical access (which would misrepresent what the data
means — the same discipline this project already applies to verdicts and
cross-context references), the endpoint is named for what it actually is:
effective *application* permissions for a user. The gap is recorded here
explicitly, not silently substituted.

## 4. The passport reference — resolved, proving ADR-028's design end-to-end

The Organization slice's evidence recorded `Person.ApplicationUserId` as an
opaque passport int that Organization itself never resolves. This slice is
that resolution point, in Security's own context: `GET /api/v1/security/users/{id}/permissions`
takes exactly the same `ApplicationUserId` shape. Seeded `ApplicationUserId
42` in `SecurityDb` — deliberately the **same id** as the Organization
slice's `Jordan Chen` (whose roster entry showed `"applicationUserId":42`)
— with a role (`ShiftSupervisor`) granting a real permission
(`ALARM_ACKNOWLEDGE`). The live call below returns that permission for user
42, demonstrating that the passport reference genuinely resolves to a real,
independently-verifiable identity in Security's own database — not just a
number sitting inertly in Organization's table. Nothing in the code joins
the two databases directly (no FK, no cross-context query) — the BFF, by
being able to call both contexts' already-existing handlers, is what closes
the loop, exactly matching ADR-017/ADR-028's intended shape.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all nine prior slices — zero regressions, despite this
slice adding zero Application/Infrastructure code to Security itself.

## Real host, real database — live evidence

Memory checked before starting the host (2.60 GB, confirmed stable across
two checks — 2.60 → 2.58 GB). Rechecked after host start (2.57 GB) and
before the endpoint call (2.57 GB) — stable throughout, no incident this
run.

`Security.ApplicationUser`/`ApplicationRole`/`Permission` all had **zero
rows**. Seeded minimal real data, deliberately reusing `ApplicationUserId
42` from the Organization slice:

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO Security.UserStatus (...) VALUES (1, 'ACTIVE', 'Active', ...);
INSERT INTO Security.RoleType (...) VALUES (1, 'OPERATOR', 'Operator', ...);
INSERT INTO Security.PermissionCategory (...) VALUES (1, 'ALARM', 'Alarm Management', ...);
INSERT INTO Security.Permission (PermissionId, PermissionCategoryId, Code, Name, ActionName, IsSafetyRelevant, IsActive, CreatedAtUtc)
  VALUES (1, 1, 'ALARM_ACKNOWLEDGE', 'Acknowledge Alarm', 'ACKNOWLEDGE', 1, 1, ...);
INSERT INTO Security.ApplicationUser (ApplicationUserId, UserStatusId, UserName, NormalizedUserName, DisplayName, IsServiceAccount, EmailConfirmed, LockoutEnabled, AccessFailedCount, CreatedAtUtc)
  VALUES (42, 1, 'jordan.chen', 'JORDAN.CHEN', 'Jordan Chen', 0, 0, 1, 0, ...);
INSERT INTO Security.ApplicationRole (ApplicationRoleId, RoleTypeId, Name, NormalizedName, IsBuiltIn, IsActive, CreatedAtUtc)
  VALUES (1, 1, 'ShiftSupervisor', 'SHIFTSUPERVISOR', 0, 1, ...);
INSERT INTO Security.UserRole (ApplicationUserId, ApplicationRoleId, AssignedAtUtc, IsActive) VALUES (42, 1, '2026-02-01', 1);
INSERT INTO Security.RolePermission (ApplicationRoleId, PermissionId, IsGranted, GrantedAtUtc) VALUES (1, 1, 1, '2026-02-01');
```

(Same `QUOTED_IDENTIFIER` requirement as the Organization slice — applied
proactively this time based on that earlier finding.)

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/security/users/42/permissions`

```json
[{"permissionCode":"ALARM_ACKNOWLEDGE","permissionName":"Acknowledge Alarm","categoryCode":"ALARM","isGranted":true}]
```

HTTP 200 — this is user 42's real, effective permission, derived through
the active role assignment and non-expired role-permission grant, exactly
matching what the Organization slice's `Jordan Chen` roster entry pointed
at.

### `GET /api/v1/security/users/999/permissions` (user with no roles)

```json
[]
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

Six sessions, all `nexus1_app`, run against `SecurityDb` specifically (its
own separate physical database, ADR-016) — confirmed, no fallback.

`sys.databases` confirmed all `ONLINE` afterward; no corruption.

## Summary

Ten vertical slices now exist in `Nexus1.Bff`. Security's contribution is
a named, honest gap (no zone/physical-access concept exists) paired with a
genuine, positive proof: the Organization ↔ Security passport reference
(ADR-017/ADR-028) resolves end-to-end when both contexts are composed in
the same BFF, using zero new code in Security and a small, well-labeled
addition in Organization.

Both Organization and Security are now complete.
