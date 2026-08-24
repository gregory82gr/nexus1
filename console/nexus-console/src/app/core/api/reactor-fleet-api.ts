import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.ReactorFleet.Application's own UnitSummaryDto exactly
// (field for field) -- not a screen-shaped reinterpretation. Only Code/Name
// exist on the real Phase 1 Unit aggregate (ADR-003); LatestPowerPercent/
// LatestPowerRecordedAtUtc come from the most recent UnitPowerSnapshot and
// are null for a unit with no recorded snapshot yet. There is no plant
// name, no installed-capacity rating, no reactor model designation, and no
// online/offline flag anywhere in this contract -- see fleet.ts's own doc
// comment for what that means for the screen.
export interface UnitSummary {
  id: number;
  code: string;
  name: string;
  latestPowerPercent: number | null;
  latestPowerRecordedAtUtc: string | null;
}

// Named simplification, ahead of Ch. 5: the base URL is a plain constant
// here, not yet the chapter's own injected-config entrypoint script. That
// mechanism is real Ch. 5 scope and is deferred until that chapter is
// tackled directly, rather than partially built as a side effect of this
// screen.
const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class ReactorFleetApi {
  private readonly http = inject(HttpClient);

  getUnits(): Observable<UnitSummary[]> {
    return this.http.get<UnitSummary[]>(`${BFF_BASE_URL}/api/v1/reactor-fleet/units`);
  }
}
