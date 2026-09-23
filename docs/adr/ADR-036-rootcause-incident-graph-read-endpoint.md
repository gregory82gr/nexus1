# ADR-036: RootCause incident-graph read endpoint

## Status

Accepted. Sibling to ADR-034 (the diagnosis query route).

## Context

The Root Cause screen (Ch.29, commit 66e1491) showed a "GRAPH VISUALISATION —
PENDING A BACKEND ENDPOINT" note instead of drawing the EVT-2026-0418 fault-tree
(Figure 29.1), because the diagnosis route returns the ranked verdict + citations,
not the graph topology (nodes/edges/delay-windows/layer-kinds). The backend already
holds this data — the seeded `Component`/`Edge` tables (ADR-032, ADR-034) — it was
just not exposed. This closes that named, visible gap: a read-only endpoint that
serves the topology so the screen can draw it.

## Decision

**Add a read-only `GET /api/v1/root-cause/incidents/{incidentId}/graph` on
RootCause.Host** returning the seeded topology, proxied thinly by the BFF, and draw
it on the Root Cause screen by reusing the console's existing inline-SVG graph
renderer. Read-only visualisation of existing seeded topology — no diagnosis-route
change, no second incident, no graph editing.

### Route

GET (not POST): it only reads seeded rows — no run created, safe and idempotent,
unlike the diagnosis POST. `EVT-2026-0418` only; any other id → **404** (same
discipline as the diagnosis route). Implemented as a CQRS query
(`GetIncidentGraphQuery` / `GetIncidentGraphQueryHandler`, `Result<T>`, hand-rolled
dispatch, no MediatR — mirrors `GetAnalysisByIdQueryHandler`) over an
`IIncidentGraphReader` port (`EfIncidentGraphReader` in Infrastructure). It runs
**no engine logic** — no walk, no coverage, no verdict; just the raw topology.
Response: `nodes[{componentId, tag, kind, status, alarmCount, illustrativeRole,
illustrativeWeight}]` and `edges[{fromComponentId, toComponentId, kind,
delayMinSeconds, delayMaxSeconds, sourceRef}]` (edges join to nodes by componentId).

### Connection — nexus1_app (read/write context), reasoned

This reads through the **default read/write context (nexus1_app)** — the same
connection `EfGraphWalker` already reads `Component`/`Edge` through. Reasoning:
- It is the *same tables, same engine-side data* the engine already reads via
  nexus1_app; using the same connection is consistent and needs no new wiring.
- H7's read-only `nexus1_explain` exists specifically to bound the **served model
  and its retrieval** to read-only (ADR-033/ADR-034); this endpoint is neither the
  model nor retrieval. The read-only boundary is about the model, not "every read."
- The endpoint issues only SELECTs; it never writes.
- (Considered and not chosen: `nexus1_explain`, which also has SELECT on the schema,
  for strict least-privilege — but it would need the keyed-readonly-context wiring
  and would be inconsistent with how the engine reads these very tables. Revisit only
  if the project adopts a blanket "every read endpoint uses a read-only credential"
  rule.)

### BFF proxy

A second thin pass-through, identical to ADR-035's diagnosis proxy but GET: reuses
the `"RootCauseHost"` typed HttpClient, relays status + body + content-type verbatim
(200 / relayed 404), returns **502** if the Host is unreachable. Not a new pattern —
one small, consistent addition.

### Frontend rendering — reuse, don't add a library

The Root Cause screen draws the graph by reusing the **Ch.27 System Dependencies**
inline-SVG approach (a shared layout module + `<svg>` with `<line>` edges, arrowhead
`<marker>`, and node `<g>` groups) — no new charting/graph library. Its own fast GET
loads the graph immediately (a pure DB read) while the diagnosis POST (20–100s) runs
separately; the graph appears first, the verdict fills in after.

**Topology vs layout — the honesty line:** the graph's *structure* (which nodes,
which edges, their kinds and delay windows) is 100% backend data, drawn purely from
the fetched response — the feature hard-codes no node/edge/delay data. The only
client-side constants are the **on-screen positions** (`root-cause-graph-layout.ts`),
which are presentation, not data, exactly as the dependencies feature treats its own
layout. Edge kinds follow Figure 2.1's legend (backbone solid, learned dashed-violet,
artefact dashed-orange — supported though unused in this incident, for legend
completeness, rejected struck-grey); delay windows label each walkable edge. The
**origin** is not a stored topology attribute (the seed leaves FV-104's role null —
origin is *computed* by the diagnosis walk), so the screen marks the origin node by
cross-referencing the real verdict once it lands, never by faking a role on the graph
data.

## Consequences

- The "pending" note is replaced by the real drawn fault-tree; the ranked table,
  citations, and audit hash are unchanged (the graph augments, not replaces, the
  verdict).
- The graph endpoint has no external dependency (pure DB read) — no 503/model
  concerns; only 200/404, and the BFF's 502-on-unreachable.

## Scope boundary

Does NOT touch the diagnosis route; does NOT add a second incident (404 otherwise);
does NOT add graph editing/authoring; no external graph library; positions are
presentation-only, topology is backend-owned.

## Rejected alternatives

- **Extending the diagnosis route to also return topology** — rejected; keeps a slow
  (20–100s) route slow and conflates the fast topology read with the pipeline run.
  A separate fast GET lets the graph draw immediately.
- **nexus1_explain for the read** — considered (least privilege); not chosen (see
  Connection).
- **A graph library (dagre/d3-force/etc.)** — rejected; the console already draws
  node-link graphs with inline SVG (Ch.27), reused here.
- **Auto-layout** — rejected for a fixed 10-node incident; a tag-keyed position map
  matches the dependencies convention and is simplest.

## Reversal condition

Revisit if a second incident lands (positions/layout generalise, or move to
auto-layout), if the topology needs delay-window ranges beyond point delays, or if a
blanket read-only-credential rule is adopted (then move the read to nexus1_explain).

## Evidence required

- API == DB: the BFF graph route JSON (10 nodes, 9 edges: 7 backbone + 1 learned + 1
  rejected) matches direct SELECTs against `RootCause.Component`/`RootCause.Edge`.
- Rendered == API: live render of the Root Cause screen; SVG node/edge counts and
  specific edges (kind + delay label) cross-checked against the API/DB.
- No hidden fixture: grep — no hardcoded node/edge/delay data in the feature (only the
  position layout, presentation-only).
- Backend component test (LocalDB): the graph query returns the seeded 10 nodes / 9
  edges with correct kinds/delays.
- Frontend component tests: the graph renders N nodes/edges from a flushed API
  response, applies the right edge-kind classes, and shows delay labels.
- Gates: solution build 0/0, ArchitectureTests unchanged, RootCause tiers green, full
  Jest suite green.

See `artifacts/evidence/2026-09-23-rootcause-incident-graph-endpoint.md`.
