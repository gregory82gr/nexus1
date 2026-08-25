import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, of } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

// Mirrors Nexus1.ReinforcementLearning.Application's own PolicyGridEntryDto
// and ClampedRecommendationDto exactly. Both routes already existed and
// were already live-proven before this cluster (2026-08-23 BFF slice) --
// no new backend code for this cluster.
export interface PolicyGridEntry {
  stateIndex: number;
  stateCode: string;
  bestActionCode: string;
  bestQValue: number;
  actionMargin: number | null;
}

export interface ClampedRecommendation {
  advisoryRecommendationId: number;
  requestedAtUtc: string;
  stateCode: string;
  recommendedActionCode: string;
  clampedActionCode: string | null;
  clampReason: string | null;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class ReinforcementLearningApi {
  private readonly http = inject(HttpClient);

  // The BFF returns a real 404 when no final policy has ever been
  // extracted -- a meaningful domain state ("no policy yet"), not a
  // connectivity failure. Mapped here to `null` so the component can tell
  // the two apart, rather than treating "no policy" as "unreachable."
  getActivePolicyGrid(): Observable<PolicyGridEntry[] | null> {
    return this.http.get<PolicyGridEntry[]>(`${BFF_BASE_URL}/api/v1/reinforcement-learning/policy`).pipe(
      catchError((err: HttpErrorResponse) => (err.status === 404 ? of(null) : this.rethrow(err))),
    );
  }

  getClampedRecommendations(): Observable<ClampedRecommendation[]> {
    return this.http.get<ClampedRecommendation[]>(`${BFF_BASE_URL}/api/v1/reinforcement-learning/recommendations`);
  }

  private rethrow(err: HttpErrorResponse): Observable<never> {
    throw err;
  }
}
