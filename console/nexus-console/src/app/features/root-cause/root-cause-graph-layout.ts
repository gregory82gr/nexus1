// Presentation-only layout for the EVT-2026-0418 fault-tree graph (ADR-036),
// mirroring dependency-graph.ts's convention. These are the ONLY client-side
// constants in the Root Cause graph feature: the topology (which nodes, which
// edges, their kinds and delay windows) is 100% backend data; these x/y numbers
// are just where each tag is drawn, in a logical viewBox the template scales to
// the container. Roughly follows the book's Figure 2.1 left-to-right cascade:
// FV-104 (origin) at the left, through the feedwater/thermal path to the reactor
// trip at the right, with the parallel RCP-1B above the loop-flow node and the
// learned 4kV contributor and rejected FT-7 candidate below the main row.
//
// A tag with no position here is simply not drawn (the fixed incident's ten tags
// are all present); nothing else in the feature hard-codes graph data.

export const GRAPH_VIEWBOX_WIDTH = 1120;
export const GRAPH_VIEWBOX_HEIGHT = 440;

export const NODE_POSITIONS: Readonly<Record<string, { x: number; y: number }>> = {
  'FV-104': { x: 70, y: 210 }, // origin
  'SG-1': { x: 235, y: 210 },
  'FWP-2A': { x: 400, y: 210 },
  'PB-2A': { x: 565, y: 210 }, // the loud proximate symptom
  'LF-1': { x: 730, y: 210 },
  'CT-1': { x: 895, y: 210 },
  'RT-1': { x: 1050, y: 210 }, // reactor trip terminus
  'RCP-1B': { x: 730, y: 70 }, // parallel strand into loop flow
  'BUS-2A': { x: 400, y: 370 }, // learned contributor into the pump
  'FT-7': { x: 585, y: 370 }, // rejected candidate into loop flow
};

// A node card's approximate footprint (ellipse) in viewBox units, used to pull
// each edge line's endpoints back to the node boundary instead of its centre --
// same technique as dependency-graph.ts, so no line crosses a node's label.
const NODE_HALF_WIDTH = 46;
const NODE_HALF_HEIGHT = 24;

function pointOnNodeBoundary(cx: number, cy: number, towardX: number, towardY: number): { x: number; y: number } {
  const dx = towardX - cx;
  const dy = towardY - cy;
  const dist = Math.hypot(dx, dy);
  if (dist === 0) {
    return { x: cx, y: cy };
  }
  const ux = dx / dist;
  const uy = dy / dist;
  const t = 1 / Math.sqrt((ux / NODE_HALF_WIDTH) ** 2 + (uy / NODE_HALF_HEIGHT) ** 2);
  return { x: cx + ux * t, y: cy + uy * t };
}

export interface EdgeGeometry {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  labelX: number;
  labelY: number;
}

/** Endpoints (pulled back to each node's boundary) and a midpoint for the delay label, for an edge between two tags. Returns null if either tag has no layout position. */
export function edgeGeometry(fromTag: string, toTag: string): EdgeGeometry | null {
  const from = NODE_POSITIONS[fromTag];
  const to = NODE_POSITIONS[toTag];
  if (!from || !to) {
    return null;
  }
  const start = pointOnNodeBoundary(from.x, from.y, to.x, to.y);
  const end = pointOnNodeBoundary(to.x, to.y, from.x, from.y);
  return {
    x1: start.x,
    y1: start.y,
    x2: end.x,
    y2: end.y,
    labelX: (start.x + end.x) / 2,
    labelY: (start.y + end.y) / 2 - 4,
  };
}

/** The delay-window label for an edge; empty for a rejected edge (no meaningful window). */
export function delayLabel(kind: string, delayMinSeconds: number, delayMaxSeconds: number): string {
  if (kind === 'rejected') {
    return '';
  }
  return delayMinSeconds === delayMaxSeconds ? `${delayMinSeconds}s` : `${delayMinSeconds}-${delayMaxSeconds}s`;
}
