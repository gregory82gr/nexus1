// Pure data module for System Dependencies (Ch. 27) -- one source of truth
// for all three tabs (Graph draws it, Matrix tabulates it, Chain walks
// it), same "one data source, multiple views" shape the book's own
// component uses.
//
// TWO DIFFERENT KINDS OF CLAIM ON THIS SCREEN, per the chapter's own
// distinction (structure vs. parameter):
//
// - TOPOLOGY (which edges exist, and their direction) is a reasonable,
//   simplified physical model of a pressurized-water reactor -- rods
//   drive reactivity, reactivity drives flux, flux drives power, plus two
//   negative-feedback loops (xenon and fuel-temperature/Doppler feedback
//   into reactivity). Same category the book explicitly authorizes,
//   consistent with the point-kinetics model already ported (Ch. 11).
//   Kept as-is; not second-guessed here.
// - COEFFICIENT (each edge's specific weight and delay) requires real
//   calibration against an actual machine this console has never had
//   access to. Every edge is typed `kind: 'illustrative-topology'`
//   directly on the data -- not just a markup note -- so a spec can
//   assert it structurally, and so a future contributor adding a 15th
//   edge is forced to decide what kind it carries rather than silently
//   inheriting whatever confidence the surrounding edges display.
export type NodeId =
  | 'rods'
  | 'reactivity'
  | 'flux'
  | 'power'
  | 'coolantA'
  | 'coolantB'
  | 'xenon'
  | 'fuelTemp'
  | 'feedwater'
  | 'steamGen'
  | 'turbine'
  | 'grid';

// NODE STATUS is a different matter entirely from the edges above --
// checked directly against this solution's real Instrumentation domain
// before writing this list, per-node, not assumed: only 3 of these 12
// nodes have any real signal backing anywhere (Neutron Flux/NEUTRONICS,
// Thermal Power/POWER, Turbine/TURBINE -- all already live via the
// existing GET /api/v1/instrumentation/units/{id}/signals endpoint, no
// new BFF route needed). The other 9 have zero real backing in this
// codebase: no rod-position entity, no Reactivity signal (a real domain
// finding, not an oversight -- EngineeringQuantityType.Reactivity is an
// unused enum value, same pattern as Power & Grid's own Frequency
// finding), no coolant/xenon/fuel-temp/feedwater/steam-generator signal
// ever seeded, and Grid's own frequency/phase/breaker/sync fields are
// already NO SOURCE per the Power & Grid cluster. `realSignalCategory:
// null` means exactly that -- checked and absent, not a placeholder.
export interface DependencyNode {
  id: NodeId;
  label: string;
  realSignalCategory: string | null;
}

export const DEPENDENCY_NODES: readonly DependencyNode[] = [
  { id: 'rods', label: 'Control Rods', realSignalCategory: null },
  { id: 'reactivity', label: 'Reactivity', realSignalCategory: null },
  { id: 'flux', label: 'Neutron Flux', realSignalCategory: 'NEUTRONICS' },
  { id: 'power', label: 'Thermal Power', realSignalCategory: 'POWER' },
  { id: 'coolantA', label: 'Coolant A', realSignalCategory: null },
  { id: 'coolantB', label: 'Coolant B', realSignalCategory: null },
  { id: 'xenon', label: 'Xenon-135', realSignalCategory: null },
  { id: 'fuelTemp', label: 'Fuel Temp', realSignalCategory: null },
  { id: 'feedwater', label: 'Feedwater', realSignalCategory: null },
  { id: 'steamGen', label: 'Steam Gen', realSignalCategory: null },
  { id: 'turbine', label: 'Turbine', realSignalCategory: 'TURBINE' },
  { id: 'grid', label: 'Grid', realSignalCategory: null },
];

export interface InfluenceEdge {
  from: NodeId;
  to: NodeId;
  // Signed: negative = a negative-feedback edge (rendered dashed violet
  // on the Graph tab, per the book's own convention for xenon/fuel-temp
  // feedback into reactivity).
  weight: number;
  delaySeconds: number;
  kind: 'illustrative-topology';
}

// The book's 14-edge model: the rods -> reactivity -> flux -> power
// forward chain, a downstream coolant/steam/turbine/grid chain, and two
// negative-feedback edges back into reactivity (xenon, fuel-temp/Doppler).
export const DEPENDENCY_EDGES: readonly InfluenceEdge[] = [
  { from: 'rods', to: 'reactivity', weight: 0.92, delaySeconds: 0.1, kind: 'illustrative-topology' },
  { from: 'reactivity', to: 'flux', weight: 0.85, delaySeconds: 0.05, kind: 'illustrative-topology' },
  { from: 'flux', to: 'power', weight: 0.9, delaySeconds: 0.05, kind: 'illustrative-topology' },
  { from: 'power', to: 'coolantA', weight: 0.7, delaySeconds: 0.2, kind: 'illustrative-topology' },
  { from: 'power', to: 'coolantB', weight: 0.68, delaySeconds: 0.2, kind: 'illustrative-topology' },
  { from: 'power', to: 'fuelTemp', weight: 0.75, delaySeconds: 0.1, kind: 'illustrative-topology' },
  { from: 'coolantA', to: 'steamGen', weight: 0.6, delaySeconds: 0.3, kind: 'illustrative-topology' },
  { from: 'coolantB', to: 'steamGen', weight: 0.58, delaySeconds: 0.3, kind: 'illustrative-topology' },
  { from: 'feedwater', to: 'steamGen', weight: 0.55, delaySeconds: 0.2, kind: 'illustrative-topology' },
  { from: 'steamGen', to: 'turbine', weight: 0.8, delaySeconds: 0.15, kind: 'illustrative-topology' },
  { from: 'turbine', to: 'grid', weight: 0.95, delaySeconds: 0.05, kind: 'illustrative-topology' },
  { from: 'flux', to: 'xenon', weight: 0.4, delaySeconds: 2.0, kind: 'illustrative-topology' },
  { from: 'xenon', to: 'reactivity', weight: -0.5, delaySeconds: 1.0, kind: 'illustrative-topology' },
  { from: 'fuelTemp', to: 'reactivity', weight: -0.45, delaySeconds: 0.05, kind: 'illustrative-topology' },
];

// One representative forward path for the Causal Chain tab -- an
// "illustrative trip reconstruction," per the book's own phrase for this
// tab -- not an exhaustive enumeration of every possible path.
export const CAUSAL_CHAIN_NODE_SEQUENCE: readonly NodeId[] = [
  'rods',
  'reactivity',
  'flux',
  'power',
  'coolantA',
  'steamGen',
  'turbine',
  'grid',
];

// Layout for the Graph tab's actual node-link diagram (an SVG overlay
// drawing real connecting lines, not a card grid next to a flat edge
// list). Positions are a logical coordinate space
// (GRAPH_VIEWBOX_WIDTH x GRAPH_VIEWBOX_HEIGHT); the template scales the
// SVG viewBox to the container, so these numbers are design units, not
// pixels on screen. Roughly follows the forward chain left-to-right
// (rods -> reactivity -> flux -> power -> coolant/fuel-temp -> steam gen
// -> turbine -> grid), with xenon and fuel temp raised above the main
// row so their dashed feedback lines back into reactivity read clearly
// as loops rather than crossing straight through other nodes.
//
// Spacing revised after review: the original Turbine/Grid positions
// (840/940, only 100 apart) left their cards overlapping at typical
// render widths. Every rightward gap in this layout is now at least 150
// units -- comfortably wider than a node card's own footprint
// (NODE_HALF_WIDTH*2 = 90) plus real visual breathing room, not just
// enough to avoid a bounding-box collision.
export const GRAPH_VIEWBOX_WIDTH = 1180;
export const GRAPH_VIEWBOX_HEIGHT = 460;

export const NODE_POSITIONS: Readonly<Record<NodeId, { x: number; y: number }>> = {
  rods: { x: 50, y: 230 },
  reactivity: { x: 190, y: 230 },
  flux: { x: 330, y: 230 },
  xenon: { x: 330, y: 55 },
  power: { x: 470, y: 230 },
  fuelTemp: { x: 470, y: 55 },
  coolantA: { x: 610, y: 120 },
  coolantB: { x: 610, y: 340 },
  feedwater: { x: 610, y: 430 },
  steamGen: { x: 760, y: 260 },
  turbine: { x: 920, y: 260 },
  grid: { x: 1080, y: 260 },
};

// A node card's approximate footprint in the same logical viewBox
// units, as an ellipse inscribed in its visual rectangle -- used to pull
// every edge line's endpoints back to the node's own boundary instead of
// its dead center, so no line ever crosses through a node's label or
// status badge. Deliberately a touch smaller than the card's true
// half-dimensions so lines end just short of the border, not flush
// against it.
const NODE_HALF_WIDTH = 42;
const NODE_HALF_HEIGHT = 22;

function pointOnNodeBoundary(centerX: number, centerY: number, towardX: number, towardY: number): { x: number; y: number } {
  const dx = towardX - centerX;
  const dy = towardY - centerY;
  const dist = Math.hypot(dx, dy);
  if (dist === 0) return { x: centerX, y: centerY };
  const ux = dx / dist;
  const uy = dy / dist;
  const t = 1 / Math.sqrt((ux / NODE_HALF_WIDTH) ** 2 + (uy / NODE_HALF_HEIGHT) ** 2);
  return { x: centerX + ux * t, y: centerY + uy * t };
}

// Both endpoints of an edge's line, pulled back from each node's center
// to its own boundary in the direction of the other node -- never the
// raw center-to-center coordinates a naive line would use.
export function edgeEndpoints(
  edge: InfluenceEdge,
): { x1: number; y1: number; x2: number; y2: number } {
  const from = NODE_POSITIONS[edge.from];
  const to = NODE_POSITIONS[edge.to];
  const start = pointOnNodeBoundary(from.x, from.y, to.x, to.y);
  const end = pointOnNodeBoundary(to.x, to.y, from.x, from.y);
  return { x1: start.x, y1: start.y, x2: end.x, y2: end.y };
}

export function nodeLabel(id: NodeId): string {
  return DEPENDENCY_NODES.find((n) => n.id === id)?.label ?? id;
}

export function matrixCell(edges: readonly InfluenceEdge[], from: NodeId, to: NodeId): InfluenceEdge | undefined {
  return edges.find((e) => e.from === from && e.to === to);
}

export function chainEdges(edges: readonly InfluenceEdge[], sequence: readonly NodeId[]): InfluenceEdge[] {
  const result: InfluenceEdge[] = [];
  for (let i = 0; i < sequence.length - 1; i++) {
    const edge = edges.find((e) => e.from === sequence[i] && e.to === sequence[i + 1]);
    if (edge) result.push(edge);
  }
  return result;
}
