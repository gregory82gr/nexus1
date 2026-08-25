import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { InstrumentationApi, UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';
import {
  CAUSAL_CHAIN_NODE_SEQUENCE,
  DEPENDENCY_EDGES,
  DEPENDENCY_NODES,
  DependencyNode,
  GRAPH_VIEWBOX_HEIGHT,
  GRAPH_VIEWBOX_WIDTH,
  InfluenceEdge,
  NODE_POSITIONS,
  NodeId,
  chainEdges,
  edgeEndpoints,
  matrixCell,
  nodeLabel,
} from './dependency-graph';

export interface EdgeLine {
  edge: InfluenceEdge;
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  strokeWidth: number;
}

// System Dependencies (Ch. 27) -- three tabs over one underlying claim
// (X influences Y, by some strength, after some delay). The book's own
// finding: Matrix and Chain both disclose their coefficients as
// "illustrative, not identified from plant data"; the Graph tab, which
// loads first, discloses nothing, despite using the exact same kind of
// hand-authored constant.
//
// THE STRUCTURAL FIX, not an editorial one: the disclosure banner lives
// ONCE, here, on the component that owns all three tabs -- rendered
// above the tab switch in dependencies.html, never duplicated into a
// per-tab template. Switching tabs (`tab` below) can change which panel
// renders; it cannot make the banner disappear, because the banner isn't
// inside any of the three panels to begin with. See dependency-graph.ts's
// own doc comment for why every edge is ALSO typed illustrative-topology
// directly on the data, not just described in this banner's text.
//
// THE GRAPH TAB DRAWS AN ACTUAL NODE-LINK DIAGRAM, per review feedback:
// an earlier version paired a card grid with a separate flat text list
// of edges below it -- correct data, but not a graph, and a tab named
// "Graph" that doesn't draw one is misleading in a different way than
// the chapter's own disclosure finding. Fixed with a real SVG line
// layer (edgeLines below) connecting node-card centers, solid for
// forward influence with width proportional to |weight|, dashed violet
// for the two negative-feedback edges -- matching the book's own
// Figure 27.1 convention exactly ("Solid = forward influence, width ∝
// illustrative weight. Dashed violet = negative feedback"). The numeric
// list is kept below the diagram as a supplementary, precise reading of
// the same 14 edges, not as a substitute for drawing them.
//
// NODE STATUS is a completely different kind of claim from the edges,
// per the chapter's own distinction, and was investigated separately:
// checked this solution's real Instrumentation domain directly, node by
// node, before writing dependency-graph.ts's own node list. Three of
// twelve nodes have real signal backing (Neutron Flux, Thermal Power,
// Turbine) -- reused here via the same InstrumentationApi.getSignals()
// call already proven for the Reactor cluster and Power & Grid, no new
// BFF route. The other nine have zero real backing anywhere, checked and
// confirmed absent (not merely unwired) -- they render with a distinct,
// non-color-coded "NO SOURCE" treatment (dashed border, hatched
// background, explicit badge) so a reader can never mistake an
// unbacked node for a real-but-nominal one from styling alone.
type NodeDisplayStatus =
  | { kind: 'no-source' }
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'real'; value: number; qualityCode: string | null }
  | { kind: 'real-no-reading' };

type SignalsState =
  | { status: 'loading' }
  | { status: 'error' }
  | { status: 'loaded'; signals: UnitSignalReading[] };

@Component({
  selector: 'nx-dependencies',
  standalone: true,
  templateUrl: './dependencies.html',
  styleUrl: './dependencies.scss',
})
export class DependenciesComponent {
  private readonly api = inject(InstrumentationApi);
  private readonly plantState = inject(PlantStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tab = signal<'graph' | 'matrix' | 'chain'>('graph');

  readonly nodes: readonly DependencyNode[] = DEPENDENCY_NODES;
  readonly edges: readonly InfluenceEdge[] = DEPENDENCY_EDGES;

  // True for every edge in this dataset today -- asserted, not assumed.
  readonly allIllustrative = this.edges.every((e) => e.kind === 'illustrative-topology');

  readonly chain = chainEdges(this.edges, CAUSAL_CHAIN_NODE_SEQUENCE);

  // The Graph tab draws an actual node-link diagram (real connecting
  // lines, not a card grid next to a flat text list) -- solid for
  // forward influence, dashed violet for the two negative-feedback
  // edges, matching the book's own Figure 27.1 convention. viewBox and
  // node positions are the one shared layout (dependency-graph.ts);
  // this getter just resolves each edge's two endpoints against it.
  readonly viewBoxWidth = GRAPH_VIEWBOX_WIDTH;
  readonly viewBoxHeight = GRAPH_VIEWBOX_HEIGHT;

  // Endpoints are pulled back to each node's own boundary (edgeEndpoints,
  // dependency-graph.ts), never the raw center-to-center coordinates --
  // per review feedback, a line landing dead-center on a node overlapped
  // its label/status text (most visibly the two feedback lines
  // converging on Reactivity's own "NO SOURCE" badge).
  readonly edgeLines: readonly EdgeLine[] = this.edges.map((edge) => {
    const { x1, y1, x2, y2 } = edgeEndpoints(edge);
    // Line width proportional to |weight|, per the book's own Figure
    // 27.1 convention ("width ∝ illustrative weight") -- computed here,
    // not in the template, so the template needs no global Math access.
    return { edge, x1, y1, x2, y2, strokeWidth: 1 + Math.abs(edge.weight) * 4 };
  });

  // Node cards are positioned in the same logical space the SVG lines
  // use, as percentages of the viewBox, so a card and the line ends
  // meeting it never drift apart regardless of the container's actual
  // rendered size.
  readonly nodeStyles: Readonly<Record<NodeId, { left: string; top: string }>> = Object.fromEntries(
    DEPENDENCY_NODES.map((n) => {
      const p = NODE_POSITIONS[n.id];
      return [n.id, { left: `${(p.x / GRAPH_VIEWBOX_WIDTH) * 100}%`, top: `${(p.y / GRAPH_VIEWBOX_HEIGHT) * 100}%` }];
    }),
  ) as Record<NodeId, { left: string; top: string }>;

  private readonly signalsState = signal<SignalsState>({ status: 'loading' });

  constructor() {
    this.api
      .getSignals(this.plantState.selectedId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (signals) => this.signalsState.set({ status: 'loaded', signals }),
        error: () => this.signalsState.set({ status: 'error' }),
      });
  }

  setTab(t: 'graph' | 'matrix' | 'chain'): void {
    this.tab.set(t);
  }

  statusFor(node: DependencyNode): NodeDisplayStatus {
    if (node.realSignalCategory === null) return { kind: 'no-source' };

    const s = this.signalsState();
    if (s.status === 'loading') return { kind: 'loading' };
    if (s.status === 'error') return { kind: 'error' };

    const signal = s.signals.find((sig) => sig.categoryCode === node.realSignalCategory);
    if (!signal || signal.latestValue === null) return { kind: 'real-no-reading' };
    return { kind: 'real', value: signal.latestValue, qualityCode: signal.latestQualityCode };
  }

  labelOf(id: NodeId): string {
    return nodeLabel(id);
  }

  cell(from: NodeId, to: NodeId): InfluenceEdge | undefined {
    return matrixCell(this.edges, from, to);
  }
}
