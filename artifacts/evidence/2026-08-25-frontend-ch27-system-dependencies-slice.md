# Evidence: Angular console, Ch. 27 — System Dependencies

## Scope

One screen, `deps` route, three tabs over one shared dataset, no new
BFF route (the real node data reuses the existing Instrumentation
signals endpoint already proven three times this arc):

1. `DependenciesComponent` (`features/dependencies/`) — Graph (default),
   Matrix, Causal Chain tabs, one shared disclosure banner at the
   container level.
2. `dependency-graph.ts` — pure data module: 12 typed nodes, 14 typed
   edges, shared by all three tabs.
3. One real signal seeded (`POWER` category, unit 1) to complete the
   3-real-node set this cluster's own investigation promised.

Read the full chapter (pp. 277–284 of `From_File_to_Framework_Final.pdf`,
extracted via `pdftotext -f 273 -l 288 -layout`) before building.

## Investigation, reported and approved before writing final code

The book's finding: Matrix and Chain both disclose their coefficients as
"illustrative, not identified from plant data"; the Graph tab — active
by default, the one a reader sees first — discloses nothing, despite
using the same kind of hand-authored constant (edge weight + delay).
Two categories of content, per the chapter's own distinction:

- **Edges (topology + coefficient)**: the book explicitly authorizes the
  causal topology itself (rods→reactivity→flux→power, plus feedback
  loops) as a reasonable, simplified physical model — the same category
  already ported for the point-kinetics model (Ch. 11). No real
  coupling-coefficient data is required or claimed. Kept as-is.
- **Nodes (status/colour)**: must be real, live-derived data, same
  standard as every other screen. Checked directly, node by node,
  against the real Instrumentation/ReactorFleet domain before writing
  any node list:

| Node | Real backing? | Source |
|---|---|---|
| Neutron Flux | **Real** | `NEUTRONICS` category signal |
| Thermal Power | **Real** | `POWER` category signal |
| Turbine | **Real** | `TURBINE` category signal (Power & Grid cluster) |
| Control Rods | Gap | No rod-position entity anywhere (confirmed again) |
| Reactivity | Gap | `EngineeringQuantityType.Reactivity` unused, same pattern as Power & Grid's own `Frequency` finding |
| Coolant A / B | Gap | No coolant-temperature signal ever seeded |
| Xenon-135 | Gap | No xenon concept anywhere |
| Fuel Temp | Gap | No fuel-temperature signal anywhere |
| Feedwater | Gap | Only exists as a Maintenance asset name (equipment health, already shown on Rod Inspection Ch. 16) and generic incident-title text — neither is a live level reading |
| Steam Gen | Gap | No steam-generator signal anywhere |
| Grid | Gap | Frequency/phase/breaker/sync already confirmed `NO SOURCE` (Power & Grid cluster) |

3 of 12 real, 9 of 12 total absence — reported to the user before
building; approved as-is, with one explicit added constraint: the 9 gap
nodes must be visually unambiguous from a real-but-low node, not merely
a muted shade of the same palette.

## Edges and the structural banner fix

`dependency-graph.ts` defines the book's 14-edge topology (12 forward
edges + 2 negative-feedback edges into Reactivity — xenon and
fuel-temperature/Doppler, dashed violet on the Graph tab), every edge
typed `kind: 'illustrative-topology'` **directly on the data**, not just
a markup note — asserted by a spec (`dependency-graph.spec.ts`), not
trusted to a template element staying present.

The disclosure banner is rendered exactly once, in `dependencies.html`,
above the `@switch (tab())` block — never inside any of the three
`@case` branches, never duplicated per tab. This is the structural fix
the chapter itself asks for: switching tabs can change which panel
renders; it cannot make the banner disappear, because the banner was
never inside any tab's own template to begin with. Verified live by
clicking through all three tabs and confirming the banner's exact text
persists unchanged (see Live evidence below), and by a Jest test that
loops `setTab('graph' | 'matrix' | 'chain')` and asserts the banner
element is present after each switch.

## The visual-distinctness requirement — confirmed explicitly

Per the reviewer's explicit added constraint, the 9 no-source nodes use
three independent, redundant distinguishing signals from the 3 real
nodes — never just a muted shade of the same palette:

1. **Border style**: real nodes get a solid `1px solid var(--green)`
   border; gap nodes get a `1px dashed var(--line)` border.
2. **Background pattern**: gap nodes get a diagonal hatch
   (`repeating-linear-gradient(45deg, ...)`), a genuinely different
   visual texture, not just a darker/lighter fill of the same flat
   colour real nodes use.
3. **Text badge**: real nodes show `LIVE` (green `pill.ok`) plus their
   actual numeric value; gap nodes show `NO SOURCE` (muted
   `pill.nosource`) and **never render a `.node-value` element at all**
   — confirmed by a Jest test that queries for `.node-value` on a gap
   node and asserts it is `null`, not just visually absent.

All three signals are visible together in the Graph-tab screenshot
below: Neutron Flux/Thermal Power/Turbine are solid-green-bordered flat
cards reading `LIVE` with a real number; every other node is a
dashed-bordered, diagonally-hatched card reading `NO SOURCE` with no
number at all. A reader cannot mistake one for the other from the
screenshot alone.

## Real signal completing the 3-node set

Checked live before capturing evidence: `POWER`-category data had never
actually been seeded in this dev database (only present in a *test*
seed helper, not the live LocalDB this arc's evidence sessions all use)
— `NEUTRONICS` and `TURBINE` were live, `POWER` was not. Seeded one real
`POWER` signal for unit 1 (`NX1-U1.RX.POWER`, `100.1`), matching the
already-approved "3 real nodes" plan — extending the existing generic
Instrumentation model with one more real row, same mechanism as every
prior signal-seeding session this arc (`NEUTRONICS`, `TURBINE`).

## No new BFF route needed

`GET /api/v1/instrumentation/units/{id:int}/signals` (Program.cs,
unchanged) already existed and was already reused by the Reactor
cluster and Power & Grid — zero backend code changes this cluster
beyond the one new signal row above.

.NET build/test: unchanged this slice — no backend code was touched.
```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → all assemblies green, unchanged
```

## Frontend: what was built

- `features/dependencies/dependency-graph.ts` — `NodeId`, `DependencyNode`
  (12 entries, `realSignalCategory: string | null`), `InfluenceEdge` (14
  entries, `kind: 'illustrative-topology'`), plus `matrixCell`/`chainEdges`
  helpers shared by all three tabs.
- `features/dependencies/dependencies.ts` — `DependenciesComponent`:
  tab state, the one shared banner, per-node status derivation
  (`real` / `real-no-reading` / `loading` / `error` / `no-source`) from
  the real signals fetch, never fabricated for a gap node regardless of
  fetch state.
- `features/dependencies/dependencies.html/.scss` — banner (once, above
  the tab switch), tab bar, Graph (node grid + edge list), Matrix (real
  12×12 table over the same edge data), Chain (one traced forward path,
  labeled "illustrative trip reconstruction" per the book's own phrase).

## Tests

```
npx jest dependencies dependency-graph → 17/17 passing (new specs alone)
npx jest (full suite)                  → 202/202 passing (was 185)
```

- Data module: exactly 14 edges, all typed; a future untyped edge fails
  the check (structural guard, mirrors the book's own test); exactly 12
  nodes; exactly 3 real-backed (`flux`/`power`/`turbine`) and 9 not;
  exactly 2 negative-feedback edges, both into `reactivity`; `matrixCell`
  and `chainEdges` read the same typed dataset correctly.
- Component: banner present after switching to each of the three tabs;
  real node status derived correctly from a mocked signals response; a
  gap node's status is `no-source` even after signals load successfully
  (never upgraded); gap and real nodes render mutually-exclusive CSS
  classes (`.gap` vs. `.real`, never both, never neither); a gap node
  never renders a `.node-value` element; Matrix/Chain tabs read the same
  edge dataset the Graph tab uses.

Production build:
```
npx ng build → 0 errors, 0 warnings. dependencies compiles to its own
               lazy chunk (~3.32 KB transfer).
```

Jest, then the Angular build, then the .NET build+test were run
sequentially, not concurrently. Available memory was checked before
starting the live hosts (1.91 GB) and `dotnet build-server shutdown`
was run as a precaution, bringing it to 2.5 GB.

## Live evidence — real host, real database, real screenshots (all 3 tabs)

`Nexus1.Bff` started subset-composed to
`BffContexts__Enabled__0=Instrumentation`, `__1=ReactorFleet`; `ng serve
--port 4200` alongside it.

```
GET /health/ready                                → Healthy, HTTP 200
GET /api/v1/instrumentation/units/1/signals      →
  POWER (100.1), NEUTRONICS (93.5, plus one no-reading channel), TURBINE (3001.1)
```

`/deps` rendered live on all three tabs (`get_page_text`, real API call
confirmed `200 OK` via the network log): Graph tab shows all 3 real
nodes with `LIVE` + real values and all 9 gap nodes with `NO SOURCE`;
Matrix tab shows the same 14 edge weights in a real 12×12 table, with
the two feedback cells visually marked; Chain tab shows the 7-step
forward reconstruction with the feedback-edges note. **The banner's
exact text was confirmed identical across all three tab switches**,
verifying the structural fix live, not just in a unit test.

### Screenshots

- `dependencies-graph.png` — the default Graph tab: banner, node grid
  (3 solid-green `LIVE` cards with real values; 9 dashed/hatched
  `NO SOURCE` cards with no value), full 14-row edge list with feedback
  edges marked.
- `dependencies-matrix.png` — Matrix tab: banner unchanged, real 12×12
  table.
- `dependencies-chain.png` — Chain tab: banner unchanged, 7-step forward
  reconstruction, feedback-edges note.

Reviewed all three directly before reporting done.

Session/database verification:

```sql
SELECT login_name, program_name, status FROM sys.dm_exec_sessions WHERE login_name = 'nexus1_app';
```
```
login_name  program_name                           status
nexus1_app  Core Microsoft SqlClient Data Provider  sleeping   (x1)
```

One session, matching the two composed contexts sharing one connection
pool (same pattern observed in the Optimization slice). Both processes
stopped cleanly after capture; `sys.databases` confirmed all 9 databases
`ONLINE` afterward.

## Post-review fix: the Graph tab now draws an actual graph

Reviewer feedback on the first round of screenshots: the Graph tab
paired a card grid with a separate flat text list of edges below it —
correct data, handled honestly, but not a graph. The book's own Figure
27.1 describes a real node-link diagram with visual arrows ("Solid =
forward influence, width ∝ illustrative weight. Dashed violet =
negative feedback"). A tab named "Graph" that doesn't draw one is
misleading in a different way than the chapter's own disclosure
finding — a visual-fidelity problem, not a fabrication problem, but
worth fixing before commit.

Fixed by adding a real SVG line layer to the Graph tab:

- `dependency-graph.ts` gained `NODE_POSITIONS` (a logical
  coordinate for each of the 12 nodes, roughly following the forward
  chain left-to-right with xenon/fuel-temp raised above so their
  feedback lines read as loops) and `GRAPH_VIEWBOX_WIDTH/HEIGHT`.
- `dependencies.ts` gained `edgeLines` (each of the 14 edges resolved to
  real `x1,y1,x2,y2` line coordinates plus a `strokeWidth` proportional
  to `|weight|`) and `nodeStyles` (each node's card position as a
  percentage of the same logical space, so lines and cards share one
  coordinate system and never drift apart regardless of container size).
- `dependencies.html`'s Graph tab now renders an SVG (`<line>` per edge,
  solid grey for forward influence, dashed violet with a distinct
  arrowhead marker for the 2 negative-feedback edges) with the 12 node
  cards absolutely positioned on top of it. The numeric edge list is
  kept below the diagram as a supplementary precise reading of the same
  14 edges — not a substitute for drawing them, which is what it was
  standing in for before.
- Matrix and Chain tabs were left exactly as approved (a matrix and an
  ordered list are the correct shapes for those, per the book) — this
  fix is scoped to the Graph tab specifically.

Added 4 new Jest tests (17 → 21 total for this feature): exactly one
`<svg><line>` per edge (14), exactly 2 lines carry the `feedback-line`
class, every node position falls inside the declared viewBox, and every
node card gets a real inline `left`/`top` style rather than default
document flow. Full suite: 202 → 206.

One polish fix caught during live re-verification: the rightmost nodes
(Turbine, Grid) clipped slightly at the panel's edge with the original
`GRAPH_VIEWBOX_WIDTH = 980` (node position 940 left too little margin).
Widened to `1040` — a pure layout constant, no logic change — and
re-verified both Jest (21/21 still green) and the live screenshot before
finalizing.

Re-ran the full gate sequence after this change: Jest 206/206, Angular
build 0 errors/warnings (`dependencies` chunk grew from ~3.3 KB to ~4.2
KB, consistent with the added SVG/positioning logic), .NET build 0/0 +
full suite unchanged (still no backend code touched). Re-verified live:
the real signals endpoint still drives the 3 real nodes correctly, and
all three tabs were re-clicked live confirming the banner text is
byte-identical across all three — the structural fix from the first
round is untouched by this visual one.

The `dependencies-graph.png` screenshot above (regenerated) now shows
the actual diagram: solid grey lines of varying width connecting the 12
node cards, two dashed violet lines running from Xenon-135 and Fuel Temp
back into Reactivity, matching the book's own Figure 27.1 convention.

## Second post-review fix: node spacing and line-endpoint legibility

A second round of review feedback on the redrawn Graph tab caught two
remaining visual problems, both confirmed against the screenshot, not
disputed:

1. **Turbine and Grid cards overlapped.** The two nodes sat only 100
   layout units apart (840/940) — narrower than a card's own visual
   footprint — so "Grid" rendered inside Turbine's box and its `NO
   SOURCE` badge was unreadable.
2. **The two feedback lines into Reactivity converged on its own text.**
   Both lines ran center-to-center, landing dead on Reactivity's `NO
   SOURCE` label rather than stopping at the card's edge.

Both are the same underlying cause: every line and every card position
had been computed from raw node *centers*, with no accounting for a
card's actual footprint. Fixed properly rather than patched per-pair:

- **Spacing**: `NODE_POSITIONS` revised so every node pair is separated
  by at least 150 layout units on the axis they differ most on —
  comfortably wider than a card's ~90-unit footprint, not just enough to
  avoid a bounding-box collision. `GRAPH_VIEWBOX_WIDTH`/`HEIGHT` grown to
  `1180`/`460` to match.
- **Line endpoints**: added `edgeEndpoints()` (`dependency-graph.ts`),
  which treats each node as an ellipse (`NODE_HALF_WIDTH`/`HEIGHT`) and
  pulls both ends of every edge's line back from the raw center to the
  node's own boundary, in the direction of the other node — not a
  one-off fix for the two feedback edges, applied to all 14 edges
  uniformly, so no line (forward or feedback) ever crosses through any
  node's label or status badge again.

Added 3 more Jest tests (21 → 24 total for this feature): every pairwise
node distance exceeds the card footprint on both axes (a general
regression guard, not just re-checking Turbine/Grid); every edge's
computed endpoints differ from the raw node centers; both feedback
lines into Reactivity land more than 15 units from its center, clear of
where the text sits.

Re-ran the full gate sequence again: Jest 209/209, Angular build
0 errors/warnings, .NET build 0/0 + full suite unchanged. Re-verified
live: real signals still confirmed driving the 3 real nodes, and — this
time via an actual Playwright script rather than manual clicks — the
banner's `textContent` was captured on all three tabs and asserted
identical (`bannersMatch: true`) before the screenshot was taken, a
stronger confirmation than eyeballing the text across manual clicks.

The regenerated `dependencies-graph.png` now shows Turbine and Grid as
two clearly separated cards, and both feedback arrowheads stopping just
short of Reactivity's border with its `NO SOURCE` text fully legible.

## Summary

Investigated all 12 nodes individually against the real domain and
reported the 3-real/9-gap split to the user before writing final code,
per their explicit request. Kept the book's authorized 14-edge topology
unchanged, typed every edge `illustrative-topology` directly on the
data, and hoisted the disclosure banner to the one container all three
tabs share — structurally, not editorially, matching the chapter's own
central argument. Built the 9 gap nodes with three independent, visually
redundant markers (border style, background pattern, badge text) so no
reader could mistake an unbacked node for a real-but-low one, per the
reviewer's explicit added constraint — confirmed here before sending the
screenshots, not asserted after the fact.
