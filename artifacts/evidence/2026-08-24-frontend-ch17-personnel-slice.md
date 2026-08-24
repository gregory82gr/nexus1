# Evidence: Angular console, Ch. 17 — Personnel (department-scoped)

## Scope

Two real screens plus a genuine, minimal backend addition:

1. **BFF**: `GET /api/v1/organization/staffing-scenarios/{id}/gaps` — a
   new thin route wrapping the existing
   `GetLatestStaffingGapsQueryHandler`/`StaffingScenarioGapDto`, per the
   explicit decision to add it rather than skip Absence Stress Test.
2. `PersonnelOverviewComponent` (`features/personnel-overview/`) —
   serves `personnel-overview`, wired to the already-proven
   `GET /organization/departments/{id}/roster`.
3. `AbsenceStressTestComponent` (`features/absence-stress-test/`) —
   serves `personnel-stress`, wired to the new gaps route.

Sector Roster (the book's per-sector/per-person drill-in) is **not
built** — named as a real gap, not silently dropped.

## Investigation: Ch. 17's real thesis, and how it changes the shape of this cluster

Unlike Ch. 16, this chapter's central argument is not "the backend has
nothing" — it's a data-minimization argument that applies even to data
the backend genuinely has. The book states the operational question
plainly: *does each sector meet its minimum complement, and is every
critical role covered?* Answering it needs a headcount and role-slot
coverage — not names, not qualifications displayed alongside them, and
*never* an absence reason (the book's own source file renders `"Sick —
influenza"` on an operations console; the chapter treats this as the
central thing to fix, not a detail). Names are reserved for exactly one
separate, route-guarded screen, reached deliberately, for the narrow
purpose of contacting a specific qualified person.

**Live demo confirmed the nav shape**: "Sector Roster" and the book's own
per-person drill-in match this project's existing 2-route Personnel nav
group (`personnel-overview`, `personnel-stress`) — the demo's own
sub-nav under "Personnel" lists sector *names* (Control Room Operations,
Nuclear Safety/Reactor Eng., Maintenance, ...) as a category picker for a
per-sector view, the same drill-in pattern as Rod Type/Film, not a flat
top-level route. No nav restructuring was needed.

**Real backend facts, checked directly**:
- `DepartmentRosterEntryDto` genuinely includes `DisplayName` — unlike
  Ch. 16, there is real identifying data here, not an absent concept.
  Department-scoped, not unit-scoped: `Plant.cs`'s own doc comment
  records the ReactorFleet↔Organization connection as deferred and never
  performed (ADR-017).
- No presence/absence field exists anywhere in the roster — it's
  time-bounded assignments, not live attendance.
- `PersonnelRequirement` and `StaffingScenario`/`StaffingScenarioRequirement`
  are real, structured domain concepts (minimum required counts,
  safety-critical flags, named what-if scenarios) — richer than
  expected. `GetLatestStaffingGapsQuery`/`StaffingScenarioGapDto`
  already existed in the Application layer, registered in DI, with
  zero infrastructure changes needed — only a route was missing.
  `StaffingScenarioGapDto` carries no position title, only a raw
  `PositionId` — a real, separate gap from the roster's own
  `PositionTitle` field (different query, no shared resolution).

**Decision applied** (per your explicit choice on the one open fork):
add the thin BFF route for the gaps query rather than skip Absence
Stress Test. Build Personnel Overview aggregated down to counts/position
coverage (never names, even though the raw DTO has them). Do not build
the names-level Sector Roster screen — it is exactly the book's own "one
screen that needs a guard," and this Angular app has no auth/guard
infrastructure at all yet; building it unguarded would be the wrong call
regardless of backend availability.

## Department-selection design (the specific question you asked me to resolve)

The topbar's existing unit selector (`PlantStateService`) doesn't apply —
Organization has no connection to a ReactorFleet unit at all, so reusing
it would conflate two different concepts. Built a separate, minimal
`DepartmentStateService` (`core/state/department-state.ts`), defaulting
to `1` (the one real seeded department, "Operations Department," from
the roster's own proven evidence). A plain numeric department-id input
lets the operator change it — not a dropdown, since no "list
departments" endpoint exists to back one honestly, and a dropdown of
guessed department names would be worse than a bare id field. Absence
Stress Test gets its own, separate, purely local `scenarioId` signal
(not shared state) — nothing else in the console needs "the current
staffing scenario."

## The new BFF route

```csharp
app.MapGet("/api/v1/organization/staffing-scenarios/{id:int}/gaps", async (int id, [FromServices] GetLatestStaffingGapsQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetLatestStaffingGapsQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});
```

No DI/infrastructure changes needed — `GetLatestStaffingGapsQueryHandler`
and `IStaffingGapFinder`/`EfStaffingGapFinder` were already registered;
this is a route-only addition, the same minimal shape as every other
slice's own thin BFF wrapper.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as every prior slice — confirmed genuinely unchanged,
not just assumed, since this route is a pure addition over existing,
already-tested Application-layer code.

## Frontend: what was built

- `core/api/organization-api.ts` — `DepartmentRosterEntry` (mirrors
  `DepartmentRosterEntryDto` exactly) and `StaffingScenarioGap` (mirrors
  `StaffingScenarioGapDto`); `OrganizationApi.getDepartmentRoster(id)` /
  `getStaffingGaps(id)`.
- `core/state/department-state.ts` — the department-selection service
  described above.
- `features/personnel-overview/personnel-aggregation.ts` — pure
  `aggregateRoster()`: strips `DisplayName`/`PersonId`/`ApplicationUserId`/
  `PersonnelNumber`/`StartDate` from every entry, returning only total
  count, safety-critical count, and per-position-title counts. This is
  the actual mechanism enforcing Ch. 17's minimization principle — not a
  UI omission that a future screen could accidentally undo, since the
  identifying fields never survive past this one function.
- `features/personnel-overview/personnel-overview.ts/.html/.scss` —
  loading/error/loaded state over the real roster endpoint, department-id
  input, renders only the aggregated summary.
- `features/absence-stress-test/absence-stress-test.ts/.html/.scss` —
  loading/error/loaded state over the new gaps endpoint, scenario-id
  input, client-side breach detection (`gapCount > 0`), positions
  rendered as `Position #{id}` (no title resolution available, stated
  honestly rather than guessed).

## Tests

```
npx jest   → 128/128 passing (was 115; 13 new specs)
```

- `personnel-aggregation.spec.ts` — counts total/safety-critical size
  correctly; groups by position title; **asserts the serialized summary
  never contains a real name** (`Alex Rivera`, `Jordan Chen`), not just
  that the UI happens not to display one; honest fallback label for a
  null position title; empty-roster case.
- `personnel-overview.spec.ts` — loading/error/loaded states, fetches
  department 1 by default, re-fetches on a new department id, and (same
  as the aggregation spec) asserts no name ever appears in component
  state.
- `absence-stress-test.spec.ts` — loading/error/loaded states, honest
  breach counting from real gap counts, an empty response treated as a
  genuine "not evaluated" state rather than an error, re-fetch on a new
  scenario id.

Production build:

```
npx ng build → 0 errors, 0 warnings. personnel-overview and
               absence-stress-test each compile to their own small lazy
               chunk (~2.2 KB / ~2.0 KB transfer).
```

## Live evidence — real host, real database, real screenshots

Memory checked before starting both processes (2.36 → 2.35 GB, stable).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Organization`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                                        → Healthy, HTTP 200
GET /api/v1/organization/departments/1/roster             →
  [{"personId":1,"displayName":"Alex Rivera",...,"positionTitle":"Reactor Operator","isSafetyCriticalPosition":true,...},
   {"personId":2,"displayName":"Jordan Chen",...,"positionTitle":"Shift Supervisor","isSafetyCriticalPosition":true,...}]
GET /api/v1/organization/staffing-scenarios/1/gaps (before seeding) → []
```

Confirmed the new route's "not evaluated yet" behavior live, before
seeding anything, exactly as designed (a real empty 200, not an error).

**Seeded real staffing-scenario data** (reusing Position 1/2 already
seeded for the roster slice — no need to invent new positions):

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO Organization.SiteType (SiteTypeId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (1, 'PLANT', 'Plant Site', 1, 1, SYSUTCDATETIME());
INSERT INTO Organization.Site (SiteId, LegalEntityId, SiteTypeId, CountryId, TimeZoneId, Code, Name, IsOperational, CreatedAtUtc) VALUES (1, 1, 1, 1, 1, 'SITE-1', 'Demonstrator Site', 1, SYSUTCDATETIME());
INSERT INTO Organization.StaffingScenario (StaffingScenarioId, SiteId, ScenarioCode, Name, Description, CreatedAtUtc) VALUES (1, 1, 'SEASONAL-FLU-20', 'Seasonal absence 20%', 'Role-slot reduction modeling a seasonal flu-like absence rate.', SYSUTCDATETIME());
INSERT INTO Organization.StaffingScenarioResult (StaffingScenarioResultId, StaffingScenarioId, EvaluatedAtUtc, OverallStatus) VALUES (1, 1, SYSUTCDATETIME(), 'Fail');
INSERT INTO Organization.StaffingScenarioGap (StaffingScenarioGapId, StaffingScenarioResultId, PositionId, RequiredCount, AvailableCount) VALUES (1, 1, 1, 2, 1);
INSERT INTO Organization.StaffingScenarioGap (StaffingScenarioGapId, StaffingScenarioResultId, PositionId, RequiredCount, AvailableCount) VALUES (2, 1, 2, 1, 1);
```

`GapCount` deliberately omitted from the insert list — it's a SQL Server
`PERSISTED` computed column (`RequiredCount - AvailableCount`), not a
value this client (or even the command handler) ever writes directly.

```
GET /api/v1/organization/staffing-scenarios/1/gaps (after seeding) →
  [{"positionId":1,"requiredCount":2,"availableCount":1,"gapCount":1,"notes":null},
   {"positionId":2,"requiredCount":1,"availableCount":1,"gapCount":0,"notes":null}]
```

The database computed `GapCount` (1 and 0) exactly as expected.

`/personnel-overview` rendered live: `2` total assigned, `2`
safety-critical positions filled, `Reactor Operator` and `Shift
Supervisor` each showing count `1` — no name anywhere on the page or in
the DOM.

`/personnel-stress` rendered live: `STAFFING BREACH`, `1 of 2 positions
short`, `Position #1` showing `1 / 2 required` / `SHORT 1`, `Position #2`
showing `1 / 1 required` / `COVERED` — matching the seeded data exactly.

### Screenshots

- `personnel-overview.png` — the consolidation/minimization note, the
  department-id input, both stat panels, the two-position coverage table.
- `absence-stress-test.png` — the scenario-id input, the real
  `STAFFING BREACH` banner, both position rows with their real
  short/covered pills.

Both reviewed directly: full-width shell, clean layouts, no cramped
columns, no dead space. As a bonus, both screenshots also confirm the
sidebar's own active-state fix (from the immediately preceding slice)
generalizes correctly to this new nav group — "Personnel" shows the
group-active treatment with "Overview"/"Stress Test" correctly
highlighted.

Login/session verification (against `OrganizationDb` specifically, its
own separate physical database per ADR-017):

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x2)
```

Two sessions, matching the two composed contexts. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Summary

Ch. 17's real lesson — minimize even when the data supports more — was
applied as an actual code mechanism (`aggregateRoster()` stripping
identifying fields before they ever reach component state), tested
directly (asserting real names never appear in the serialized summary),
not just as a UI choice that a later screen could quietly undo. Added
one thin, real BFF route per your explicit decision, wrapping an
Application-layer capability that already existed and was already
registered — no fabricated data, and the empty-before-seeding state was
confirmed live before any seed data existed. The one screen the book
says needs a guard was correctly not built, since this console has no
guard infrastructure to make that responsible yet.
