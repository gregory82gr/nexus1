# Evidence: Angular console, Ch. 20 — Zone Access (reshaped)

## Scope

One real screen, shared by both nav routes, plus one genuine, minimal
backend addition:

1. **BFF**: `GET /api/v1/radiation-monitoring/zones` — a new thin route
   wrapping the existing `GetActiveRadiationZonesQueryHandler`/
   `ActiveRadiationZoneDto`, same pattern as every prior slice's own
   thin addition.
2. `ZoneRegistryComponent` (`features/zone-registry/`) — serves both
   `access-presence` ("Live Presence") and `access-matrix` ("Permissions
   Matrix"), wired to the new route.

## Investigation: the first cluster where BOTH book screens come up empty

Ch. 20's own source material has the same boundary as Ch. 16-19: *"Volume
III has no access-control endpoint; the tags, zones, and movements are
generated."* That alone wasn't new — three prior clusters found the same
about the book's own source and still had at least one screen with a
clean real analogue (Reactor Instrumentation, Ageing & Degradation,
Fleet Overview). **This is the first cluster where neither of the book's
two screens has anything real to build against, anywhere in this
solution:**

- **Permissions Matrix** (which entity *class* may enter which zone — a
  policy table with no individuals, by design) needs a class-to-zone
  authorization mapping. Confirmed the already-known finding still
  holds: Security's entire domain is application-level RBAC (roles,
  permissions, user lock, preferences) — no zone concept anywhere.
  Checked further than Security alone, per the task's own instruction: a
  solution-wide search (`class.*Presence`, `EntryLog`, `AccessLog`,
  `class.*Badge`, `AuthorizedClass`, `ZoneEntry`, `EntityClass`) across
  every context's domain layer came back with **zero matches**. No
  class-to-zone authorization concept exists anywhere in this codebase.
- **Live Presence** (named people, tag id, real-time zone, a violation
  pill, and Part IV's first write — acknowledging an access alarm) needs
  a presence/badge/entry-log concept. Same solution-wide search, same
  result: nothing.

**What the investigation found instead, in a place the task explicitly
asked to check**: `RadiationMonitoring.RadiationZone` is a genuine
physical-zone entity — `Code`, `Name`, a type/status/classification
lookup chain, `IsEntryControlled`, `RequiresDosimeter`, an optional home
unit. It doesn't support either book screen (no class or person is
attached to a zone anywhere in this model), but it's real, physical
zone data, and a fleet-wide query already existed for it
(`GetActiveRadiationZonesQuery`/`ActiveRadiationZoneDto` — atlas
C.13.5.2 query 1), already registered in DI, never mapped to a BFF
route. The per-unit sibling of this same data (`UnitRadiationZoneDto`)
is already exposed and already used by the Overview screen (`{"code":
"ZONE-UNIT-1","classification":"LOW","status":"POSTED"}`), confirming
this isn't new/unproven data, just a fleet-wide view of it that had
never been wired up.

**Decision applied**: build neither book screen as designed — that would
mean fabricating a class-authorization table or a presence tracker from
nothing. Add the one thin BFF route for the real zone registry, and
build a single, honestly-named `ZoneRegistryComponent` that both nav
routes point at — the same consolidation shape as the Reactor cluster
(distinct nav labels over one real, shared data source), rather than two
screens dressed up as things they aren't. The component's own header
pill reflects which nav entry was clicked (`Live Presence` vs
`Permissions Matrix`), for orientation only, exactly like
`focusLabel` did for Reactor Instrumentation — never a data filter, and
the screen states plainly, in both places, why it isn't showing either
of the book's own two concepts.

## The new BFF route

```csharp
app.MapGet("/api/v1/radiation-monitoring/zones", async ([FromServices] GetActiveRadiationZonesQueryHandler handler, CancellationToken cancellationToken) =>
{
    var result = await handler.Handle(new GetActiveRadiationZonesQuery(), cancellationToken);
    return Results.Ok(result.Value);
});
```

Fleet-wide, no `{id}` parameter — the query itself takes none. No DI/
infrastructure changes needed.

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → 37/37 assemblies green, 869/869 total
```

Same 869 baseline as every prior slice — confirmed genuinely unchanged.

## Frontend: what was built

- `core/api/radiation-zones-api.ts` — `ActiveRadiationZone` (mirrors
  `ActiveRadiationZoneDto` exactly) and `RadiationZonesApi.getActiveZones()`.
- `features/zone-registry/zone-grouping.ts` — pure `groupByClassification()`,
  same discipline as every prior grouping module in this app: group by
  the real `Classification` field, never by an invented entity-class
  taxonomy.
- `features/zone-registry/zone-registry.ts/.html/.scss` — loading/error/
  loaded state over the real endpoint; a `focusLabel` `@Input()` for
  which nav entry was clicked, purely for the header pill; zones grouped
  and rendered with real code/name/status/optional home-unit tag — no
  class column, no person, no violation flag, because none of that data
  exists.

## Tests

```
npx jest → 151/151 passing (was 144; 7 new specs)
```

- `zone-grouping.spec.ts` — real-classification grouping, sorted,
  deterministic, empty-list case.
- `zone-registry.spec.ts` — loading/error/loaded states, fetches the
  fleet-wide endpoint with no id, groups correctly, `focusLabel` changes
  the header without changing the underlying data (same test shape as
  Reactor Instrumentation's own `focusLabel` spec), real error state on
  an unreachable endpoint.

Production build:

```
npx ng build → 0 errors, 0 warnings. zone-registry compiles to one
               shared lazy chunk (~1.9 KB transfer) referenced by both
               its routes.
```

Both gate runs (Jest, then the .NET build+test) were run sequentially,
not concurrently, per the resource-contention lesson from the Plant
Lifecycle slice.

## Live evidence — real host, real database, real screenshots

Memory checked before starting both processes (2.30 GB, healthy).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=RadiationMonitoring`, `__1=ReactorFleet`; `ng
serve --port 4200` alongside it.

```
GET /health/ready                                   → Healthy, HTTP 200
GET /api/v1/radiation-monitoring/zones (one zone)    →
  [{"code":"ZONE-UNIT-1","name":"Demonstrator Zone for Unit 1","unitCode":"UNIT-1","classification":"LOW","status":"POSTED"}]
```

Same zone seeded during the earlier RadiationMonitoring slice (visible
already in the Overview screen's own composed response) — no
reseeding needed to prove the route works at all. **Seeded one more real
zone** with a different classification, to demonstrate the grouping
logic against more than a single group:

```sql
SET QUOTED_IDENTIFIER ON;
INSERT INTO RadiationMonitoring.RadiationAreaClassification (RadiationAreaClassificationId, Code, Name, DisplayOrder, IsActive, CreatedAtUtc) VALUES (2, 'HIGH', 'High Radiation Area', 2, 1, SYSUTCDATETIME());
INSERT INTO RadiationMonitoring.RadiationZone (RadiationZoneId, RadiationZoneTypeId, RadiationZoneStatusId, RadiationAreaClassificationId, Code, Name, IsEntryControlled, RequiresDosimeter) VALUES (2, 1, 1, 2, 'ZONE-CONTAINMENT-1', 'Containment High-Radiation Area', 1, 1);
```

```
GET /api/v1/radiation-monitoring/zones (after seeding) →
  [{"code":"ZONE-UNIT-1",...,"unitCode":"UNIT-1","classification":"LOW","status":"POSTED"},
   {"code":"ZONE-CONTAINMENT-1",...,"unitCode":null,"classification":"HIGH","status":"POSTED"}]
```

`/access-matrix` rendered live (`get_page_text`, no console errors):
header pill `PERMISSIONS MATRIX`, `2` zones registered, across `2`
classifications, `HIGH`/`LOW` groups each showing their one real zone.
`/access-presence` rendered live with the identical zone data, header
pill `LIVE PRESENCE` — confirming `focusLabel` again changes only the
orientation label, not the underlying data, live, not just asserted from
the spec.

### Screenshots

- `permissions-matrix-registry.png` — `/access-matrix`, real two-zone
  registry grouped by classification.
- `live-presence-registry.png` — `/access-presence`, same real data,
  different header label.

Both reviewed directly: full-width shell, clean layout, no cramped
columns. The sidebar's own active-state fix (from an earlier slice)
correctly distinguishes "Live Presence" active from "Permissions Matrix"
active even though both routes render the same component.

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

Checked the real domain across every context, not just Security's own
already-known RBAC-only finding, per the task's explicit instruction, and
found that neither of Ch. 20's two screens has any real backing anywhere
in this solution — the first cluster where that's true of both screens
at once. Rather than fabricate a class-authorization matrix or a
presence tracker, built one honestly-named zone registry from the one
real, thematically-adjacent capability that does exist
(`RadiationMonitoring.RadiationZone`, fleet-wide), added via one thin BFF
route wrapping an Application-layer query that already existed and was
already registered, and pointed both original nav routes at it — the
same consolidation discipline as the Reactor cluster, applied here to a
case where both original concepts, not just some of them, turned out to
have nothing real behind them.
