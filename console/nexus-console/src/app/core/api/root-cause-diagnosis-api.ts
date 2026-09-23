import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Ch. 29 (Root Cause) -- the real diagnosis engine, reached over the network.
// Mirrors Nexus1.RootCause.Application.Diagnosis.DiagnosisResponse exactly. The
// route is a synchronous POST that RUNS the pipeline (engine decides, served
// model explains, H3/H4 validates, audit seals) and returns the sealed result;
// it is hosted by RootCause.Host and proxied by the BFF (ADR-034/ADR-035).
// Backend serves ONLY EVT-2026-0418 -- any other id returns 404.
//
// `weight` and `coverage` are ILLUSTRATIVE figures derived from the engineered
// graph's edge weights, not probabilities identified from plant data (the
// domain's own repeated caveat) -- screens must label them as such.
export interface DiagnosisCandidate {
  tag: string;
  role: string; // origin | proximate | parallel | contributing | ruled-out
  weight: number; // illustrative, 0..1
  coverage: number | null; // share of the flood explained, 0..1
}

export interface DiagnosisCitation {
  chunkId: number;
  sourceLabel: string;
}

export interface DiagnosisResponse {
  diagnosisRunId: number;
  incidentId: string;
  verdict: string | null; // origin tag when concluded; null when abstained
  abstained: boolean;
  abstainReason: string | null;
  candidates: DiagnosisCandidate[];
  citations: DiagnosisCitation[];
  auditHash: string; // the audit-chain head that sealed this run
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RootCauseDiagnosisApi {
  private readonly http = inject(HttpClient);

  /** Runs the diagnosis pipeline for the incident and returns its sealed result. A 404 means the backend has no such incident; 502/503 mean the pipeline could not run. */
  runDiagnosis(incidentId: string): Observable<DiagnosisResponse> {
    return this.http.post<DiagnosisResponse>(
      `${BFF_BASE_URL}/api/v1/root-cause/incidents/${encodeURIComponent(incidentId)}/diagnoses`,
      null,
    );
  }
}
