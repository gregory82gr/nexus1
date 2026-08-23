# Evidence: hardening item 3 — the recurring LINQ→SQL translation bug

## Investigation

Read all three finders that reference the issue directly, rather than
trusting their own cross-referencing comments at face value:
`Nexus1.Robotics.Infrastructure/.../EfLatestHealthSnapshotFinder.cs`,
`Nexus1.RadiationMonitoring.Infrastructure/.../EfLatestReadingPerMonitorFinder.cs`,
`Nexus1.EmergencyPreparedness.Infrastructure/.../EfResourceReadinessDashboardFinder.cs`.

**Finding: genuinely one root cause, confirmed by reading the actual
queries, not just their comments.** All three are instances of the same
EF Core (`Microsoft.EntityFrameworkCore.SqlServer` 8.0.11) limitation:
it cannot translate a query where a JOIN sits directly downstream of an
ordered "latest per group" aggregate — whether written as
`GroupBy().OrderByDescending().First()` (Robotics' original failing
attempt), or as a further join against a `let`-bound correlated
subquery's result, or as a `LEFT JOIN` whose own key selector is itself
an ordered correlated subquery (EmergencyPreparedness's trigger — a
syntactically different shape, same underlying gap).

Two safe shapes were independently converged on across the three sectors,
without a shared reference existing before now:

1. Fleet-wide "need the full latest row": `let`-bound correlated
   subquery, null-checked, joined exactly once directly against that
   result (Robotics' and RadiationMonitoring's fleet-wide queries — proven
   to translate to `OUTER APPLY`).
2. Per-unit "need lookup codes": resolve each field as its own
   independent scalar correlated subquery, then resolve lookup-table
   codes via a **separate in-memory dictionary pass** after materializing
   rows — never join a lookup table onto an ordered subquery's result
   inside the same query (Robotics' and RadiationMonitoring's per-unit
   queries; EmergencyPreparedness's dashboard nests the lookup as a second
   scalar subquery instead, for the same reason).

No dedicated ADR previously recorded this as one cross-cutting pattern —
each occurrence only cross-referenced the prior one informally in code
comments. `ADR-023` (cited by RadiationMonitoring's comment) is Robotics'
own sector-scope ADR and does not itself document the query pattern.

## Decision: document the pattern, do not build a shared abstraction

Considered a generic reusable "latest per parent" LINQ helper/extension
method, per the task's own prompt to weigh it. **Rejected**, for two
concrete reasons (not just "keep it simple"):

- The three call sites' field lists, entity types, and DTO shapes differ
  substantially enough that a generic helper would need heavy expression-
  tree composition to serve all three — itself a novel, unproven LINQ
  shape that risks introducing a *new* translation failure while trying
  to prevent a known one, with less visibility into which specific shape
  broke.
- Each of the three existing call sites already reads clearly with its
  own doc comment naming the constraint; a shared helper adds indirection
  without saving meaningful code given how much of each query is already
  bespoke to its own fields.

Recorded as **`docs/adr/ADR-031-latest-per-parent-linq-translation-pattern.md`**
— the one place the constraint and both safe shapes now live, so a fourth
sector cites ADR-031 directly instead of re-deriving the fix or chaining
through three separate finder files' comments.

## No code change

All three existing call sites already use one of the two safe shapes —
there is no live bug to fix today, only a pattern to name and record so
it isn't rediscovered a fourth time. Confirmed by re-reading each
implementation directly.

## Build and test suite

Not re-run for this item alone (documentation-only, zero code touched);
covered by the same shared gate as the CorePlatform fix in this hardening
pass — 869/869, 37/37 assemblies, 0 failed.

## Summary

Three hardening items handled individually, per instruction:

1. Instrumentation `UnitCode`/`{id:int}` — already resolved by the
   existing sibling-method pattern; no fix needed, recorded as such.
2. CorePlatform's missing CHECK constraint — safe, atlas-verbatim,
   additive migration added and verified live against real data.
3. The recurring LINQ translation bug — confirmed as one real root cause
   across three sectors; documented in a new ADR rather than built into a
   reusable abstraction, since the abstraction's own risk outweighed its
   benefit given how few call sites exist and how different their shapes
   are.

None of the three required stopping to report a bigger-than-expected
scope — all three landed at or below the size this task anticipated.
