# Evidence: BFF sixteenth vertical slice — EmergencyPreparedness (backend-only, no console screen)

## Scope

Extended `Nexus1.Bff` with a sixteenth vertical slice, wiring in all four of
EmergencyPreparedness's already-existing queries:

- `GET /api/v1/emergency-preparedness/sites/{siteId}/plans` — a site's
  active emergency plans, with current revision number and revision count.
- `GET /api/v1/emergency-preparedness/exercises/corrective-observations` —
  exercises (drills) with at least one corrective-action-required finding.
- `GET /api/v1/emergency-preparedness/evacuation-routes/open-or-restricted`
  — open/restricted evacuation routes and the radiological zones they cross.
- `GET /api/v1/emergency-preparedness/resource-readiness-dashboard` —
  emergency-resource counts by site/type/latest-readiness-status.

Built as **backend-only**, same treatment as EventManagement — no Angular
console screen backs this domain, confirmed by an exhaustive sitemap search
before this slice began (see the EventManagement/EmergencyPreparedness joint
investigation, reported separately). Documented explicitly in code comments
and here, not fitted to a screen shape it doesn't have.

## 1. Which existing queries/commands map to real BFF-worthy reads

All four existing queries were judged genuinely useful and built — none
were skipped. Each matches one of the atlas's own four named verification
queries, adapted only where a cross-database passport-only reference
required it (`SiteId` projected as a plain int rather than a joined site
code, since `Organization.Site` lives in a different physical database,
same adaptation `EfSiteActivePlansFinder`/`EfResourceReadinessDashboardFinder`
already carry in their own doc comments):

1. **Plan status** → `GetSiteActivePlansQuery(int SiteId)`.
2. **Resource readiness** → `GetResourceReadinessDashboardQuery()`.
3. **Exercise/drill history** → `GetExercisesWithCorrectiveObservationsQuery()`.
4. **Evacuation routes** → `GetOpenOrRestrictedRoutesCrossingZonesQuery()`.

The two commands (`ApproveEmergencyPlanCommand`, `ScheduleExerciseCommand`)
are write-side and out of scope for this read-only BFF slice, same
treatment as every prior slice's commands.

Zero new Application-layer code was needed — all four handlers already
existed and are wired in as-is.

## 2. Site-scoped, not unit-scoped — a genuinely new granularity for this BFF

**Design note, as requested.** Every one of the fifteen prior slices routed
around either a `{unitId}` (per-reactor-unit data — Maintenance, RadiationMonitoring,
Instrumentation, etc.) or a fleet-wide/global flat listing with no scoping
parameter at all (CorePlatform, Audit, Compliance, EventManagement).
EmergencyPreparedness is the **first context scoped by `SiteId`** — a
whole site (potentially containing multiple plants/units), not a single
reactor unit. This is a real, different granularity in the domain itself
(`EmergencyPlan.SiteId`, `Exercise.SiteId`, `EmergencyResource.SiteId`,
`EvacuationRoute.SiteId`, `AssemblyPoint.SiteId` — all site-level, none
carry a `UnitId` anywhere in this domain), not a naming choice made for
this slice.

Routed honestly per this real granularity: only
`GET .../sites/{siteId}/plans` takes a scoping route parameter — it is the
one query that is genuinely site-scoped (`GetSiteActivePlansQuery(int SiteId)`).
The other three queries are **fleet-wide with zero parameters**, even
`GetResourceReadinessDashboardQuery()`, whose DTO carries `SiteId` per row
but is queried unscoped and grouped across every site — so those three are
flat listings, not forced under a `{siteId}` or `{unitId}` prefix that
doesn't match their real query shape. This mirrors the same discipline
CorePlatform's fleet-wide endpoints already established (name the real
scope honestly, don't force a route shape that implies scoping the query
doesn't have).

**For future reference:** if a later slice needs a second site-scoped
context, `GET /api/v1/{context}/sites/{siteId}/...` is now the established
route shape for that granularity, parallel to the `{unitId}` shape used
everywhere else.

## 3. Hosted-service check — re-confirmed directly, right before wiring

Per instruction, re-read `AddEmergencyPreparednessInfrastructure` directly
immediately before wiring anything into the BFF, rather than relying on the
earlier investigation-pass finding alone: still zero
`AddHostedService<...>()` calls. Phase-2-style, shares `AlarmManagementDb`
(ADR-025). No opt-out parameter needed.

## 4. The `EvacuationRoute` → `RadiationMonitoring.RadiationZone` join

Included as-is in the `open-or-restricted` endpoint — it is exactly what
`GetOpenOrRestrictedRoutesCrossingZonesQuery` already computes (the atlas's
own verification query 3, verbatim), not a new join invented for this
slice. Genuinely useful without overreaching: an evacuation-route screen
that didn't show which radiological zones a route crosses would be missing
the one piece of information (`IsAvoidIfAlarmed`) that makes the route
data actionable during an actual radiological event.

## 5. Build and full regression suite

```
dotnet build src/Hosts/Nexus1.Bff/Nexus1.Bff.csproj → 0 Warning(s), 0 Error(s)
dotnet build Nexus1.Runtime.sln                     → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln                       → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged from the EventManagement slice's baseline — no regressions.

## 6. Memory discipline — a real stop-and-resume this task

First check, before any host start: **1.66 GB → 1.66 GB → 1.63 GB → 1.62 GB**
across four readings — below the ~1.7 GB threshold and trending down.
Stopped rather than pushing through, reported the numbers, and waited.
Re-checked once memory had recovered: **2.04 GB → 2.07 GB**, stable and
comfortably above threshold — proceeded with the host start only then.

## 7. Real host, real database — live evidence (subset composition: ReactorFleet + EmergencyPreparedness)

All EmergencyPreparedness tables (entities and lookups) had **zero rows** —
no dev-run residue from any prior slice. Seeded minimal dev data:

- One `EmergencyPlan` (`PLAN-SITE1-001`, site 1, `ACTIVE`) with one revision.
- One `AssemblyPoint`, linked to the real, pre-existing `RadiationMonitoring.RadiationZone`
  row (`ZONE-UNIT-1`, id 1 — not fabricated).
- Two `EvacuationRoute` rows, deliberately one `OPEN` and one `RESTRICTED`
  (to exercise the query's `IN ('OPEN','RESTRICTED')` filter meaningfully),
  both crossing that same real radiation zone via `EvacuationRouteZone`.
- Two `EmergencyResource` rows, deliberately **one with a `ResourceReadinessCheck`
  and one without** — to exercise the dashboard's nullable
  "never assessed" case, per instruction.
- One `Exercise` with one `ExerciseObservation` where
  `CorrectiveActionRequired = 1`.

### `GET /health/ready`

```
Healthy
HTTP 200
```

### `GET /api/v1/emergency-preparedness/sites/1/plans`

```json
[{"planCode":"PLAN-SITE1-001","planStatus":"ACTIVE","currentRevisionNumber":1,"revisionRowCount":1}]
```

HTTP 200.

### `GET /api/v1/emergency-preparedness/sites/999/plans` (nonexistent site)

```json
[]
```

HTTP 200 — empty array, not an error.

### `GET /api/v1/emergency-preparedness/exercises/corrective-observations`

```json
[{"exerciseCode":"EX-2026-001","exerciseName":"Q3 Evacuation Drill","correctiveObservationCount":1}]
```

HTTP 200.

### `GET /api/v1/emergency-preparedness/evacuation-routes/open-or-restricted`

```json
[{"routeCode":"ROUTE-SITE1-001","routeStatus":"OPEN","radiationZoneCode":"ZONE-UNIT-1","isAvoidIfAlarmed":true},
 {"routeCode":"ROUTE-SITE1-002","routeStatus":"RESTRICTED","radiationZoneCode":"ZONE-UNIT-1","isAvoidIfAlarmed":false}]
```

HTTP 200. Both status values present, both correctly crossing the real
radiation zone, `isAvoidIfAlarmed` correctly differentiated per route.

### `GET /api/v1/emergency-preparedness/resource-readiness-dashboard`

```json
[{"siteId":1,"resourceType":"PPE","readinessStatus":null,"resourceCount":1},
 {"siteId":1,"resourceType":"PPE","readinessStatus":"READY","resourceCount":1}]
```

HTTP 200. Two distinct groups for the same site/type, exactly as the data
was seeded: one resource with no readiness check ever recorded
(`readinessStatus: null`) grouped separately from the one that does
(`"READY"`) — confirms the correlated-subquery "latest per parent row"
pattern and the nullable-readiness-status grouping live, not just by
reading the finder's own doc comment.

### Login verification

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```

```
login_name  program_name                            status
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
nexus1_app  Core Microsoft SqlClient Data Provider   sleeping
```

**Two sessions** — matching exactly the two composed contexts
(`ReactorFleet`, `EmergencyPreparedness`), both under `nexus1_app`.

`sys.databases` confirmed all `ONLINE` after host stop; no corruption.

## Summary

Sixteen vertical slices now exist in `Nexus1.Bff`. EmergencyPreparedness is
the second backend-only slice (after EventManagement) — real, useful
domain data (plan status, resource readiness, exercise/drill history,
evacuation routes crossing radiological zones) exposed honestly with no
current console screen, confirmed by exhaustive sitemap search before
building anything. It is also the **first site-scoped context** in this
BFF, a genuinely different granularity from every prior unit-scoped or
fleet-wide slice — routed honestly with a `{siteId}` parameter only where
the underlying query actually has that scope, and as a flat fleet-wide
listing everywhere it doesn't. All four endpoints reuse already-existing,
unmodified Application handlers with zero new Application-layer code. A
real memory-pressure stop-and-resume occurred mid-task and was handled per
the established discipline: reported the declining numbers, held off
starting the host, and proceeded only once a fresh check confirmed
recovery and stability.
