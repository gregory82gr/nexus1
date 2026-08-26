import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Bff's own ContextHealthResult exactly. Not under
// /api/v1/... -- this is a health endpoint (GET /health/contexts),
// alongside the existing /health/live and /health/ready. Real per-context
// database CONNECTIVITY reachability (CanConnectAsync + pending-migration
// check) for whichever contexts are actually composed in the running
// host -- never service-level monitoring (message processing, queue
// depth, business logic), which this backend has no way to compute for
// anything. See features/security/security.ts's own doc comment for the
// full investigation behind why this, and not the book's own
// telRate/twinLag-driven panel, is what's real here.
export interface ContextHealthResult {
  contextName: string;
  status: string;
  durationMs: number;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class SecurityApi {
  private readonly http = inject(HttpClient);

  getContextHealth(): Observable<ContextHealthResult[]> {
    return this.http.get<ContextHealthResult[]>(`${BFF_BASE_URL}/health/contexts`);
  }
}
