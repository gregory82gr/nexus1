import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Audit.Application's own AuditEvidenceRecordDto and
// Nexus1.Compliance.Application's own ComplianceReviewDto exactly. Both
// endpoints are scoped per RootCause analysis id (an opaque long) --
// RootCause stays out-of-process (ADR-001), and neither Audit nor
// Compliance has a UnitId anywhere, so there is no fleet-wide listing to
// fall back to. See audit-log.ts's own doc comment for how this screen
// works within that real scoping boundary.
//
// sourceAnalysisId in both DTOs is a C# `long`; the real analysis ids
// already seen live (e.g. 639223958897100060) exceed
// Number.MAX_SAFE_INTEGER, so the JSON-deserialized numeric value can
// silently lose precision in JS. Deliberately never read from either
// interface below -- every caller already knows the id it looked up
// (it's the string the operator typed), so the response's own numeric
// copy is simply not trusted for round-tripping.
export interface AuditEvidenceRecord {
  auditEvidenceId: string;
  eventType: string;
  schemaVersion: number;
  correlationId: string;
  causationId: string | null;
  envelopeSha256Hex: string;
  occurredAtUtc: string;
  recordedAtUtc: string;
}

export interface ComplianceReview {
  complianceReviewId: string;
  verdict: string;
  state: string;
  openedAtUtc: string;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class AuditApi {
  private readonly http = inject(HttpClient);

  getEvidence(analysisId: string): Observable<AuditEvidenceRecord[]> {
    return this.http.get<AuditEvidenceRecord[]>(`${BFF_BASE_URL}/api/v1/audit/analyses/${analysisId}/evidence`);
  }

  getComplianceReviews(analysisId: string): Observable<ComplianceReview[]> {
    return this.http.get<ComplianceReview[]>(`${BFF_BASE_URL}/api/v1/compliance/analyses/${analysisId}/reviews`);
  }
}
