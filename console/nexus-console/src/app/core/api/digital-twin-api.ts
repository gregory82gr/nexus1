import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.DigitalTwin.Application's own UnitTwinStateDto exactly
// (field for field). Per-unit only -- there is no fleet-wide digital-twin
// HTTP route: a GetActiveTwinsForFleetQuery exists in the Application
// layer, but Nexus1.Bff's Program.cs only ever maps the per-unit route,
// GET /api/v1/digital-twin/units/{id}. Does NOT include divergence/
// sync-drift data -- a named gap on the endpoint's own side (see its
// doc comment in Program.cs and IActiveTwinFinder.GetActiveTwinsForUnitAsync),
// not something this client omits.
export interface UnitTwinState {
  unitId: number;
  unitCode: string;
  twinCode: string;
  twinName: string;
  modelType: string;
  status: string;
  fidelity: string;
  isAuthoritative: boolean;
}

// Digital Twin screen (Appendix A follow-up): the three routes Program.cs
// added alongside the per-unit one above, mirroring ActiveTwinDto,
// ModelVariableSignalTraceDto, and OpenDivergenceDto exactly. See
// features/digital-twin/digital-twin.ts's own doc comment for the
// investigation these three shapes were traced from.
export interface ActiveTwin {
  unitCode: string;
  twinCode: string;
  modelType: string;
  status: string;
  fidelity: string;
}

export interface ModelVariableSignalTrace {
  twinCode: string;
  modelVariable: string;
  signalTag: string;
  bindingRole: string;
  bindingStatus: string;
}

// Real modeled-vs-measured delta per signal (OpenDivergenceDto) -- fleet-wide
// only. No per-unit variant exists anywhere in this backend: reaching a
// specific unit's divergences requires a real four-hop join
// (TwinDivergence -> TwinSnapshot -> TwinRuntimeSession -> TwinModelVersion
// -> TwinModel.UnitId) that no existing query performs -- checked directly,
// not assumed. This interface carries no unit field because the real DTO
// doesn't either.
export interface OpenDivergence {
  detectedAtUtc: string;
  signalTag: string;
  modelVariable: string | null;
  modeledValue: number;
  measuredValue: number;
  deltaValue: number;
  severity: string;
  status: string;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class DigitalTwinApi {
  private readonly http = inject(HttpClient);

  // Returns an ARRAY, not a single object -- GetUnitTwinStateQueryHandler's
  // own signature is IQueryHandler<..., IReadOnlyList<UnitTwinStateDto>>,
  // because a unit can legitimately have more than one active, non-deleted
  // twin model (IsAuthoritative marks which one is live). Always HTTP 200,
  // even for a unit with none -- Program.cs's route always does
  // Results.Ok(result.Value); an empty list means "no twin modeled for
  // this unit," which the endpoint's own prior evidence explicitly
  // documents as "not an error" -- confirmed live, not assumed, before
  // this client was written to expect a single object.
  getUnitTwinStates(unitId: number): Observable<UnitTwinState[]> {
    return this.http.get<UnitTwinState[]>(`${BFF_BASE_URL}/api/v1/digital-twin/units/${unitId}`);
  }

  getFleetTwins(): Observable<ActiveTwin[]> {
    return this.http.get<ActiveTwin[]>(`${BFF_BASE_URL}/api/v1/digital-twin/fleet`);
  }

  getSignalTrace(twinCode: string): Observable<ModelVariableSignalTrace[]> {
    return this.http.get<ModelVariableSignalTrace[]>(`${BFF_BASE_URL}/api/v1/digital-twin/twins/${encodeURIComponent(twinCode)}/signals`);
  }

  getDivergences(): Observable<OpenDivergence[]> {
    return this.http.get<OpenDivergence[]>(`${BFF_BASE_URL}/api/v1/digital-twin/divergences`);
  }
}
