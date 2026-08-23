import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Every interface below mirrors Nexus1.Bff's own composed OverviewDto and
// its four nested per-context DTOs field-for-field (System.Text.Json's
// default camelCase) -- not a screen-shaped reinterpretation. See
// overview.ts's own doc comment for what this shape does and doesn't
// support relative to the book's own Ch. 6 claims.

export interface UnitPowerSnapshot {
  powerPercent: number;
  recordedAtUtc: string;
}

export interface UnitDetail {
  id: number;
  code: string;
  name: string;
  latestPowerPercent: number | null;
  latestPowerRecordedAtUtc: string | null;
  recentPowerSnapshots: UnitPowerSnapshot[];
}

export interface ActiveAlarm {
  alarmEventId: number;
  message: string;
  severity: string;
  raisedAtUtc: string;
}

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

export interface UnitSignalReading {
  tag: string;
  name: string;
  categoryCode: string;
  latestValue: number | null;
  latestQualityCode: string | null;
  latestTimestampUtc: string | null;
}

export interface Overview {
  unitId: number;
  unit: UnitDetail | null;
  activeAlarms: ActiveAlarm[] | null;
  radiation: UnitRadiationSafety | null;
  signals: UnitSignalReading[] | null;
  // Keyed by section name exactly as the BFF emits them: "unit",
  // "activeAlarms", "radiation", "signals". A section present here failed
  // independently of the other three (Task.WhenAll + per-call try/catch on
  // the BFF side) -- it does not mean the whole response failed.
  errors: Record<string, string>;
}

const BFF_BASE_URL = 'http://localhost:5103'; // same named simplification as reactor-fleet-api.ts, ahead of Ch. 5

@Injectable({ providedIn: 'root' })
export class OverviewApi {
  private readonly http = inject(HttpClient);

  getOverview(unitId: number): Observable<Overview> {
    return this.http.get<Overview>(`${BFF_BASE_URL}/api/v1/overview/units/${unitId}`);
  }
}
