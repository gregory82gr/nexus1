# ADR-007: Hosts get health-check-only Minimal APIs; Query BFF deferred

## Status

Accepted.

## Context

`From_Services_To_Runtime`'s service declaration for RootCause marks
`http: required`. Ch. 11 covers liveness/readiness health-check discipline
for hosted services. Appendix A separately lists a **Query BFF** as its own
edge role, distinct from RootCause's own service declaration — an
aggregating read-side gateway that sits in front of one or more services'
query surfaces, not something a service exposes directly off its own host.

Two things needed deciding before touching `Nexus1.ModularRuntime` or
`Nexus1.RootCause.Host` beyond their current empty Worker Service shells:
what `http: required` actually means for Phase 1, and whether the
Application-layer queries already built (`GetActiveAlarmsForUnitQuery`,
`GetAnalysisByIdQuery`) get an HTTP surface now.

## Decision

**Both hosts get Minimal API health-check endpoints only, for Phase 1** —
liveness (`/health/live`: is the process up) and readiness (`/health/ready`:
can it reach its database), per the book's Ch. 11 discipline. This is what
satisfies RootCause's `http: required` declaration — a health surface, not
a query surface. `Nexus1.ModularRuntime` exposes health checks for both
`ReactorFleetDbContext` and `AlarmManagementDbContext` (both point at the
same physical database per ADR-006, but each is checked independently so a
schema-specific problem is distinguishable); `Nexus1.RootCause.Host`
exposes one for `RootCauseDbContext`.

**No Query BFF project yet.** Appendix A's own framing — a separate edge
role, not part of any one service's declaration — means queries were never
meant to hang directly off `Nexus1.RootCause.Host` either; a BFF is a
distinct aggregating gateway, not "RootCause's queries, exposed." Since
`GetActiveAlarmsForUnitQuery` and `GetAnalysisByIdQuery` already exist and
are tested at the Application layer (component tests, real database), they
don't need an HTTP surface to be useful yet. No project is scaffolded for
a role with nothing behind it — the same "no empty placeholder projects"
discipline already applied to Audit/Compliance/Reporting (CLAUDE.md §2) and
to `Nexus1.Contracts.ReactorFleet` before it had a real type to hold
(ADR-001-amend).

**Minimal APIs, not class-based Controllers.** Consistent with the earlier
hand-rolled-dispatch decision (ADR-002-amend's MediatR note): a couple of
health endpoints don't need an extra class-based indirection layer.
Controllers exist as a valid ASP.NET Core pattern but add ceremony
(`ControllerBase` subclass, attribute routing, MVC middleware
registration) with no payoff at this surface area — the same restraint
principle already applied throughout this project, not a rejection of
Controllers as a pattern for whenever a real query/command HTTP surface
does get built.

## Consequences

- `Nexus1.ModularRuntime` and `Nexus1.RootCause.Host` move from
  `Microsoft.NET.Sdk.Worker` (their scaffold-time template, a
  `BackgroundService` that only logs a timestamp — no real composition) to
  `Microsoft.NET.Sdk.Web`, needed for `WebApplication`/Minimal API/Kestrel.
  The scaffold-time `Worker.cs` in each is removed, not kept alongside —
  it never composed anything real, so there's nothing worth preserving.
- Both hosts' `Program.cs` become genuine composition roots: DI
  registration for their composed contexts' Application/Infrastructure
  layers, `DbContext` wiring, and the two health endpoints — no business
  logic, matching the dependency law already enforced by
  `Nexus1.ArchitectureTests` ("Host projects are composition roots only").
- `GetActiveAlarmsForUnitQuery`/`GetAnalysisByIdQuery` remain reachable
  only from tests and (once wired) other in-process callers until a BFF or
  direct HTTP surface is built for them — an explicit, named gap, not
  something quietly missing.

## Rejected alternatives

- **Scaffold the Query BFF now, even without a real external consumer.**
  Rejected: no current consumer needs it; matches the same restraint
  already applied elsewhere in this project rather than a special case for
  HTTP surfaces specifically.
- **Expose the existing queries directly off `Nexus1.RootCause.Host`**
  (skip the BFF concept entirely, add `MapGet("/analyses/{id}", ...)`
  straight onto the service's own host). Rejected: contradicts Appendix
  A's own edge-role separation — a BFF existing as a distinct architectural
  role in the source material means RootCause's own host isn't the place
  queries are meant to surface, even as a shortcut.
- **Controllers instead of Minimal APIs.** Rejected for the same reason
  MediatR was deferred: no current need justifies the added ceremony.

## Reversal condition

Revisit the Query BFF specifically once there's a real external consumer —
a UI, an external client, a demonstrator scenario — that needs to query
alarms or root-cause analyses over HTTP. This is a deferral, not a
rejection, the same pattern as the MediatR decision (ADR-002-amend): when
that need arrives, decide the BFF's shape (its own host? a gateway
composed differently?) with a fresh ADR rather than backfilling one now for
a consumer that doesn't exist. Revisit Controllers-vs-Minimal-APIs if a
real query/command HTTP surface eventually needs enough structure
(versioning, complex model binding, filters) that Minimal APIs' inline
style becomes unwieldy.

## Evidence required

- Both hosts start and stay running (not crash-loop) against real
  connection strings.
- `/health/live` and `/health/ready` both return `Healthy` for each host,
  verified against the real LocalDB databases already proven in the
  persistence and Application-layer steps — not asserted from a clean
  build alone.
