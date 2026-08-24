# Evidence: Angular console, Ch. 8 — Plant 3D View (reshaped around real twin data)

## Scope

`Plant3dComponent` (`features/plant-3d/`), wired to the real, already-proven
`GET /api/v1/digital-twin/units/{id}` endpoint. Replaces the placeholder on
the `/plant3d` route.

## What Ch. 8 actually specifies, and the scope decision made against it

Ch. 8's own Plant 3D View is a **fleet-wide** three.js scene (every unit
rendered together — Unit 1, Unit 2, three SMR modules), driven by exactly
four derived facts: unit count, each unit's reactor class (used to compute
steam-generator count — 3 for a PWR-900, 4 for a PWR-1300, 0/integral for
an SMR), each unit's online/offline state, and its power output. Everything
else in the scene (positions, distances, building shapes) is explicitly
invented — the book calls this "representative geometry" and keeps that
disclaimer permanently on screen, not buried in a footnote. The chapter's
real teaching content, though, is the three.js/Angular ownership boundary:
Angular owns one empty `<div>`, the toolbar, and `ngOnDestroy`; three.js
owns the canvas, scene graph, and render loop; exactly three kinds of
signal are allowed to cross the boundary; and teardown must cancel the
frame loop before disposing geometry/material (in that order), dispose the
renderer last, and force the WebGL context to release — because Angular's
lazy routes genuinely destroy this component (unlike the book's own
`index.html`, which only ever hides pages and never disposes anything).

**None of the four inputs the book's scene needs exist in the real
backend**, checked directly rather than assumed:

- `ReactorFleet.Unit` (`Unit.cs`) is bare `Code`/`Name` (ADR-003) — no
  reactor class, no online/offline flag, anywhere in the domain.
- The real digital-twin endpoint is **per-unit only**. A
  `GetActiveTwinsForFleetQuery` exists in the Application layer, but
  `Program.cs` never maps a fleet-wide HTTP route for it — only
  `GET /api/v1/digital-twin/units/{id}`.
- The per-unit DTO's fields (`ModelType`, `Status`, `Fidelity`) answer a
  different question than the book's scene needs. Checked the actual
  domain classes, not just the DTO: `TwinModelType` classifies a
  *simulation model* (physics surrogate / point-kinetics / thermal-
  hydraulic / equipment-health / visualization) — not a reactor class.
  `TwinModelStatus` is the *twin model's own lifecycle*
  (draft/validating/active/retired/superseded/failed) — not reactor
  online/offline. `TwinFidelityLevel` is a declared trust band
  (illustrative/training/shadow/advisory-ready/validated) — unrelated to
  plant operating state.

This was reported to the user before writing any code (not discovered
after the fact), and the user chose explicitly: **drop the book's physical
plant-layout scene entirely** rather than force it onto data that doesn't
support it (which would have meant inventing a reactor class and an
online flag that exist nowhere in the real domain), and instead build an
honest per-unit 3D visualization of the real twin *binding* — an abstract
object whose color reflects `Status` and whose opacity reflects `Fidelity`,
labeled with `ModelType`/`TwinCode`/`IsAuthoritative`. The three.js/Angular
ownership boundary and its exact teardown discipline are ported faithfully;
only the subject matter of the scene changes.

## A second real-shape correction, caught before going live

Reading the endpoint's own prior evidence
(`2026-08-22-bff-digitaltwin-plant3dview-slice.md`) before wiring the
client — not after — surfaced a second mismatch, unrelated to the scope
question above: `GetUnitTwinStateQueryHandler`'s own signature is
`IQueryHandler<GetUnitTwinStateQuery, IReadOnlyList<UnitTwinStateDto>>` —
**the endpoint returns an array**, because a unit can legitimately have
more than one active, non-deleted twin model (`IsAuthoritative` marks
which one is live). It also **always answers HTTP 200**, even for a unit
with none — `Program.cs`'s route is unconditionally `Results.Ok(result.Value)`
— and that endpoint's own prior evidence explicitly documents an empty
list as "not an error; a unit legitimately having no twin modeled is not
a fault condition."

The first draft of `DigitalTwinApi`/`Plant3dComponent` got both wrong: it
typed the response as a single object and treated a 404 as the "no twin"
signal. Caught and fixed before any live call was made, by reading the
handler and the prior evidence directly rather than assuming the shape
from the DTO's name. Fixed version:

- `DigitalTwinApi.getUnitTwinStates(unitId)` returns `UnitTwinState[]`.
- The component picks `twins.find(t => t.isAuthoritative) ?? twins[0] ?? null`
  — a named, deliberate choice for the never-yet-observed case of multiple
  twins with none flagged authoritative, not an oversight.
- `twin: null` in the loaded state is a genuine, non-error outcome ("no
  twin modeled for this unit"), rendered as its own honest panel, not
  folded into the error state.

## What was built

- `core/api/digital-twin-api.ts` — `UnitTwinState` interface mirroring
  `UnitTwinStateDto` field-for-field; `getUnitTwinStates(unitId)`.
- `features/plant-3d/twin-visual.ts` — pure functions, no three.js import
  (Ch. 8's own discipline: "pure path math (testable without three.js)"):
  - `statusTone(status)`: keyword-matches the real lookup-table text into
    `ok`/`warn`/`crit`/`unknown`. Deliberately conservative — `Status` is a
    free-text lookup-table name in the real schema (`TwinModelStatus.cs`),
    not a closed enum; an unrecognized string renders `unknown`, never
    guessed into a confident tone.
  - `fidelityBandIndex(fidelity)`: matches against the five-band order
    `TwinFidelityLevel.cs`'s own doc comment states (illustrative →
    validated); returns `null` for anything that doesn't match rather than
    guessing a position.
  - `TONE_COLOR` / `fidelityOpacity`: map tone/band to the console's own
    token palette (`--green`/`--amber`/`--red`/`--text-mute`), not
    invented hex values; an unrecognized fidelity renders faint (0.35
    opacity) rather than confidently solid.
- `features/plant-3d/twin-scene.ts` (`TwinScene`) — the non-Angular half.
  One `Group` containing one `Mesh` (icosahedron + `MeshStandardMaterial`),
  ambient + directional light, slow idle spin, and drag-to-rotate (the one
  interaction convention checked against the live demo's own 3D screens —
  "Drag a core to rotate" / "Drag the scene to rotate" — not its physical
  content). `destroy()` follows the book's exact ordering: cancel the
  frame loop first, detach listeners, walk the scene graph disposing every
  geometry and material separately, dispose the renderer last and force
  context loss, remove the canvas.
- `features/plant-3d/plant-3d.ts` (`Plant3dComponent`) — signals-based
  `loading`/`error`/`loaded` state over the real HTTP call;
  `ngAfterViewInit` dynamically imports `three`, constructs `TwinScene`,
  and — inside its own `try`/`catch` — sets `unavailable` on *either*
  failure mode: the chunk failing to load, or `WebGLRenderer` throwing
  because the browser/environment has no WebGL context. The book wraps
  scene creation in `NgZone.runOutsideAngular()`; this app is genuinely
  zoneless (Ch. 2), so there is no zone to escape and the render loop
  can't trigger change detection regardless — omitted as genuinely
  unnecessary, documented as a deliberate omission, not a missed port.
- Route `plant3d` now lazy-loads `Plant3dComponent` instead of the shared
  placeholder.

## Tests

```
npx jest   → 32/32 passing (was 31; +1 net across the new spec files below,
             plus one shared setup-jest.ts addition)
```

- `twin-visual.spec.ts` — pure mapping tests, including "never guesses an
  unrecognized status/fidelity into a known tone/band."
- `twin-scene.spec.ts` — a fake-three.js lifecycle test in Ch. 8's own
  style ("the spec that pays for itself"): counts live geometries/
  materials against a hand-rolled fake `three` module, asserts they reach
  zero after `destroy()`, and asserts `cancelAnimationFrame` was called
  exactly once. Also covers destroying a never-started scene, and
  `setState(null)`/a real state not throwing.
- `plant-3d.spec.ts` — component-level: loading/error/loaded states over
  `HttpTestingController`; tone/fidelity-band derivation from real DTO
  fields; the never-guess-unknown-values case; picking the authoritative
  twin out of a multi-entry array; the empty-array "no twin modeled" case
  as a real, non-error loaded state; and — the one genuinely real (not
  mocked) assertion in the suite — that jsdom's own lack of a WebGL
  context is caught by the component's `try`/`catch` and surfaces as
  `unavailable() === true`, exercising the exact fallback branch a
  restricted-egress or headless environment would hit live, without
  mocking `WebGLRenderer` to force it.
- `setup-jest.ts` gained a minimal `ResizeObserver` stub — jsdom doesn't
  implement it at all, and `TwinScene` needs it to keep the canvas sized
  to its host `<div>`. A test-environment gap fix, not application
  behavior.

Production build:

```
npx ng build → 0 errors. three.js lazy-loads into its own chunk
               ("three-module", ~153 KB transfer), separate from the
               eagerly-loaded initial bundle and from the 10 KB plant-3d
               route chunk itself -- confirms the dynamic import('three')
               is only fetched when the route is visited, the same goal
               the book's own CDN <script> replacement was after.
```

## Live evidence — real host, real database, real screenshot

Memory checked before starting both processes (1919 MB available, stable
across two checks). `Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=DigitalTwin`, `__1=ReactorFleet` (both share
`AlarmManagementDb`, per ADR-020); `ng serve --port 4200` alongside it.

```
GET /health/ready                         → Healthy, HTTP 200
GET /api/v1/digital-twin/units/1          →
  [{"unitId":1,"unitCode":"UNIT-1","twinCode":"TWIN-UNIT-1",
    "twinName":"Demonstrator Twin for Unit 1","modelType":"PHYSICS-BASED",
    "status":"ACTIVE","fidelity":"HIGH","isAuthoritative":true}]
GET /api/v1/digital-twin/units/999        → [] (HTTP 200, no twin -- real,
                                              matches the endpoint's own
                                              prior evidence exactly)
```

The dev database still held the twin row seeded during the backend's own
2026-08-22 BFF slice — no reseeding needed. Its `Fidelity` value, `"HIGH"`
/ `"High Fidelity"`, **does not match any of the five bands documented in
`TwinFidelityLevel.cs`'s own summary comment** ("illustrative, training,
shadow, advisory-ready, validated"). That is a live, unplanned validation
of `fidelityBandIndex`'s conservative design: it correctly returned `null`
for this real value, and the screen rendered "Trust band: UNRECOGNIZED"
rather than guessing a position — proving the defensive design wasn't
overcaution, since the very first real row it met didn't fit the
documented vocabulary.

`GET /api/v1/digital-twin/units/1` rendered on the real page (`get_page_text`):

```
Demonstrator Twin for Unit 1
Twin code       TWIN-UNIT-1
Model type      PHYSICS-BASED
Status          ACTIVE
Fidelity        HIGH
Trust band      UNRECOGNIZED
Authoritative   YES
```

### The geometry-measurement check that turned out to be misleading

Following the standing rule from the Ch. 6 correction, a `getBoundingClientRect()`
sanity check was run before the screenshot, via the Browser pane's own
`javascript_tool` (the same tab whose *screenshot* capability is already
confirmed broken this session). It reported the canvas at `width: 19` px
inside a `646` px stage — apparently a serious bug.

**The real Playwright screenshot (below) shows this was a false alarm**:
the icosahedron renders centered and correctly sized in the actual page.
The most likely explanation is that the Browser pane's own non-compositing
state (already documented as broken for screenshots) also produces
unreliable `ResizeObserver`/layout timing for scripts run against that same
tab — not a bug in the app. This is recorded here rather than quietly
fixed and forgotten, and it reinforces last time's standing rule from the
other direction: a geometry measurement can be *wrong in the confident
direction* too, not only DOM text being blind to layout. Playwright,
driven independently against a real compositing pipeline, is the only
mechanism in this session with real proof it produces trustworthy layout
information — geometry snapshots from the Browser pane tool are now
treated as informative at best, not evidence on their own, until that
tool's compositing issue is otherwise resolved.

### Screenshot

`artifacts/evidence/screenshots/plant3d-ch8-twin-binding.png`, captured
with the same Playwright-direct mechanism established in the Ch. 6
correction (`chromium.launch()`, navigate, `waitForTimeout` for a few
render frames, `page.screenshot()`). Reviewed directly: sidebar full
height, topbar full width, the two-column stage/info layout correctly
filling the `.content` area (no shell-level regression from Ch. 8's own
addition), a green icosahedron rendered at a sensible size and position
(status `ACTIVE` → the console's `--green` token, fidelity `HIGH` →
unrecognized → faint 0.35 opacity, visibly more translucent than a
confidently-validated model would render), the "REPRESENTATIVE VISUAL ·
NOT A SURVEY MODEL" marker in the stage's lower-right corner, and the
named-gaps paragraph (divergence data, physical plant layout) legible in
the info panel.

Login/session verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping
```

Two sessions, matching the two composed contexts (DigitalTwin,
ReactorFleet — both against `AlarmManagementDb`). Both processes stopped
cleanly after capture; `sys.databases` confirmed all 9 databases `ONLINE`
afterward.

## Summary

Ch. 8's own physical plant-layout scene needs four facts (unit count,
reactor class, online state, power output) that don't exist anywhere in
the real domain or the real digital-twin endpoint — checked directly, not
assumed, and reported before building. Per the user's explicit decision,
the screen was reshaped around what the endpoint actually answers: a
per-unit twin binding, visualized honestly with the real `Status`/
`Fidelity`/`IsAuthoritative` fields and a named list of what's absent
(divergence data, the physical plant layout). A second, independent shape
error — the endpoint returning an array, and 200-with-empty-array rather
than 404, for "no twin" — was caught by reading the endpoint's own prior
evidence before writing the client, not discovered live. The three.js/
Angular ownership boundary and its exact teardown discipline are ported
faithfully. Live evidence includes a real screenshot that both confirms
the screen renders correctly and shows that a geometry-measurement
sanity check, taken from the same already-compromised Browser-pane tab,
produced a misleading result — reinforcing screenshot-as-standing-rule
from a new angle.
