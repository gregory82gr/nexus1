import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Ch. 29 / ADR-036 -- the engineered causal-graph topology for an incident, read
// over the network from the read-only graph route (RootCause.Host -> BFF). Mirrors
// Nexus1.RootCause.Application.Diagnosis.IncidentGraphResponse. This is the real
// topology (nodes/edges/kinds/delay-windows) the Root Cause screen draws as
// Figure 29.1; on-screen positions are a separate frontend layout concern, not
// part of this data.
export interface GraphNode {
  componentId: number;
  tag: string;
  kind: string; // valve | pump | sensor | bus | condition
  status: string; // ok | degrading | failed
  alarmCount: number;
  illustrativeRole: string | null;
  illustrativeWeight: number | null;
}

export interface GraphEdge {
  fromComponentId: number;
  toComponentId: number;
  kind: string; // backbone | learned | artefact | rejected
  delayMinSeconds: number;
  delayMaxSeconds: number;
  sourceRef: string | null;
}

export interface IncidentGraph {
  incidentId: string;
  nodes: GraphNode[];
  edges: GraphEdge[];
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RootCauseGraphApi {
  private readonly http = inject(HttpClient);

  /** Reads the incident's causal-graph topology. Fast (a pure DB read, no inference); a 404 means the backend has no such incident. */
  getGraph(incidentId: string): Observable<IncidentGraph> {
    return this.http.get<IncidentGraph>(
      `${BFF_BASE_URL}/api/v1/root-cause/incidents/${encodeURIComponent(incidentId)}/graph`,
    );
  }
}
