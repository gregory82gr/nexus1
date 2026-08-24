# Evidence: Angular console, Ch. 19 — Robotics & Vehicles

## Scope

Two real screens plus two genuine, minimal backend additions:

1. **BFF**: `GET /api/v1/robotics/missions/{id}/readiness-failures` and
   `GET /api/v1/robotics/missions/{id}/timeline` — thin routes wrapping
   `GetBlockingReadinessFailuresQueryHandler` and
   `GetMissionTimelineQueryHandler`, both already existing and registered.
   Only `readiness-failures` is wired into a screen this round;
   `timeline` is added (trivial marginal cost, already registered) but
   not yet consumed by any component — named as available-for-later, not
   built into a screen, since a mission event-history view is a
   different concept than a readiness check and wasn't asked for this
   round.
2. `RoboticsFleetComponent` (`features/robotics-fleet/`) — serves
   `robotics-overview`, wired to the already-proven
   `GET /robotics/units/{id}`.
3. `MissionReadinessComponent` (`features/mission-readiness/`) — serves
   `robotics-readiness`, reshaped around the real recorded-assessment
   model rather than the book's own design.

## Investigation: same "book has nothing" pattern, but a structurally different real domain this time

Ch. 19's own source material states its boundary as plainly as Ch. 16-18
did: *"Volume III has no robotics endpoint. The fleet, its battery
levels, its accumulated doses, and its dose limits are all generated."*
So the book's own Mission Readiness is a live capability-matching engine
over invented inputs: six abstract "standard mission types" (Containment
survey, Leak inspection, Debris manipulation, Radiation mapping,
Emergency response, Cask handling), each evaluated against the *current*
fleet using capability tags, battery, and accumulated radiation dose
against a per-mission dose budget (`evaluate(mission, fleet)` →
`{covered, margin, limiting, considered, inputConfidence}`).

**Checked the real domain directly, not just the exposed DTO, before
assuming any of that carries over — it doesn't, on two separate axes**:

1. **No dose or radiation field exists anywhere in Robotics' domain.**
   `Robot` (code, name, model→type, status, home unit) and
   `RobotHealthSnapshot` (battery percent, estimated runtime, CPU load,
   fault count) were both read directly. Neither has, or has room for, a
   cumulative-dose or dose-limit concept. A total absence, the same
   shape as Decommissioning/Waste's own finding, not a nullable column
   this screen is choosing to omit.
2. **`Mission` is a real, already-dispatched work order, not an abstract
   mission-type definition.** It carries `UnitId`, `MissionType`/
   `Status`/`Priority`, and real requested/planned/actual timestamps —
   tracking a mission that already exists, not evaluating whether a
   *hypothetical* mission type could be covered right now. There is no
   "given the current fleet, could mission type X be covered" concept
   anywhere in this domain, and no formal capability-tag system either
   (`RobotType` is a single free-text code per robot, not a set of
   capability tags a mission could require several of).

**What the real domain DOES have, and what Mission Readiness is built
around instead**: `MissionReadinessAssessment`/`MissionReadinessItem` —
a genuinely recorded readiness verdict for one specific, already-known
mission, with named blocking checks (`CheckName`, `IsBlocking`,
`Detail`), queried via `GetBlockingReadinessFailuresQuery` (already
existed, already registered in DI — atlas C.12.5.2 query 4, filtering
`IsBlocking = 1` and status `BLOCKED`/`EXPIRED`). This is the real
analogue of the book's own "decompression panel" (why a mission's
readiness failed) — just scoped to a mission that was already assessed,
not a live hypothetical evaluation over six invented mission types.

**A further real limitation, found while wiring this up, not assumed in
advance**: `UnitMissionDto` (the mission-summary list the overview
endpoint returns) carries each mission's `MissionCode` but not its
numeric `Id` — so there is no way to drill from a row in that list
straight into its own readiness detail. The lookup is necessarily a
separate, manually-keyed tool (a mission-id text input), not a
click-through from the mission list above it, and the screen states this
plainly rather than implying a connection that doesn't exist.

**Decision applied**: build Fleet Overview fully real (nothing to
reshape — robot status/health has no book-vs-domain mismatch). Build
Mission Readiness around the real recorded-assessment model: the unit's
real mission list, plus a real, working lookup of any mission's actual
recorded blocking-readiness checks by id. Add the two thin BFF routes
for the Application-layer capabilities that already existed (same
pattern as the Absence Stress Test / Ageing & Degradation precedents),
no new Application or Infrastructure code.

## The two new BFF routes

```csharp
app.MapGet("/api/v1/robotics/missions/{id:long}/readiness-failures", async (long id, [FromServices] GetBlockingReadinessFailuresQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetBlockingReadinessFailuresQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});

app.MapGet("/api/v1/robotics/missions/{id:long}/timeline", async (long id, [FromServices] GetMissionTimelineQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetMissionTimelineQuery(id), cancellationToken);
    return Results.Ok(result.Value);
});
```

`{id:long}`, not `{id:int}` — `MissionId`'s own underlying type is
`long`, checked directly rather than assumed from the BFF's usual
`{id:int}` convention. No DI/infrastructure changes needed for either
route.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as every prior slice — confirmed genuinely unchanged.

## Frontend: what was built

- `core/api/robotics-api.ts` — `UnitRobotStatus`/`UnitMission`/
  `UnitRoboticsOverview` (mirror the existing DTOs exactly) plus
  `ReadinessFailure` (mirrors `ReadinessFailureDto`); `RoboticsApi.
  getUnitOverview(unitId)` / `getReadinessFailures(missionId)`. The
  mission-id parameter is passed as a string, not a number, to avoid any
  JS `Number` precision loss on a genuinely 64-bit backend id, even
  though this dev database's own ids are small.
- `features/robotics-fleet/robotics-fleet.ts/.html/.scss` —
  loading/error/loaded state over the real endpoint; robot code, name,
  status, and latest battery/communication reading, or an honest
  "NO HEALTH DATA" pill for a robot with no snapshot yet.
- `features/mission-readiness/mission-readiness.ts/.html/.scss` — two
  independent panels: the unit's real mission list (loading/error/
  loaded), and a separate readiness lookup (idle/loading/error/loaded)
  driven by a manually-entered mission id, calling the new route.

## Tests

```
npx jest → 144/144 passing (was 136; 8 new specs)
```

- `robotics-fleet.spec.ts` — loading/error/loaded states, a robot with no
  health snapshot rendered honestly, real error state on an unreachable
  endpoint.
- `mission-readiness.spec.ts` — missions load independently from the
  lookup; the lookup starts idle (not auto-firing); a real lookup by a
  manually-entered id renders real failures; an empty response is
  treated as a genuine "no blocking failures" state, not an error; real
  error states for both panels independently.

Production build:

```
npx ng build → 0 errors, 0 warnings. robotics-fleet and
               mission-readiness each compile to their own small lazy
               chunk (~1.5 KB / ~2.1 KB transfer).
```

Both gate runs (Jest, then the .NET build+test) were run sequentially,
not concurrently, per the resource-contention lesson from the Plant
Lifecycle slice — no crash this time.

## Live evidence — real host, real database, real screenshots

Memory checked before starting both processes (2.24 → 2.27 GB, stable).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Robotics`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                                          → Healthy, HTTP 200
GET /api/v1/robotics/units/1                                →
  {"unitId":1,"robots":[{"robotCode":"ROBOT-1",...,"latestBatteryPercent":82.00,...},
   {"robotCode":"ROBOT-2",...,"latestBatteryPercent":null,...}],
   "missions":[{"missionCode":"MISSION-1","title":"Inspect containment weld seams",...}]}
GET /api/v1/robotics/missions/1/readiness-failures (before seeding) → []
```

Same robots/mission seeded during the earlier backend slice
(`2026-08-22-bff-robotics-fleet-overview-slice.md`) — no reseeding
needed for Fleet Overview. Confirmed the new readiness-failures route's
genuine empty state live, before seeding anything for it.

**Seeded a real readiness assessment for Mission 1**, with one blocking
and one non-blocking item, to prove the query's own filter (only
`IsBlocking = 1` and status `BLOCKED`/`EXPIRED`) works correctly, not
just that the route responds:

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO Robotics.ReadinessStatus (ReadinessStatusId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (1, 'READY', 'Ready', 1, 1, SYSUTCDATETIME());
INSERT INTO Robotics.ReadinessStatus (ReadinessStatusId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (2, 'BLOCKED', 'Blocked', 2, 1, SYSUTCDATETIME());
INSERT INTO Robotics.MissionReadinessAssessment (MissionReadinessAssessmentId, MissionId, ReadinessStatusId, AssessedAtUtc, Summary) VALUES (1, 1, 2, SYSUTCDATETIME(), 'Two checks blocking dispatch.');
INSERT INTO Robotics.MissionReadinessItem (MissionReadinessItemId, MissionReadinessAssessmentId, ReadinessStatusId, CheckName, Detail, IsBlocking) VALUES (1, 1, 2, 'Battery charge', 'ROBOT-2 at 18 percent charge, below the 30 percent minimum for this mission.', 1);
INSERT INTO Robotics.MissionReadinessItem (MissionReadinessItemId, MissionReadinessAssessmentId, ReadinessStatusId, CheckName, Detail, IsBlocking) VALUES (2, 1, 1, 'Communication link', 'All assigned robots reporting CONNECTED.', 0);
```

```
GET /api/v1/robotics/missions/1/readiness-failures (after seeding) →
  [{"checkName":"Battery charge","readinessStatus":"BLOCKED","detail":"ROBOT-2 at 18 percent charge, below the 30 percent minimum for this mission."}]
```

Only the blocking item appears — the non-blocking "Communication link"
item was correctly excluded by the query's own filter, confirmed live,
not just asserted from reading the query's source.

`/robotics-overview` rendered live: both real robots, `ROBOT-2` showing
`NO HEALTH DATA` honestly. `/robotics-readiness` rendered live: the real
mission list, and — after clicking "Check readiness" with the default
lookup id `1` — the real `BLOCKED` pill and detail text for `Battery
charge`, matching the seeded data exactly.

### Screenshots

- `robotics-fleet-overview.png` — both robots, one with real health data,
  one honestly showing no health data yet.
- `mission-readiness-idle.png` — the mission list and the lookup panel
  before any lookup has been run.
- `mission-readiness-lookup.png` — the real `BLOCKED` result for mission
  id 1 after clicking "Check readiness".

All reviewed directly: full-width shell, clean layouts, no cramped
columns. The sidebar's own active-state fix (from an earlier slice)
again generalizes correctly to this new nav group, distinguishing
"Fleet Overview" active from "Mission Readiness" active correctly.

Login/session verification:

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

Investigated the real domain directly (not just the exposed DTOs) before
building, per the task's own instruction, and found a genuine structural
mismatch on two axes: no dose/radiation data anywhere (total absence),
and "mission" meaning a real dispatched work order rather than the
book's own abstract, hypothetically-evaluated mission type. Built Fleet
Overview fully real (no mismatch there), and reshaped Mission Readiness
around what the domain actually models — real missions, plus a real
recorded readiness assessment lookup — rather than forcing the book's
live capability-matching engine onto data that cannot support it. Added
two thin BFF routes for Application-layer capabilities that already
existed and were already registered, confirmed both the "before" (empty)
and "after" (real, filtered) states live, and named the mission-list's
own real code-vs-id limitation explicitly rather than pretending a
click-through connection that doesn't exist.
