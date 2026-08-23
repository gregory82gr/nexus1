# ADR-031: "Latest per parent" LINQ queries must not join after an ordered correlated subquery

## Status

Accepted. Root-caused as one pattern after independently recurring three
times; no code change required (every existing occurrence already uses a
safe shape) — this ADR exists to stop it recurring a fourth time.

## Context

A hardening pass across the BFF effort's tracked-but-not-fixed items asked
whether an EF Core LINQ-translation failure hit in three different Phase 2
sectors (Robotics, RadiationMonitoring, EmergencyPreparedness) was
genuinely the same root cause each time, or three coincidentally
similar-looking but distinct issues. Read all three finder implementations
directly rather than assumed from their own cross-referencing comments.

**Robotics** (`EfLatestHealthSnapshotFinder`, the original occurrence): an
early attempt to compute "each robot's latest health snapshot, joined to
its battery/communication status codes" using
`GroupBy(s => s.RobotId).Select(g => g.OrderByDescending(...).First())`
followed by a join threw `InvalidOperationException:
ProjectionBindingExpression could not be translated`. Fixed by switching to
a correlated subquery (`let latest = ...Where(...).OrderByDescending(...).
FirstOrDefault(); where latest != null; join ... on latest!.X equals ...`)
for the fleet-wide query — this shape translates to a SQL Server `OUTER
APPLY` and works. But the sector's own **per-unit** query
(`GetRobotStatusForUnitAsync`) went one step further and avoided even that
join-after-subquery shape, instead resolving each field as an independent
scalar subquery and looking up lookup-table codes via a separate
in-memory `Dictionary` pass after materializing the rows.

**RadiationMonitoring** (`EfLatestReadingPerMonitorFinder`): explicitly
cites Robotics' finding and mirrors both of its shapes — the fleet-wide
query uses the same `let`-plus-join pattern (proven safe by Robotics), and
the per-unit query uses the same per-field-scalar-subquery-plus-in-memory-
dictionary pattern, for the identical reason.

**EmergencyPreparedness** (`EfResourceReadinessDashboardFinder`): a
differently-shaped trigger of the same underlying limitation — here the
failing attempt was a genuine `LEFT JOIN` whose **outer key selector was
itself a correlated, ordered subquery**
(joining `ResourceReadinessCheck`'s latest row to `ReadinessStatus` by
first computing "latest check per resource," then joining that result to
the status lookup). The fix nests the lookup as a second scalar subquery
*inside* the `OrderByDescending().Select().FirstOrDefault()` chain, so no
join against an ordered-subquery result is ever attempted at all.

**Conclusion: one real root cause, not three coincidences.** EF Core's
LINQ provider (as pinned in this solution, `Microsoft.EntityFrameworkCore.
SqlServer` 8.0.11) cannot translate a query where a `JOIN` — whether
written as LINQ `join`, or as an equi-join implied by a correlated
subquery used as a join key — sits directly downstream of an ordered
"latest per group" aggregate (`GroupBy`+`OrderByDescending`+`First`, or an
ordered correlated subquery consumed by a further join). The three
call sites are the same limitation surfacing through three syntactically
different LINQ shapes that all reduce to the same underlying SQL
translation gap.

## Decision

### Record the pattern; do not build a shared abstraction

Two safe, already-independently-converged-upon shapes exist and are
already in use everywhere this problem has come up:

1. **Fleet-wide "latest per parent, need the full row":** a correlated
   subquery bound with `let`, consumed and null-checked, *then* joined —
   safe as long as the join happens once, directly against the `let`
   result, not against a second layer of aggregation.
2. **Per-unit "latest per parent, need lookup codes":** resolve every
   needed scalar field as its own independent correlated subquery
   (`.Where(...).OrderByDescending(...).Select(x => x.Field).
   FirstOrDefault()`), then resolve any lookup-table codes via a **separate
   `ToDictionaryAsync` pass** after materializing the rows — never join a
   lookup table onto an ordered subquery's result inside the same query.

**Considered and rejected: a generic reusable "latest per parent" LINQ
helper or extension method.** Rejected for two concrete reasons, not
merely "keep it simple":

- Every real call site's field list, entity types, and DTO shape differ
  substantially (5 fields for RadiationMonitoring's per-unit query, 4 for
  Robotics', a grouped aggregate for EmergencyPreparedness's dashboard) —
  a generic helper would need enough type parameters and expression-tree
  composition to serve all three that it would itself become a fourth,
  novel LINQ shape EF Core has never been proven to translate. Introducing
  one generic abstraction to prevent a translation bug risks *introducing*
  a new translation bug in the abstraction itself, with less direct
  visibility into which specific shape broke.
- The three existing call sites already read clearly on their own, each
  with a doc comment naming the constraint and pointing at this ADR — a
  future author hitting the same wall gets the same information from
  reading the pattern in context as they would from a shared helper's own
  doc comment, without the indirection.

The right level of reuse here is **documented convention**, not code: this
ADR is the one place the constraint and both safe shapes are recorded, so
a fourth sector cites ADR-031 instead of re-deriving the fix or
cross-referencing three separate finder files.

## Verification performed

No code changed — this ADR is a documentation-only artifact confirming an
already-fixed, already-shipped pattern. All three finder implementations
were re-read directly (not assumed from their own comments) to confirm the
shared root cause; no new test was needed since none of the three sites'
behavior changed.

## Consequences

- A fourth sector hitting "latest per parent" should reach for pattern 1
  or 2 above directly, and cite this ADR in its own finder's doc comment,
  rather than rediscovering the failure via a live `InvalidOperationException`
  against LocalDB a fourth time.
- If a genuinely generic helper ever becomes justified (e.g., a fourth and
  fifth occurrence turn out to share an *identical* field shape, not just
  the same translation constraint), revisit the "rejected" decision above
  with that concrete evidence in hand — this ADR does not forbid it
  forever, it records that the evidence for it doesn't exist yet.
