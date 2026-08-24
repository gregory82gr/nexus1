import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Maintenance.Application's own UnitAssetConditionDto
// exactly. This is the ONE real endpoint behind all three of the book's
// Rod Inspection cluster screens (Inspection Overview, NDT Methods, Rod
// Type/Film) -- Maintenance's domain model has no rod-specific entity
// anywhere, only a generic asset/condition model (any maintainable
// equipment item, generic category/status/grade lookups). See
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

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class MaintenanceApi {
  private readonly http = inject(HttpClient);

  getAssetConditions(unitId: number): Observable<UnitAssetCondition[]> {
    return this.http.get<UnitAssetCondition[]>(`${BFF_BASE_URL}/api/v1/maintenance/units/${unitId}/assets`);
  }
}
