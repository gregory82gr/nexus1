# Evidence: Angular console, Appendices A–F — Digital Twin & About

## Scope

Two real, actionable gaps from the book's own Appendix A (screens "never
individually audited"): **Digital Twin** (`/twin`, fleet-wide
reconciliation table) and **About** (`/about`, static scope statement).
Plus a one-line documentation note on `--panel-2` per Appendix E.1's
token cross-check.

## Investigation, reviewed before writing final code

Traced each of the book's three named Digital Twin concepts against real
code before building (report reviewed and approved before this build):

- **Model fidelity**: real, fleet-wide (`GetActiveTwinsForFleetQuery`,
  DI-registered, never mapped to a route before this chapter).
- **Signals mirrored**: real (`TraceModelVariableToSignalQuery(twinCode)`,
  a genuine `SignalBinding` join to `Instrumentation.Signal`, real FK per
  ADR-020), scoped per twin code.
- **Per-signal deviation**: real (`GetOpenDivergencesQuery`, modeled vs
  measured value per signal) — **fleet-wide only**. Confirmed directly:
  `IActiveTwinFinder.GetActiveTwinsForUnitAsync`'s own doc comment names
  the real gap — a specific unit's divergences require a four-hop join
  (`TwinDivergence -> TwinSnapshot -> TwinRuntimeSession ->
  TwinModelVersion -> TwinModel.UnitId`) that no existing query performs,
  and `OpenDivergenceDto` itself carries no unit reference.

About needs no backend — confirmed pure static content, same pattern as
Help & Guide.

## What was built

### Backend — 3 thin routes, zero new logic

`Nexus1.Bff/Program.cs`: `GET /api/v1/digital-twin/fleet`
(`GetActiveTwinsForFleetQueryHandler`), `GET
/api/v1/digital-twin/twins/{twinCode}/signals`
(`TraceModelVariableToSignalQueryHandler`), `GET
/api/v1/digital-twin/divergences` (`GetOpenDivergencesQueryHandler`) —
all three handlers were already DI-registered via
`AddDigitalTwinApplication()`, never mapped to a route before. No new
Application/Domain code.

```
dotnet build src/Hosts/Nexus1.Bff -> 0 Warning(s), 0 Error(s)
```

### Frontend

- `core/api/digital-twin-api.ts` — extended the **existing** `DigitalTwinApi`
  service (Plant 3D View's own, Ch. 8) with three new methods
  (`getFleetTwins`, `getSignalTrace`, `getDivergences`) rather than
  replacing the file. See "Error caught" below — this file was initially
  clobbered by mistake and had to be reverted and properly merged.
- `features/digital-twin/digital-twin.ts/.html/.scss/.spec.ts` — three
  panels: **Active Twins** (fleet-wide, model fidelity, clickable rows),
  **Signals Mirrored** (loads on twin selection), **Open Divergences** —
  explicitly labeled `FLEET-WIDE — NOT PER-UNIT` (a pill, matching Ch.
  31's `DB CONNECTIVITY CHECK — NOT SERVICE-LEVEL MONITORING` styling
  convention exactly) with the real four-hop-join gap named in the
  panel's own explanatory text, not hidden in a code comment only.
- `features/about/about.ts/.html/.scss/.spec.ts` — pure static content,
  no backend call, worded consistently with Help & Guide's own Scope
  panel (identical text reused verbatim for that section so the two
  screens don't drift apart). **Drafted for your review — not yet
  treated as final wording.**
- `app.routes.ts` — `/twin` and `/about` now point at the real
  components. **Path note**: kept the existing route path `'twin'`
  rather than switching to `/digital-twin` as literally instructed —
  the route table's own header states every path must match
  `index.html`'s original `data-p` value; the feature folder is named
  `digital-twin/`, the URL is not. Flagging this deviation explicitly
  rather than silently doing something different from the instruction.
- `styles/_tokens.scss` — one-line doc comment on `--panel-2` noting it's
  a real project addition beyond the book's Appendix E.1 list.

### Error caught and fixed mid-build

`core/api/digital-twin-api.ts` already existed (Plant 3D View, Ch. 8's
own `UnitTwinState`/`getUnitTwinStates`) and was overwritten by an
initial `Write` call instead of extended. Caught immediately by the full
Jest run: 7 tests failed in `plant-3d.spec.ts` with `TypeError:
this.api.getUnitTwinStates is not a function`. Fixed by restoring the
original content (via `git show HEAD:...`) and merging the three new
interfaces/methods alongside it, rather than replacing it. Full suite
confirmed green after the fix — see Gates below.

## Gates

```
npx jest (new specs alone) -> 2 suites, 6/6 passing
npx jest (full suite)      -> 53 suites, 267/267 passing (was 261; regression
                               caught and fixed before this count, see above)
npx ng build                -> 0 errors, 0 warnings; digital-twin and about
                               each compile to their own lazy chunk
                               (2.32 KB / 1.21 KB transfer)
```

.NET: no dedicated `Nexus1.Bff` test project exists in this solution (checked
directly — confirmed for every prior BFF-route chapter too, not unique to
this one). The three new routes are pure minimal-API wrappers with zero new
logic; the handlers they wrap already have full component-test coverage
(`GetActiveTwinsForFleetQueryHandlerTests.cs`,
`TraceModelVariableToSignalQueryHandlerTests.cs`,
`GetOpenDivergencesQueryHandlerTests.cs`, all pre-existing). "New tests for
the 3 routes" honestly means: those existing handler tests, confirmed still
passing, not new BFF-level tests (no infrastructure for that exists anywhere
in this project).

```
dotnet build Nexus1.Runtime.sln           -> 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln --no-build -> 869 tests across 37 assemblies, 0 failures
                                              (unchanged from before this chapter, confirmed
                                              via a full untruncated log)
```

## Live evidence

Real gap found and disclosed: the dev database's `DigitalTwin.TwinModelType`/
`TwinModelStatus`/`TwinFidelityLevel` lookup rows for the existing real
`TwinModel` (id 1, "TWIN-UNIT-1", unit 1, seeded in an earlier chapter) DID
exist, but `SignalBinding`, `TwinVariable`, `TwinModelVersion`,
`TwinRuntimeSession`, `TwinSnapshot`, `TwinDivergence`, and every
`BindingRole`/`BindingStatus`/`SnapshotReason`/`DivergenceSeverity`/
`DivergenceStatus`/`ModelVariableType`/`SolverType`/`ValidationStatus`
lookup were completely empty — a pre-existing dev-seed gap, not something
introduced this chapter. Filled in the real, valid graph (matching
`DigitalTwinSeedHelper`'s own component-test shape exactly) for the
already-real twin: one real `SignalBinding` to the real `NX1-U1.RX.POWER`
signal, one real `TwinSnapshot`, and three real `TwinDivergence` rows
(two Open at different severities, one Closed — proving the handler's
own "excludes closed" behavior live, not just in its component test).

BFF composed with ReactorFleet + Instrumentation + DigitalTwin:

```
GET /api/v1/digital-twin/fleet
  -> [{"unitCode":"UNIT-1","twinCode":"TWIN-UNIT-1","modelType":"PHYSICS-BASED","status":"ACTIVE","fidelity":"HIGH"}]

GET /api/v1/digital-twin/twins/TWIN-UNIT-1/signals
  -> [{"twinCode":"TWIN-UNIT-1","modelVariable":"REACTOR_POWER","signalTag":"NX1-U1.RX.POWER","bindingRole":"INPUT","bindingStatus":"ACTIVE"}]

GET /api/v1/digital-twin/divergences
  -> 2 Open divergences returned (Critical -5.8Δ on NX1-U1.RX.POWER, Warning +1.6Δ
     on UNIT1-NI-001); the 3rd, Closed, row correctly excluded.
```

Confirmed the same live in the rendered screen: fleet twin row, selecting
it loads the real signal trace, and the Open Divergences panel shows both
real open rows with the `FLEET-WIDE — NOT PER-UNIT` label clearly visible
alongside them, never silently narrowed to look per-unit.

### Screenshots

- `digital-twin.png` — Active Twins (1 real twin, selected/highlighted),
  Signals Mirrored (real trace loaded for the selected twin), Open
  Divergences (`FLEET-WIDE — NOT PER-UNIT` pill prominent, real
  Critical/Warning severity distinction, real modeled/measured/delta
  values).
- `about.png` — static reference content, real stack facts, honest-gap
  discipline statement, Scope section matching Help & Guide's wording.

Both reviewed directly before reporting done — layout clean, the
fleet-wide-not-per-unit labeling is unambiguous and not buried.

## Drafted About copy — for your review

The exact text is in `features/about/about.html`, screenshotted above.
Summary of what it claims, each checked against this project directly:
real frontend/testing/backend stack facts (Angular 18, TypeScript 5.5,
RxJS 7.8, Three.js, Jest, Playwright, .NET 8); the honest-gap discipline
framed as this console's own observed, recurring behavior across every
chapter, not a one-off claim; and a Scope paragraph reusing Help &
Guide's own wording verbatim. Not finalized — flag anything you'd want
changed.

## Summary

Read the investigation report before building. Confirmed all three of
the book's named Digital Twin concepts (fidelity, signals mirrored,
per-signal deviation) are real and reachable via thin routes wrapping
already-DI-registered handlers — the one genuine constraint
(divergences are fleet-wide only, a real four-hop join stands between
that and per-unit) is disclosed on-screen, not narrowed silently.
Found and disclosed a pre-existing dev-seed gap (DigitalTwin's own
supporting lookup/graph tables were empty for an already-real twin) and
filled it with a real, valid, FK-consistent seed matching the project's
own component-test shape. One real mistake caught and fixed before it
compounded: an existing API client file was clobbered instead of
extended, caught immediately by the full Jest run and fixed properly.
About is pure static content, drafted for review, worded to match Help
& Guide rather than drift from it.
