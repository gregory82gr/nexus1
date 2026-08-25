import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.RadiationMonitoring.Application's own UnitRadiationSafetyDto
// exactly (Monitors + Zones). A dedicated client for the direct per-unit
// endpoint, not the heavier composite /overview/units/{id} response Ch. 6's
// Overview screen already folds this same data into (see overview-api.ts's
// own UnitRadiationSafety type) -- same "one real capability, called
// directly where a screen only needs it" pattern as radiation-zones-api.ts
// versus that same composite endpoint's Zones field.
export interface UnitRadiationMonitorReading {
  monitorCode: string;
  monitorName: string;
  monitorStatus: string;
  latestValue: number | null;
  engineeringUnitSymbol: string | null;
  quality: string | null;
  latestReadingAtUtc: string | null;
}

export interface UnitRadiationZone {
  code: string;
  name: string;
  classification: string;
  status: string;
}

export interface UnitRadiationSafety {
  unitId: number;
  monitors: UnitRadiationMonitorReading[];
  zones: UnitRadiationZone[];
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RadiationSafetyApi {
  private readonly http = inject(HttpClient);

  getUnitRadiationSafety(unitId: number): Observable<UnitRadiationSafety> {
    return this.http.get<UnitRadiationSafety>(`${BFF_BASE_URL}/api/v1/radiation-monitoring/units/${unitId}`);
  }
}
