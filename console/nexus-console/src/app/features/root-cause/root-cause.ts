import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FIXED_INCIDENT_ID, RootCauseService } from './root-cause.service';
import { GraphNode, IncidentGraph, RootCauseGraphApi } from '../../core/api/root-cause-graph-api';
import {
  GRAPH_VIEWBOX_HEIGHT,
  GRAPH_VIEWBOX_WIDTH,
  NODE_POSITIONS,
  delayLabel,
  edgeGeometry,
} from './root-cause-graph-layout';

// Root Cause (Ch. 29) -- the console's most rigorous screen: the engineered
// causal verdict for EVT-2026-0418, read from the real diagnosis pipeline
// (ADR-032..ADR-035), plus the fault-tree graph (Figure 29.1) drawn from the real
// backend topology (ADR-036). The screen never computes a conclusion; the verdict
// comes from the one RootCauseService owns, and the graph from its own fast
// read-only topology endpoint. Confidences are shown as illustrative. The graph's
// STRUCTURE (nodes/edges/kinds/delay-windows) is 100% backend; only the on-screen
// positions are a presentation layout (root-cause-graph-layout.ts).
type GraphState = { status: 'loading' } | { status: 'ready'; graph: IncidentGraph } | { status: 'error' };

interface RenderNode {
  node: GraphNode;
  x: number;
  y: number;
  role: string; // the seeded illustrative role, or 'origin' when this tag is the computed verdict
}

interface RenderEdge {
  fromTag: string;
  toTag: string;
  kind: string;
  label: string;
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  labelX: number;
  labelY: number;
}

@Component({
  selector: 'nx-root-cause',
  standalone: true,
  templateUrl: './root-cause.html',
  styleUrl: './root-cause.scss',
})
export class RootCauseComponent {
  private readonly rootCause = inject(RootCauseService);
  private readonly graphApi = inject(RootCauseGraphApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly incidentId = FIXED_INCIDENT_ID;

  // Verdict (slow -- the diagnosis pipeline, 20-100s).
  readonly state = this.rootCause.state;
  readonly citations = this.rootCause.citations;
  readonly auditHash = this.rootCause.auditHash;

  readonly result = computed(() => {
    const s = this.state();
    return s.status === 'ready' ? s.result : null;
  });

  readonly abstainReason = computed(() => {
    const s = this.state();
    return s.status === 'abstained' ? s.reason : null;
  });

  readonly errorMessage = computed(() => {
    const s = this.state();
    return s.status === 'error' ? s.message : '';
  });

  // Graph (fast -- a pure DB read, draws immediately while the verdict runs).
  readonly graphState = signal<GraphState>({ status: 'loading' });
  readonly viewBoxWidth = GRAPH_VIEWBOX_WIDTH;
  readonly viewBoxHeight = GRAPH_VIEWBOX_HEIGHT;

  // The origin is not a stored topology attribute -- it is the diagnosis's computed
  // verdict. Marking it here (once the verdict lands) cross-references the real
  // result rather than faking a role on the graph data; null while the pipeline runs
  // or if it abstains.
  private readonly originTag = computed(() => this.result()?.verdict ?? null);

  readonly renderNodes = computed<RenderNode[]>(() => {
    const s = this.graphState();
    if (s.status !== 'ready') {
      return [];
    }
    const origin = this.originTag();
    return s.graph.nodes
      .map((node) => {
        const pos = NODE_POSITIONS[node.tag];
        if (!pos) {
          return null;
        }
        const role = node.tag === origin ? 'origin' : (node.illustrativeRole ?? 'none');
        return { node, x: pos.x, y: pos.y, role };
      })
      .filter((n): n is RenderNode => n !== null);
  });

  readonly renderEdges = computed<RenderEdge[]>(() => {
    const s = this.graphState();
    if (s.status !== 'ready') {
      return [];
    }
    const tagById = new Map(s.graph.nodes.map((n) => [n.componentId, n.tag]));
    const edges: RenderEdge[] = [];
    for (const e of s.graph.edges) {
      const fromTag = tagById.get(e.fromComponentId);
      const toTag = tagById.get(e.toComponentId);
      if (!fromTag || !toTag) {
        continue;
      }
      const geo = edgeGeometry(fromTag, toTag);
      if (!geo) {
        continue;
      }
      edges.push({
        fromTag,
        toTag,
        kind: e.kind,
        label: delayLabel(e.kind, e.delayMinSeconds, e.delayMaxSeconds),
        ...geo,
      });
    }
    return edges;
  });

  constructor() {
    this.rootCause.load(FIXED_INCIDENT_ID);
    this.graphApi
      .getGraph(FIXED_INCIDENT_ID)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (graph) => this.graphState.set({ status: 'ready', graph }),
        error: () => this.graphState.set({ status: 'error' }),
      });
  }

  pct(value: number | null): string {
    return value == null ? '—' : `${Math.round(value * 100)}%`;
  }
}
