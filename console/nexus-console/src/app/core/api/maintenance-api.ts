import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Maintenance.Application's own UnitAssetConditionDto
// exactly. This is the ONE real endpoint behind all three of the book's
// Rod Inspection cluster screens (Inspection Overview, NDT Methods, Rod
// Type/Film) -- Maintenance's domain model has no rod-specific entity
// anywhere, only a generic asset/condition model. See
// features/asset-condition/asset-condition.ts's own doc comment for what
// that means for these screens.
export interface UnitAssetCondition {
  assetCode: string;
  name: string;
  category: string;
  status: string;
  isSafetyRelated: boolean;
  latestAssessedAtUtc: string | null;
  latestConditionGrade: string | null;
  latestHealthScorePercent: number | null;
  latestRemainingUsefulLifeDays: number | null;
}

// Mirrors ActiveDegradationCaseDto exactly. Fleet-wide (no unit or
// department scoping at all -- the real query takes no parameter).
// TrendPoints is a COUNT of measured points, not the individual values,
// and there is no limit/threshold field -- see
// features/ageing-degradation/ageing-degradation.ts's own doc comment
// for why that shapes what this screen can honestly show.
export interface ActiveDegradationCase {
  assetCode: string;
  mechanism: string;
  severity: string;
  detectedAtUtc: string;
  trendPoints: number;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class MaintenanceApi {
  private readonly http = inject(HttpClient);

  getAssetConditions(unitId: number): Observable<UnitAssetCondition[]> {
    return this.http.get<UnitAssetCondition[]>(`${BFF_BASE_URL}/api/v1/maintenance/units/${unitId}/assets`);
  }

  getActiveDegradationCases(): Observable<ActiveDegradationCase[]> {
    return this.http.get<ActiveDegradationCase[]>(`${BFF_BASE_URL}/api/v1/maintenance/degradation-cases`);
  }
}
