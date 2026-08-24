import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Organization.Application's own DepartmentRosterEntryDto
// exactly. Department-scoped, not unit-scoped -- there is no connection at
// all between ReactorFleet.Unit and Organization's hierarchy (ADR-017), so
// this is the first screen in the console that isn't driven by
// PlantStateService's selectedId. See core/state/department-state.ts.
export interface DepartmentRosterEntry {
  personId: number;
  displayName: string;
  personnelNumber: string | null;
  positionTitle: string | null;
  isSafetyCriticalPosition: boolean | null;
  applicationUserId: number | null;
  startDate: string;
  isPrimary: boolean;
}

// Mirrors StaffingScenarioGapDto exactly. Position-level only -- no name,
// no reason -- matching the same minimization discipline
// personnel-aggregation.ts applies to the roster. PositionId is a raw
// int; this endpoint exposes no title resolution for it (a real gap, not
// fabricated -- see absence-stress-test.ts's own doc comment).
export interface StaffingScenarioGap {
  positionId: number;
  requiredCount: number;
  availableCount: number;
  gapCount: number;
  notes: string | null;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class OrganizationApi {
  private readonly http = inject(HttpClient);

  getDepartmentRoster(departmentId: number): Observable<DepartmentRosterEntry[]> {
    return this.http.get<DepartmentRosterEntry[]>(`${BFF_BASE_URL}/api/v1/organization/departments/${departmentId}/roster`);
  }

  getStaffingGaps(staffingScenarioId: number): Observable<StaffingScenarioGap[]> {
    return this.http.get<StaffingScenarioGap[]>(`${BFF_BASE_URL}/api/v1/organization/staffing-scenarios/${staffingScenarioId}/gaps`);
  }
}
