# ADR-030: BFF layer walking skeleton — ReactorFleet vertical slice

## Status

Accepted.

## Context

Backend work (Phase 1's distributed slice and all eleven Phase 2 monolithic
sectors) is complete. The project now begins a Backend-for-Frontend (BFF)
layer to serve a future Angular console. The Angular UI will reuse a
companion book's screens, routing, and design system, but its data-access
layer targets a contract of this project's own design, shaped around what
each screen actually needs — not a generic per-entity CRUD wrapper over
every context.

This is also the point at which ADR-007's own deferral condition is met.
ADR-007 deferred the Query BFF because `GetActiveAlarmsForUnitQuery`/
`GetAnalysisByIdQuery` had no external consumer yet; component tests alone
proved them at the Application layer. That condition no longer holds — a
real external consumer (the future Angular console) now exists, and this
ADR is that reversal being acted on.

## Decision

**Build `Nexus1.Bff`, a new ASP.NET Core Minimal API host, as a walking
skeleton limited to one vertical slice: ReactorFleet's two read screens.**

### Composition

- `Nexus1.Bff` calls context Application layers in-process, exactly the way
  `Nexus1.ModularRuntime` does today — no HTTP hop for any in-process
  context. This walking skeleton composes only `Nexus1.ReactorFleet.Application`/
  `.Infrastructure`; every other context is added the same way, one slice at
  a time, in future work.
- `RootCause` is untouched. It remains the one out-of-process service
  (ADR-001) and nothing about how it's reached changes here — this slice
  simply doesn't need it yet, since neither endpoint touches root-cause
  data. When a future slice does need it, the BFF reaches it the same way
  anything else reaches it today, not a new integration this ADR invents.
- `Nexus1.ArchitectureTests`' `DependencyLawTests.Classify` now recognizes
  `Nexus1.Bff` as a `Host` project (alongside the existing special case for
  `Nexus1.ModularRuntime`), so it is unconstrained by the Application/
  Infrastructure cross-context rules the same way every other host already
  is — a composition root wires contexts together by design.

### Contract style

REST, Minimal API endpoints, one endpoint per screen need, not one endpoint
per entity. The two endpoints in this slice:

- `GET /api/v1/reactor-fleet/units` — fleet-overview screen: a minimal
  summary row per unit.
- `GET /api/v1/reactor-fleet/units/{id}` — unit-detail screen: the summary
  fields plus recent power history, the natural thing a detail view wants
  that an overview row doesn't.

No MediatR. Both endpoints resolve a concrete query-handler type from DI and
call `.Handle(...)` directly, the same hand-rolled dispatch pattern used
everywhere else in this project (ADR-002-amend).

### A real gap surfaced while building this: ReactorFleet had no queries

`Nexus1.ReactorFleet.Application` previously contained exactly one
handler — `RecordUnitPowerSnapshotCommandHandler` — and zero queries.
`IRepository<TRoot, TId>` is deliberately Add/Get-by-id only (Blueprint_to_Core's
Add/Get split), so it cannot list units at all. Building these two
endpoints required adding, for the first time in ReactorFleet:

- `IUnitFleetFinder`, a read-side Finder interface matching the pattern
  Organization's Application layer already established (`ISitePlantHierarchyFinder`,
  `IStaffingGapFinder`, etc.) — a Finder reads projected DTOs directly from
  the DbContext; a Repository loads and mutates aggregates. These are
  deliberately separate interfaces, not one interface doing both jobs.
- `GetUnitsQuery`/`GetUnitsQueryHandler` and `GetUnitByIdQuery`/
  `GetUnitByIdQueryHandler`, using the existing (but until now, unused by
  ReactorFleet) `IQuery<TResponse>`/`IQueryHandler<TQuery, TResponse>`
  contracts from `Nexus1.BuildingBlocks.Application`.
- `EfUnitFleetFinder` in `Nexus1.ReactorFleet.Infrastructure`, implementing
  the finder via correlated-subquery LINQ projections (the same
  `x.SomeValueObject.Value`-in-projection pattern `EfSitePlantHierarchyFinder`
  already uses for `PlantSummaryDto`), not a raw SQL query or an in-memory
  fetch-everything shortcut.

This matches the project's own verification discipline (CLAUDE.md): the
task's premise assumed these queries already existed; they didn't, and the
gap is recorded here rather than silently patched over.

### A second real gap: `Unit` has no "plant" or "status" field

`ReactorFleet.Unit` is Phase 1's deliberately bare-identity slice (ADR-003):
`Id`, `Code`, `Name` — nothing else. It has no FK, passport or otherwise, to
any plant/site concept (that lives in `OrganizationDb`, a different physical
database, ADR-017) and no status column of any kind. The fleet-overview
screen's field list therefore does **not** include a plant or a status field
— only `Code`, `Name`, and the latest recorded `PowerPercent`/timestamp
(both nullable, since a unit may have zero recorded snapshots). Inventing a
passport reference to Organization or fabricating a status value not backed
by any real column would be exactly the kind of undisciplined cross-context
shortcut ADR-028 exists to guard against. This is recorded as a known gap,
not silently worked around:

**Reversal condition**: once `ReactorFleet.Unit`'s own domain model grows
a real plant/site reference or a real status concept (a future ADR's
decision, not this one), the fleet-overview endpoint's DTO gains that field
using the same passport-or-FK discipline already applied everywhere else in
this project. Until then, the endpoint returns what genuinely exists.

### Authentication — explicitly out of scope

The BFF starts fully unauthenticated. No login, no token validation, no
authorization policy of any kind protects either endpoint.

**Reversal condition**: revisit once the Angular console needs real login.
`Security.ApplicationUser` already exists and will be the eventual source
of identity; wiring authentication into the BFF (JWT bearer validation,
policy-based authorization per screen, etc.) is separate future work with
its own ADR, not bundled into this walking skeleton.

### Health check

The BFF host has a direct DB dependency (it composes `ReactorFleetInfrastructure`,
including `ReactorFleetDbContext`, in-process) — so it gets the same
`DbContextHealthCheck<ReactorFleetDbContext>` pattern every other host
uses (checks pending migrations, not just connectivity), registered under
`/health/ready`; `/health/live` stays dependency-free, matching ADR-007's
existing liveness/readiness split.

## Consequences

- New project `Nexus1.Bff` (`src/Hosts/Nexus1.Bff`), nested under the
  solution's existing `Hosts` folder alongside `Nexus1.ModularRuntime` and
  `Nexus1.RootCause.Host`.
- `Nexus1.ReactorFleet.Application` gained its first queries; `Nexus1.ReactorFleet.Infrastructure`
  gained its first Finder. No existing ReactorFleet command/handler/repository
  changed.
- `Nexus1.ArchitectureTests.DependencyLawTests.Classify` updated to
  recognize `Nexus1.Bff` as a `Host`.
- No MediatR, no messaging, no OpenTelemetry added to `Nexus1.Bff` in this
  pass — ADR-027's own deferral (Phase 2 sectors have no external traffic
  yet) is not reversed by this ADR for ReactorFleet's Application/Infrastructure
  layers; only the BFF host itself exists now, with no tracing added to it
  either. If the BFF becomes the "real external traffic" trigger ADR-027
  names, that's a separate, explicit follow-up, not assumed here.
- No authentication middleware, no CORS policy, no rate limiting — an
  intentionally minimal walking skeleton, not a production-hardened edge.

## Rejected alternatives

- **Generic per-entity CRUD endpoints** (e.g. a single `/api/v1/units`
  resource supporting arbitrary filtering/paging). Rejected per the
  decision already made: screen-first design, one endpoint per real screen
  need, not a generic data-access facade the Angular console would have to
  shape client-side anyway.
- **Fabricating a plant/status field on the summary DTO to match the
  screen's originally assumed shape.** Rejected — no such data exists on
  `Unit` today; inventing it would misrepresent what the tool actually
  knows, the same discipline this project already applies to verdicts and
  cross-context references.
- **Wiring authentication now, even minimally (e.g. a hardcoded API key).**
  Rejected — explicitly out of scope per the decision already made; a
  half-measure auth mechanism would need to be un-done rather than extended
  once real login lands.

## Evidence required

- Real BFF host started against the real ReactorFleet database (shares
  `AlarmManagementDb`, ADR-006); both endpoints called live, actual JSON
  responses captured.
- `GET /health/ready` returns `200 Healthy` with the `reactorfleet-db`
  check passing.
- Full regression suite green (no existing test broken by the new
  queries/finder or the `DependencyLawTests.Classify` change).
- Confirmation that `Nexus1.RootCause.Host`'s own integration path is
  untouched — no file under `src/Contexts/RootCause` or
  `src/Hosts/Nexus1.RootCause.Host` modified by this change.
