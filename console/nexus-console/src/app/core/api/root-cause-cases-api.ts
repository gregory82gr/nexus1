import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Reporting.Application's own CaseSummaryDto exactly. The
// route (GET /api/v1/reporting/units/{id}) is hosted by Reporting, but the
// real content is RootCause's own investigation-case history (an alarm
// flood opened a case; eventually a free-text verdict) -- named after what
// this data actually is, not the technical host, same discipline as
// radiation-safety-api.ts / alarm-management-api.ts. An earlier slice's own
// route comment called this "the Trends & History screen's" data; that was
// a non-binding naming guess from before this cluster's own investigation --
// see ai-diagnostics.ts's own doc comment for why it belongs here instead.
export interface RootCauseCase {
  caseId: number;
  unitId: number;
  alarmFloodId: number;
  status: string;
  verdict: string | null;
  openedAtUtc: string;
  verdictIssuedAtUtc: string | null;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RootCauseCasesApi {
  private readonly http = inject(HttpClient);

  getCasesForUnit(unitId: number): Observable<RootCauseCase[]> {
    return this.http.get<RootCauseCase[]>(`${BFF_BASE_URL}/api/v1/reporting/units/${unitId}`);
  }
}
