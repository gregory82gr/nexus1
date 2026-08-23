# Evidence: BFF ninth vertical slice — Organization, Personnel (ADR-030 follow-up)

## Scope

Extended `Nexus1.Bff` with a ninth vertical slice, composed alongside all
eight existing contexts/composition in the same host:

- `GET /api/v1/organization/departments/{id}/roster` — Personnel screen.

No new ADR — recorded inline, same as every slice since ADR-030.

## 1. What Organization's Application layer already exposed

A meaningful existing surface: `ResolvePersonOrganizationContextQuery`
(atlas C.3.8 query 2 — resolve a login account *or* a person id directly to
person + current department + current team; this is the built-in mechanism
for the passport-only `Security.ApplicationUser` reference, see below),
`GetSitePlantHierarchyQuery` (Site → Plant physical hierarchy),
`GetLatestStaffingGapsQuery` (workforce-planning shortfall analysis, per
staffing scenario), plus assignment/staffing commands.

None of these is a roster (a list of people). `ResolvePersonOrganizationContextQuery`
resolves exactly one person; `GetLatestStaffingGapsQuery` is about required-
vs-available headcount, not who's actually on the roster.

## 2. Hosted-service check

Read `Organization.Infrastructure`'s `ServiceCollectionExtensions` directly:
zero `AddHostedService<...>()` calls. Organization is Phase 2 (ADR-017), no
messaging/observability wiring. Confirmed by reading the file, not assumed.

## 3. The central finding: per-unit personnel roster is impossible here, not just unsupported

Checked the domain model before assuming a per-unit shape, per the task's
own instruction. **There is no connection whatsoever — not even
passport-only — between `ReactorFleet.Unit` and Organization's hierarchy.**
`Plant.cs`'s own doc comment says this explicitly and unambiguously:

> "A plant within a site — not a reactor unit. It is the organizational
> plant container under a physical site; `ReactorFleet.Unit` **will later**
> carry its passport to this table through `PlantId`... **That wiring is
> not performed by this ADR** (ADR-017)."

This is a deliberately deferred piece of wiring, recorded in the codebase's
own comments, not an oversight discovered here. A `/units/{id}/personnel`
endpoint would have had nothing real to query — no FK, no passport column,
nothing.

**Shaped honestly around what the model actually supports instead**:
Organization's real hierarchy is `Department → DepartmentAssignment →
Person` (time-bounded assignments; `EndDate IS NULL` = currently active,
mirroring `IPersonOrganizationContextFinder`'s own "current" convention).
Built `GetDepartmentRosterQuery`/`GetDepartmentRosterQueryHandler` and a new
`IDepartmentRosterFinder`/`EfDepartmentRosterFinder`, returning each
currently-assigned person's name, position title, whether that position is
safety-critical, and their `Security.ApplicationUser` passport link if any.
The route is deliberately `/organization/departments/{id}/roster`, not
`/organization/units/{id}/...` — the shape follows the real model, not the
BFF's own established `{id:int}`-per-unit convention where that convention
doesn't apply.

## 4. The passport-only reference — used, not routed around

`Person.ApplicationUserId` (nullable int, no enforced FK — `ADR-017`,
`ADR-028`'s scoped `nexus1_app` login is the actual enforcement mechanism,
not a database constraint) is surfaced in `DepartmentRosterEntryDto` exactly
as-is: the raw passport int, or `null` if the person has no login. This is
deliberately *not* resolved further here — Organization's own domain model
only ever knows whether a person has a linked login, never anything about
that login's own attributes (username, roles, active flag), since
`SecurityDb` is a different physical database with no real FK to join
against. Resolving the passport to an actual Security identity is Security's
own job, not something to fake or route around in Organization's own query.

**Proven live**, not just asserted: seeded two people in the same
department — one (`Alex Rivera`) with no linked login, one (`Jordan Chen`)
with `ApplicationUserId = 42`. The live response (below) shows
`"applicationUserId":null` for the first and `"applicationUserId":42` for
the second, exactly as designed — the reference is real, present, and
correctly opaque from Organization's own point of view.

## Build and full regression suite

```
dotnet build Nexus1.Runtime.sln   → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln    → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as all eight prior slices — zero regressions.

## Real host, real database — live evidence

Memory checked before starting the host (2.68 GB, confirmed stable across
two checks — 2.68 → 2.67 GB). Rechecked after host start (2.54 GB) and
before the endpoint call (2.68 GB) — stable throughout, no incident this
run.

`Organization.Department`/`Person`/`DepartmentAssignment` all had **zero
rows**. Seeded minimal real data. One seeding wrinkle: the first attempt
failed with `Msg 1934` ("SET options have incorrect settings:
'QUOTED_IDENTIFIER'") — `sqlcmd` compiles a multi-statement `-Q` batch as a
whole, and something in the table set (likely a filtered index) requires
`QUOTED_IDENTIFIER ON` for the *entire* batch to compile; when it failed,
**nothing** in the batch had executed (confirmed: all tables still at 0
rows afterward), not a partial insert — re-ran with `SET QUOTED_IDENTIFIER
ON;` prepended and it succeeded cleanly.

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO Organization.LegalEntityType (...) VALUES (1, 'OPERATOR', 'Reactor Operator', ...);
INSERT INTO Organization.DepartmentType (...) VALUES (1, 'OPERATIONS', 'Operations', ...);
INSERT INTO Organization.PersonType (...) VALUES (1, 'EMPLOYEE', 'Employee', ...);
INSERT INTO Organization.LegalEntity (LegalEntityId, LegalEntityTypeId, Code, Name, IsOperator, IsVendor, CreatedAtUtc)
  VALUES (1, 1, 'NEXUS1-CORP', 'Nexus1 Operating Company', 1, 0, ...);
INSERT INTO Organization.Department (DepartmentId, LegalEntityId, DepartmentTypeId, Code, Name, CreatedAtUtc)
  VALUES (1, 1, 1, 'OPS-DEPT', 'Operations Department', ...);
INSERT INTO Organization.Position (PositionId, DepartmentId, Code, Title, IsSafetyCritical, RequiresShiftWork, CreatedAtUtc)
  VALUES (1, 1, 'REACTOR-OP', 'Reactor Operator', 1, 1, ...);
INSERT INTO Organization.Position (...) VALUES (2, 1, 'SHIFT-SUPERVISOR', 'Shift Supervisor', 1, 1, ...);
INSERT INTO Organization.Person (PersonId, PersonTypeId, ApplicationUserId, GivenName, FamilyName, DisplayName, IsActive, CreatedAtUtc)
  VALUES (1, 1, NULL, 'Alex', 'Rivera', 'Alex Rivera', 1, ...);
INSERT INTO Organization.Person (...) VALUES (2, 1, 42, 'Jordan', 'Chen', 'Jordan Chen', 1, ...);
INSERT INTO Organization.DepartmentAssignment (DepartmentAssignmentId, PersonId, DepartmentId, PositionId, StartDate, IsPrimary, CreatedAtUtc)
  VALUES (1, 1, 1, 1, '2026-01-01', 1, ...);
INSERT INTO Organization.DepartmentAssignment (...) VALUES (2, 2, 1, 2, '2026-02-01', 1, ...);
```

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/organization/departments/1/roster`

```json
[
  {"personId":1,"displayName":"Alex Rivera","personnelNumber":null,"positionTitle":"Reactor Operator","isSafetyCriticalPosition":true,"applicationUserId":null,"startDate":"2026-01-01","isPrimary":true},
  {"personId":2,"displayName":"Jordan Chen","personnelNumber":null,"positionTitle":"Shift Supervisor","isSafetyCriticalPosition":true,"applicationUserId":42,"startDate":"2026-02-01","isPrimary":true}
]
```

HTTP 200 — confirms the passport reference renders correctly for both the
no-login and has-login cases.

### `GET /api/v1/organization/departments/999/roster` (department with no roster)

```json
[]
```

HTTP 200.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

Five sessions, all `nexus1_app`, run against `OrganizationDb` specifically
(its own separate physical database, ADR-017) — confirmed, no fallback.

`sys.databases` confirmed all `ONLINE` afterward; no corruption.

## Summary

Nine vertical slices now exist in `Nexus1.Bff`. Organization's contribution
is a genuine, well-documented architectural boundary — not a gap this
codebase forgot to close, but one it explicitly recorded as deferred and
never performed — handled by shaping the endpoint around what actually
exists (Department-scoped roster) rather than forcing a per-unit shape onto
data that has no way to support it. The passport-only reference to
Security was exercised and shown working exactly as ADR-028 intends: opaque
from Organization's side, present when set, resolvable only by whoever
composes with Security directly.

Security is next, evidence to follow separately.
