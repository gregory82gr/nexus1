# Evidence: hardening item 1 — Instrumentation `UnitCode` vs. `{id:int}` inconsistency

## Investigation

The tracked item (recorded in `2026-08-23-bff-instrumentation-model-analysis-slice.md`)
named `GetActiveHistorizedSignalsForUnitQuery` and
`GetOpenSignalQualityEventsForUnitQuery` as keyed by `UnitCode` (string),
inconsistent with every BFF route's `{id:int}` convention.

Checked directly, rather than assumed:

1. **The int-keyed sibling fix was already built**, in the same slice that
   recorded the item. `GetUnitSignalReadingsQuery(int UnitId)` and
   `GetUnitSignalQualityEventsQuery(int UnitId)` already exist, with doc
   comments stating explicitly: "added for route-shape consistency with
   every other BFF endpoint." This is the exact sibling-method pattern
   used everywhere else in the 17-slice effort (never rename/retype an
   atlas-verbatim query; add a minimal new sibling for the real BFF need).
2. **The BFF already uses only the int-keyed siblings.** Confirmed in
   `Program.cs`:
   ```
   MapGet("/api/v1/instrumentation/units/{id:int}/signals", ..., GetUnitSignalReadingsQueryHandler, ...)
   MapGet("/api/v1/instrumentation/units/{id:int}/signal-quality", ..., GetUnitSignalQualityEventsQueryHandler, ...)
   ```
   Both routes are already `{id:int}`, matching every other slice.
3. **Nothing outside their own component tests calls the original
   UnitCode-keyed queries.** Searched the whole solution
   (`src/` and `tests/`) for callers of `GetActiveHistorizedSignalsForUnitQuery`/
   `GetOpenSignalQualityEventsForUnitQuery`: the only matches are their own
   definition files, their own handlers, and
   `GetActiveHistorizedSignalsForUnitQueryHandlerTests.cs`/
   `GetOpenSignalQualityEventsForUnitQueryHandlerTests.cs`.
   `Nexus1.ModularRuntime` doesn't reference them either.

## Conclusion: already resolved — no fix needed

The BFF's own route surface is already 100% `{id:int}`-consistent. What
remains is two atlas-verbatim, `UnitCode`-keyed queries that exist purely
to match the Schema Atlas's own literal query text and are exercised only
by their own component tests — the same shape every other sector's
atlas-verbatim query takes once a BFF-facing int-keyed sibling exists
alongside it (e.g. Reporting's `GetCaseSummariesForUnitQuery` next to
whatever atlas-verbatim query it may have had, CorePlatform's fleet-wide
queries, etc.). This was correctly flagged as worth checking, but on
investigation is not a real inconsistency in the BFF's own surface — it
was misfiled as an open gap when it had, in effect, already been closed by
the sibling-method work in the same slice that recorded it.

**No code change made.** No migration, no new query, no route change —
this item required investigation only, and the investigation's own
conclusion is the resolution.

## Build and test suite

Not re-run for this item alone (no code touched); confirmed green as part
of the shared gate covering all three hardening items in this pass (see
the CorePlatform and LINQ-pattern reports) — 869/869, 37/37 assemblies,
0 failed.
