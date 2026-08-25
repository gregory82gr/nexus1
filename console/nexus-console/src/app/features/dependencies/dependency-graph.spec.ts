import {
  CAUSAL_CHAIN_NODE_SEQUENCE,
  DEPENDENCY_EDGES,
  DEPENDENCY_NODES,
  GRAPH_VIEWBOX_HEIGHT,
  GRAPH_VIEWBOX_WIDTH,
  NODE_POSITIONS,
  chainEdges,
  edgeEndpoints,
  matrixCell,
} from './dependency-graph';

describe('dependency-graph data', () => {
  it('has exactly the book\'s 14-edge topology', () => {
    expect(DEPENDENCY_EDGES).toHaveLength(14);
  });

  it('types every edge as illustrative-topology -- no untyped literal can slip past the union', () => {
    expect(DEPENDENCY_EDGES.every((e) => e.kind === 'illustrative-topology')).toBe(true);
  });

  it('fails the all-typed check if a future edge omits its kind -- a structural guard', () => {
    const withGap = [...DEPENDENCY_EDGES, { ...DEPENDENCY_EDGES[0], kind: undefined as unknown as 'illustrative-topology' }];
    expect(withGap.every((e) => e.kind === 'illustrative-topology')).toBe(false);
  });

  it('has exactly 12 nodes', () => {
    expect(DEPENDENCY_NODES).toHaveLength(12);
  });

  it('has real signal backing for exactly 3 nodes (Neutron Flux, Thermal Power, Turbine), matching the investigation', () => {
    const real = DEPENDENCY_NODES.filter((n) => n.realSignalCategory !== null);
    expect(real).toHaveLength(3);
    expect(real.map((n) => n.id).sort()).toEqual(['flux', 'power', 'turbine']);
  });

  it('declares no real backing (null) for the other 9 nodes -- checked and absent, not omitted', () => {
    const gaps = DEPENDENCY_NODES.filter((n) => n.realSignalCategory === null);
    expect(gaps).toHaveLength(9);
  });

  it('includes two negative-feedback edges into reactivity (xenon, fuel-temp/Doppler)', () => {
    const feedback = DEPENDENCY_EDGES.filter((e) => e.weight < 0);
    expect(feedback).toHaveLength(2);
    expect(feedback.every((e) => e.to === 'reactivity')).toBe(true);
  });

  it('matrixCell finds the real edge weight for a known pair and undefined for a non-edge', () => {
    expect(matrixCell(DEPENDENCY_EDGES, 'rods', 'reactivity')?.weight).toBe(0.92);
    expect(matrixCell(DEPENDENCY_EDGES, 'rods', 'grid')).toBeUndefined();
  });

  it('chainEdges walks the same typed edge dataset the graph and matrix use, feedback edges excluded', () => {
    const chain = chainEdges(DEPENDENCY_EDGES, CAUSAL_CHAIN_NODE_SEQUENCE);
    expect(chain).toHaveLength(CAUSAL_CHAIN_NODE_SEQUENCE.length - 1);
    expect(chain.every((e) => e.kind === 'illustrative-topology')).toBe(true);
    expect(chain.some((e) => e.weight < 0)).toBe(false);
  });

  it('gives every node a real position inside the graph viewBox, for the Graph tab\'s actual node-link diagram', () => {
    for (const n of DEPENDENCY_NODES) {
      const pos = NODE_POSITIONS[n.id];
      expect(pos).toBeDefined();
      expect(pos.x).toBeGreaterThanOrEqual(0);
      expect(pos.x).toBeLessThanOrEqual(GRAPH_VIEWBOX_WIDTH);
      expect(pos.y).toBeGreaterThanOrEqual(0);
      expect(pos.y).toBeLessThanOrEqual(GRAPH_VIEWBOX_HEIGHT);
    }
  });

  it('keeps every adjacent pair of node positions far enough apart that their cards cannot touch -- Turbine/Grid regression', () => {
    // Node-card footprint is ~90 units wide (2x NODE_HALF_WIDTH) -- any
    // two nodes closer than that on both axes simultaneously would
    // visually overlap. Checked pairwise for every node, not just the
    // one pair (Turbine/Grid) the review caught.
    const ids = DEPENDENCY_NODES.map((n) => n.id);
    for (let i = 0; i < ids.length; i++) {
      for (let j = i + 1; j < ids.length; j++) {
        const a = NODE_POSITIONS[ids[i]];
        const b = NODE_POSITIONS[ids[j]];
        const dx = Math.abs(a.x - b.x);
        const dy = Math.abs(a.y - b.y);
        const tooClose = dx < 90 && dy < 50;
        expect(tooClose).toBe(false);
      }
    }
  });

  it('pulls every edge line\'s endpoints back to the node boundary, never the raw node center', () => {
    for (const edge of DEPENDENCY_EDGES) {
      const from = NODE_POSITIONS[edge.from];
      const to = NODE_POSITIONS[edge.to];
      const { x1, y1, x2, y2 } = edgeEndpoints(edge);
      expect([x1, y1]).not.toEqual([from.x, from.y]);
      expect([x2, y2]).not.toEqual([to.x, to.y]);
    }
  });

  it('keeps the two feedback lines into reactivity clear of its own center, where the NO SOURCE text sits', () => {
    const reactivityCenter = NODE_POSITIONS['reactivity'];
    const feedback = DEPENDENCY_EDGES.filter((e) => e.weight < 0 && e.to === 'reactivity');
    for (const edge of feedback) {
      const { x2, y2 } = edgeEndpoints(edge);
      const distanceFromCenter = Math.hypot(x2 - reactivityCenter.x, y2 - reactivityCenter.y);
      expect(distanceFromCenter).toBeGreaterThan(15);
    }
  });
});
