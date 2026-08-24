import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.RadiationMonitoring.Application's own
// ActiveRadiationZoneDto exactly. Fleet-wide (no unit parameter) --
// unlike UnitRadiationSafetyDto.Zones, which is per-unit and already
// used by the Overview screen. See
// features/zone-registry/zone-registry.ts's own doc comment for why
// this is the real data behind the Zone Access nav group, rather than
// either of the book's own two screens.
export interface ActiveRadiationZone {
  code: string;
  name: string;
  unitCode: string | null;
  classification: string;
  status: string;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RadiationZonesApi {
  private readonly http = inject(HttpClient);

  getActiveZones(): Observable<ActiveRadiationZone[]> {
    return this.http.get<ActiveRadiationZone[]>(`${BFF_BASE_URL}/api/v1/radiation-monitoring/zones`);
  }
}
