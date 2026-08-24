import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Instrumentation.Application's own UnitSignalReadingDto
// exactly. This is the ONE real endpoint behind five of the book's Reactor
// sub-screens (Core, Control Rods, Neutronics, Coolant/TH, Steam
// Generators) -- Instrumentation's domain model has no separate entity
// for any of those subsystems, only a generic Signal/Measurement pair
// distinguished by CategoryCode (a data-content lookup, not a domain
// concept). See reactor-instrumentation.ts's own doc comment for how this
// client uses that.
export interface UnitSignalReading {
  tag: string;
  name: string;
  categoryCode: string;
  latestValue: number | null;
  latestQualityCode: string | null;
  latestTimestampUtc: string | null;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class InstrumentationApi {
  private readonly http = inject(HttpClient);

  getSignals(unitId: number): Observable<UnitSignalReading[]> {
    return this.http.get<UnitSignalReading[]>(`${BFF_BASE_URL}/api/v1/instrumentation/units/${unitId}/signals`);
  }
}
