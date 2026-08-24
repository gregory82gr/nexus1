# Evidence: Angular console, Ch. 16 — Rod Inspection cluster (consolidated)

## Scope

Two real screens, replacing the two placeholders in the Rod Inspection
nav group (`insp-overview`, `ndt-methods`):

1. `AssetConditionComponent` (`features/asset-condition/`) — serves the
   Inspection Overview route, wired to `GET /api/v1/maintenance/units/{id}/assets`.
2. `NdtMethodsComponent` (`features/ndt-methods/`) — serves the NDT
   Methods route, fully client-side, no BFF call.

Rod Type/Film is **not built** — named as a real gap, not silently
dropped.

## Investigation: what Ch. 16 specifies, and why this port diverges further than the Reactor cluster did

Ch. 16 is three screens: Inspection Overview (a rod list), Rod Type/Film
(a per-rod radiograph viewer reached by drilling into a specific rod —
matching the live demo's own `/inspection/AIC-B4/rod-07`-style route, not
a flat top-level nav entry, which is why this project's own Ch. 3 nav
reconstruction already only registered two flat routes here, not three —
no inconsistency to fix), and NDT Methods (a static reference table).

**The book's own source material states plainly that none of this is
real, at any level**: *"Volume III has no inspection endpoint, no NDT
results, no rod inventory, and no verdicts... this entire module is
generated."* The radiograph is a synthetic image the console draws, the
indications on it are placed by the console, and the acceptance verdict
is computed from those placed indications — a demonstration, explicitly
labeled as one (a watermark baked into the image's own pixels, "DEMONSTRATION
MODE — NO RECORDS," specifically because the book's own chapter is about
how a qualifying caption does *not* survive a screenshot the way a
watermark burned into the pixels does).

**This project's real backend is actually ahead of the book's own source
material here** — checked the real DTO before assuming otherwise:
`UnitAssetConditionDto` (`AssetCode`, `Name`, `Category`, `Status`,
`IsSafetyRelated`, plus a nullable latest condition assessment —
grade/health-score/remaining-useful-life) is a real, generic
asset/condition model, not empty. But its own doc comment is explicit:
*"NDT Methods and Rod Type/Film have nothing real to map to at all — not
missing fields on this DTO, but concepts absent from the schema
entirely."* So the situation here is stronger than the Reactor cluster's
own consolidation case: there isn't just one generic endpoint standing in
for several screen names — two of the three screens (Rod Type/Film's
radiograph/verdict, and Inspection Overview's per-rod NDT results if it
tried to show them) have **zero** real backing of any kind, and building
a convincing fake image or a computed-from-nothing verdict would be
exactly the fabrication Ch. 16's own "marker that must survive the
screenshot" chapter is a sustained argument against.

**Recommendation applied**: build the real, generic asset/condition list
honestly (Inspection Overview), explicitly do not build any radiograph,
indication, or acceptance-verdict content anywhere (Rod Type/Film), and
give NDT Methods its own real, separate, **static** screen — because the
book itself treats that table as genuinely authored reference content
("it needs no provenance marker beyond an author and a review date,
because a reader cannot mistake a methods table for a measurement"), the
same class of content as Model Analysis's own model-constants panel or
Training Mode's `SCORING` table, not something to fold into the live
data view just because both routes happen to share one endpoint. This is
a three-way split (one live-data screen, one deliberately-unbuilt gap,
one static-reference screen) rather than the Reactor cluster's simpler
"one live screen for everything" collapse, because the underlying
reality here is a genuine three-way split too: real generic data, zero
real data, and authored reference content are three different kinds of
thing, and collapsing them into one screen would have been less honest
than keeping them apart.

## What was built

- `core/api/maintenance-api.ts` — `UnitAssetCondition` mirrors
  `UnitAssetConditionDto` field-for-field; `MaintenanceApi.getAssetConditions(unitId)`.
- `features/asset-condition/asset-grouping.ts` — pure grouping by the
  real `Category` field (same discipline as the Reactor cluster's
  `groupByCategory`: group by what the data actually reports, never by
  an invented "rod type" or "NDT method" taxonomy).
- `features/asset-condition/asset-condition.ts` (+html/scss/spec) —
  signals-based loading/error/loaded state over the real endpoint; shows
  every asset's category, safety flag, and latest condition
  grade/health-score/remaining-useful-life, or an honest "NO ASSESSMENT"
  pill when none exists yet. No radiograph, no verdict, anywhere.
- `features/ndt-methods/ndt-methods-reference.ts` — six real NDT methods
  (RT/UT/VT/ECT/PT/MT: what each detects, typical use), including the
  same genuine physical note the book's own source file makes: magnetic
  particle testing is N/A for rod cladding because zirconium alloy and
  austenitic stainless steel are both non-ferromagnetic.
- `features/ndt-methods/ndt-methods.ts` (+html/scss/spec) — renders that
  table. No `HttpClient` anywhere in the component or its providers.

## Tests

```
npx jest   → 106/106 passing (was 97; 9 new specs)
```

- `asset-grouping.spec.ts` — real-category grouping, sorted, deterministic.
- `asset-condition.spec.ts` — loading/error/loaded states, honest
  assessed-count math (only assets with a real condition grade count).
- `ndt-methods.spec.ts` — creates successfully with **no** `HttpClient`
  provider registered at all (proof of no BFF dependency, same technique
  as Model Analysis's own spec); renders all six methods, not a subset;
  asserts the real non-ferromagnetic constraint text is present, not a
  placeholder.

Production build:

```
npx ng build → 0 errors, 0 warnings. asset-condition and ndt-methods each
               compile to their own small lazy chunk (~2.1 KB / ~1.5 KB
               transfer).
```

## Live evidence — real host, real database, real screenshots

Memory checked before starting both processes (2.54 GB, healthy).
`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Maintenance`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                          → Healthy, HTTP 200
GET /api/v1/maintenance/units/1/assets     →
  [{"assetCode":"ASSET-UNIT1-001","name":"Primary Coolant Pump","category":"MECHANICAL",
    "status":"IN_SERVICE","isSafetyRelated":true,"latestAssessedAtUtc":"2026-08-22T10:00:00",
    "latestConditionGrade":"GOOD","latestHealthScorePercent":91.00,"latestRemainingUsefulLifeDays":1750},
   {"assetCode":"ASSET-UNIT1-002","name":"Backup Feedwater Valve (no condition yet)",
    "category":"MECHANICAL","status":"IN_SERVICE","isSafetyRelated":false,
    "latestAssessedAtUtc":null,"latestConditionGrade":null,"latestHealthScorePercent":null,
    "latestRemainingUsefulLifeDays":null}]
```

Same two assets seeded during the earlier backend slice
(`2026-08-23-bff-maintenance-rod-inspection-slice.md`) — no reseeding
needed.

`/insp-overview` rendered live (`get_page_text`): `1 / 2` assets assessed,
one `MECHANICAL` group, `Primary Coolant Pump` showing `SAFETY` /
`GOOD` / `91% · 1750d RUL`, `Backup Feedwater Valve` showing
`NO ASSESSMENT` — matching the real endpoint response exactly, no
fabricated fields.

`/ndt-methods` rendered live with all six methods and the real
non-ferromagnetic note, confirming the static reference content displays
correctly and (per its own spec) makes no backend call.

### Screenshots

- `inspection-overview.png` — the consolidation note, `1 / 2` summary,
  the real `MECHANICAL` group with both assets.
- `ndt-methods.png` — all six methods, full reference detail, the
  magnetic-particle note.

Both reviewed directly: full-width shell (no regression), clean
single-column layouts, no cramped columns, no dead space.

Login/session verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x2)
```

Two sessions, matching the two composed contexts. Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases `ONLINE`
afterward.

## Summary

Investigated before building: the book's own Rod Inspection module has
zero real data behind any of its three screens (a fully-generated
radiograph/verdict, by the source material's own admission), while this
project's real Maintenance context — checked directly — has a genuine
but entirely generic asset/condition model with nothing rod-specific,
NDT-specific, or radiograph-specific in it anywhere. Built the one real
thing honestly (asset/condition data, grouped by real category, no
fabricated per-rod detail), explicitly declined to build any radiograph
or verdict content (the exact kind of convincing fabrication Ch. 16's own
"marker that must survive the screenshot" argument warns against), and
kept NDT Methods as its own genuinely static reference screen rather than
folding it into the live view, since authored reference content and live
asset data are different kinds of thing even when adjacent in the book's
own chapter.
