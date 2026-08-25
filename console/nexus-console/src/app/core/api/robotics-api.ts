import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

// Mirrors Nexus1.Robotics.Application's own UnitRobotStatusDto exactly.
// No dose/radiation field anywhere -- checked the real domain directly
// (RobotHealthSnapshot has BatteryPercent/EstimatedRuntimeMin/
// CpuLoadPercent/FaultCount, nothing else), not just the DTO. A total
// absence, not a nullable field this client omits.
export interface UnitRobotStatus {
  robotCode: string;
  robotName: string;
  robotStatus: string;
  latestBatteryPercent: number | null;
  latestBatteryStatus: string | null;
  latestCommunicationStatus: string | null;
  latestSnapshotAtUtc: string | null;
}

// Mirrors UnitMissionDto exactly. A real, already-dispatched mission
// record (a work order), not the book's own abstract "standard mission
// type" concept -- see features/mission-readiness/mission-readiness.ts's
// own doc comment for the full reasoning.
export interface UnitMission {
  missionCode: string;
  title: string;
  missionType: string;
  missionStatus: string;
  missionPriority: string;
  requestedAtUtc: string;
  plannedStartUtc: string | null;
  plannedEndUtc: string | null;
  actualStartUtc: string | null;
  actualEndUtc: string | null;
}

export interface UnitRoboticsOverview {
  unitId: number;
  robots: UnitRobotStatus[];
  missions: UnitMission[];
}

// Mirrors ReadinessFailureDto exactly -- one named, blocking check from a
// mission's own recorded MissionReadinessAssessment.
export interface ReadinessFailure {
  checkName: string;
  readinessStatus: string;
  detail: string | null;
}

const BFF_BASE_URL = 'http://localhost:5103';

@Injectable({ providedIn: 'root' })
export class RoboticsApi {
  private readonly http = inject(HttpClient);

  getUnitOverview(unitId: number): Observable<UnitRoboticsOverview> {
    return this.http.get<UnitRoboticsOverview>(`${BFF_BASE_URL}/api/v1/robotics/units/${unitId}`);
  }

  // missionId is a real backend `long` -- passed as a string to avoid any
  // JS Number precision loss on a genuinely 64-bit id, even though this
  // dev database's own ids are small.
  getReadinessFailures(missionId: string): Observable<ReadinessFailure[]> {
    return this.http.get<ReadinessFailure[]>(`${BFF_BASE_URL}/api/v1/robotics/missions/${missionId}/readiness-failures`);
  }
}
