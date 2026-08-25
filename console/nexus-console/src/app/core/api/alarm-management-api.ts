import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.AlarmManagement.Application's own ActiveAlarmSummaryDto
// (fleet-wide, UnitId included -- distinct from the per-unit ActiveAlarmDto
// overview-api.ts's own ActiveAlarm type mirrors) and the BFF's
// AcknowledgeAlarmRequest. Both routes were already live and proven before
// this slice (2026-08-22 read/write BFF vertical slice) -- no new backend
// code for this cluster, a dedicated client calling them directly rather
// than through the composite /overview response, same pattern as
// radiation-safety-api.ts versus overview-api.ts's own radiation field.
export interface ActiveAlarm {
  alarmEventId: number;
  unitId: number;
  message: string;
  severity: string;
  raisedAtUtc: string;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class AlarmManagementApi {
  private readonly http = inject(HttpClient);

  getActiveAlarms(): Observable<ActiveAlarm[]> {
    return this.http.get<ActiveAlarm[]>(`${BFF_BASE_URL}/api/v1/alarm-management/alarms/active`);
  }

  acknowledge(alarmEventId: number, acknowledgedByUserId: string): Observable<void> {
    return this.http.post<void>(`${BFF_BASE_URL}/api/v1/alarm-management/alarms/${alarmEventId}/acknowledge`, {
      acknowledgedByUserId,
    });
  }
}
