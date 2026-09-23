# Evidence — RootCause incident-graph read endpoint (Figure 29.1)

**Date:** 2026-09-23
**ADR:** ADR-036 (sibling to ADR-034)
**Scope:** A read-only graph-topology endpoint + BFF proxy + the Root Cause screen
drawing the real EVT-2026-0418 fault-tree, replacing the "GRAPH VISUALISATION —
PENDING" note. Read-only visualisation of existing seeded topology.

## Built

| Piece | Location |
|---|---|
| `GetIncidentGraphQuery` + DTOs + `IIncidentGraphReader` port | Application/Diagnosis |
| `GetIncidentGraphQueryHandler` (CQRS, Result<T>, hand-rolled) | Application/Diagnosis |
| `EfIncidentGraphReader` (reads Component/Edge via nexus1_app) | Infrastructure/Diagnosis |
| `GET /api/v1/root-cause/incidents/{id}/graph` on RootCause.Host | Host/Program.cs |
| BFF proxy `GET …/graph` (thin pass-through, 502 on unreachable) | Bff/Program.cs |
| `RootCauseGraphApi` (typed BFF client) | console core/api |
| `root-cause-graph-layout.ts` (positions only — presentation) | console features/root-cause |
| Root Cause screen draws the SVG graph (reuses Ch.27 pattern) | root-cause.{ts,html,scss} |
| ADR-036 | docs/adr |

## Gates

- `dotnet build Nexus1.Runtime.sln` → **0 warnings / 0 errors**.
- `Nexus1.ArchitectureTests` → **8/8** (unchanged).
- `Nexus1.RootCause.UnitTests` → **35/35**.
- `Nexus1.RootCause.ComponentTests` → **56/56** (53 prior + 3 new `IncidentGraphReaderTests`,
  LocalDB + live Ollama): the reader returns the seeded **10 nodes / 9 edges (7 backbone
  + 1 learned + 1 rejected)** with correct kinds/delays; the handler 404s on an unknown
  incident.
- `npx jest` (console) → **283 passed / 55 suites / 0 failed** — includes the new graph
  render tests (node/edge counts, edge-kind classes, delay labels, origin highlight);
  no regression.
- Clean `ng build` (no NG warnings for the new SVG template).

## API == DB (evidence A)

`GET` the BFF graph route and compare to direct SELECTs:

BFF JSON: `nodes: 10 | edges: 9 | kinds {backbone:7, learned:1, rejected:1}`, full edge list:
```
FV-104 -> SG-1    backbone 36-36s      LF-1  -> CT-1  backbone 30-30s
SG-1   -> FWP-2A  backbone 18-18s      CT-1  -> RT-1  backbone 4-4s
FWP-2A -> PB-2A   backbone 42-42s      RCP-1B-> LF-1  backbone 10-10s   (parallel strand)
PB-2A  -> LF-1    backbone 12-12s      BUS-2A-> FWP-2A learned  2-2s     (data-informed)
                                       FT-7  -> LF-1  rejected 0-0s     (ruled-out, kept)
```
DB direct: `db_components=10 db_edges=9`; edge kinds `backbone n=7, learned n=1,
rejected n=1`; `FV-104->SG-1 kind=backbone delay=36`. **JSON == DB, exactly.**

## Rendered == API (evidence B)

Live Playwright render of `/rcgraph` against the full stack (console → BFF → Host → DB;
Ollama up for the verdict). DOM counts:
```
{ "nodes": 10, "edges": 9, "backbone": 7, "learned": 1, "rejected": 1,
  "delayLabels": 8, "originTag": "FV-104", "ruledOutTag": "FT-7", "verdict": "FV-104" }
```
- 10 node groups + 9 edge lines with the right per-kind `data-kind` == the API/DB.
- 8 delay labels (7 backbone + 1 learned; the rejected edge carries no window).
- The origin (FV-104) is highlighted by **cross-referencing the real verdict** (not a
  faked topology attribute); FT-7 shows its seeded ruled-out role.
- Screenshot: `artifacts/evidence/screenshots/root-cause-graph.png` — the fault-tree
  drawn left-to-right FV-104 → … → reactor trip, with the parallel/learned/rejected
  strands and the Figure 2.1 legend.

## No hidden fixture (evidence C)

Grep of the feature (root-cause.ts / .service.ts / graph-layout.ts / graph-api.ts)
for hardcoded topology (`backbone|learned|rejected|artefact|delayMin|fromComponentId|
edges:[…]`) returned only field references, the delay-label formatter (computing from
parameters), and TypeScript interface declarations — **no hardcoded nodes/edges/delays.**
The only client-side graph constant is `NODE_POSITIONS` (x/y positions, presentation).
The topology is 100% the fetched API response.

## Connection (nexus1_app), reasoned

The reader reads through the default read/write context (nexus1_app) — the same
connection `EfGraphWalker` already reads Component/Edge through. H7's read-only
nexus1_explain bounds the served model + retrieval, not "every read"; this endpoint is
neither, reads the same engine-side tables, and only SELECTs. (Least-privilege via
nexus1_explain was considered; see ADR-036.)

## Scope boundary

Does NOT touch the diagnosis route; does NOT add a second incident (404 otherwise);
does NOT add graph editing/authoring; no external graph library; positions are
presentation-only, topology is backend-owned.
