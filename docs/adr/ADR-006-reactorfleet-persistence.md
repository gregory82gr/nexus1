# ADR-006: ReactorFleet shares AlarmManagement's database

## Status

Accepted.

## Context

CLAUDE.md §5 step 3 requires `AlarmManagementDb` and `RootCauseDb` as
**separate databases**, per the book's data-ownership rule — each
independently-deployed service owns its own physical database, no
cross-context foreign keys at the database level, only passport IDs.
RootCause gets `RootCauseDb` because RootCause is the one service ADR-001
extracts to its own independently-deployed host in Phase 1.

ReactorFleet was left open, deferred from ADR-001-amend/ADR-003 to this
point: *"ReactorFleet persistence can share the modular runtime's database
initially, or get its own — decide via ADR when you get there."*

The two options aren't symmetric with RootCause's situation. RootCause's
own database exists because RootCause is genuinely, physically a separate
deployment (`Nexus1.RootCause.Host`, its own process, its own connection
string, its own operational lifecycle). ReactorFleet is not — per
ADR-001-amend, it's composed **in-process** into `Nexus1.ModularRuntime`
alongside AlarmManagement, in the same process, same deployment unit, same
operational lifecycle as AlarmManagement. The book's DB-per-service
argument (deployment isolation, independent scaling, independent failure
domains) doesn't yet apply to a context that isn't independently deployed —
it would be provisioning infrastructure-level isolation for a boundary that
doesn't exist yet at the process level either.

## Decision

**ReactorFleet shares AlarmManagement's physical database** (referred to
here as `AlarmManagementDb`, since that's the one per-context database
already associated with anything composed into `Nexus1.ModularRuntime`),
using its **own SQL schema** — `ReactorFleet.Unit`, `ReactorFleet.
UnitPowerSnapshot` — exactly matching the Schema Atlas's per-sector-schema
naming, not folded into the `AlarmManagement` schema.

Concretely:
- `Nexus1.ReactorFleet.Infrastructure` gets its own `ReactorFleetDbContext`
  (not a shared context with AlarmManagement — each bounded context keeps
  its own `DbContext`, even when two contexts' `DbContext`s point at the
  same physical database via the same connection string). This keeps
  ReactorFleet's persistence code fully separable from AlarmManagement's:
  splitting them onto separate databases later is a connection-string and
  migration-history change, not a redesign.
- Each `DbContext` gets its own EF Core migrations history table name
  (`__EFMigrationsHistory_ReactorFleet`,
  `__EFMigrationsHistory_AlarmManagement`) to avoid two `DbContext`s
  sharing one physical database from colliding on the default
  `__EFMigrationsHistory` table.
- No cross-schema foreign keys are added between `ReactorFleet.*` and
  `AlarmManagement.*` tables at the database level — the same passport-ID
  discipline the Domain layer already enforces in code (ADR-004: no
  `AlarmManagement.Domain` → `ReactorFleet.Domain` reference) is mirrored
  at the schema level. Sharing a physical database is purely a deployment
  convenience, not a licence to add real FKs across the boundary.

## Consequences

- One fewer database to provision/manage for Phase 1's local development
  and testing.
- If ReactorFleet is ever extracted to its own independently-deployed host
  (the ADR-001-amend reversal condition), migrating its schema to its own
  physical database is a connection-string change plus a data migration,
  not a code redesign — `ReactorFleetDbContext` already only knows about
  `ReactorFleet.*` tables.
- Anyone reading the connection strings later must not assume "same
  database" implies "shared ownership" — the schema separation and the
  no-cross-schema-FK rule are load-bearing, not cosmetic.

## Rejected alternatives

- **Give ReactorFleet its own `ReactorFleetDb`.** Rejected: matches true
  microservice DB-per-context discipline more strictly, but provisions
  deployment-level isolation for a context that isn't independently
  deployed yet — the same over-provisioning restraint already applied to
  ReactorFleet's domain scope (ADR-003) and AlarmManagement's flood
  threshold (ADR-004) argues against paying this cost before there's a
  real consumer for it (i.e., before ReactorFleet is actually extracted).
- **Fold ReactorFleet's tables into the `AlarmManagement` SQL schema**
  (same schema, not just same database). Rejected: contradicts the Schema
  Atlas's own per-sector-schema convention and would make a future
  extraction to ReactorFleet's own database a genuine redesign (splitting
  tables out of a mixed schema) rather than a connection-string change.

## Reversal condition

Revisit when ReactorFleet is extracted to its own independently-deployed
host (per ADR-001-amend's reversal condition) — at that point
`ReactorFleetDbContext` moves to its own connection string and, if desired,
its own physical database, with no schema-level rework required.

## Evidence required

- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.ReactorFleet.Infrastructure`, targeting the `ReactorFleet` SQL
  schema, independent of `Nexus1.AlarmManagement.Infrastructure`'s own
  migration history.
- `dotnet build` clean with both `DbContext`s coexisting.
