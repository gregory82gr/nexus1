import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { MissionReadinessComponent } from './mission-readiness';
import { ReadinessFailure, UnitRoboticsOverview } from '../../core/api/robotics-api';

describe('MissionReadinessComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MissionReadinessComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const overview: UnitRoboticsOverview = {
    unitId: 1,
    robots: [],
    missions: [
      { missionCode: 'MISSION-1', title: 'Inspect containment weld seams', missionType: 'INSPECTION', missionStatus: 'IN_PROGRESS', missionPriority: 'NORMAL', requestedAtUtc: '2026-08-22T08:00:00', plannedStartUtc: null, plannedEndUtc: null, actualStartUtc: null, actualEndUtc: null },
    ],
  };

  it('starts loading missions and fetches the unit overview endpoint', () => {
    const fixture = TestBed.createComponent(MissionReadinessComponent);
    expect(fixture.componentInstance.missionsState().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);
  });

  it('renders the real mission list', () => {
    const fixture = TestBed.createComponent(MissionReadinessComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);

    const c = fixture.componentInstance;
    expect(c.missionsState().status).toBe('loaded');
    expect(c.loadedMissions).toHaveLength(1);
    expect(c.loadedMissions[0].missionCode).toBe('MISSION-1');
  });

  it('starts the readiness lookup idle, and looks up real failures for a manually entered mission id', () => {
    const fixture = TestBed.createComponent(MissionReadinessComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);

    expect(fixture.componentInstance.failuresState().status).toBe('idle');

    const failures: ReadinessFailure[] = [{ checkName: 'Dose budget', readinessStatus: 'BLOCKED', detail: 'Over dose budget' }];
    fixture.componentInstance.onLookupIdInput('1');
    fixture.componentInstance.lookupReadiness();
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/missions/1/readiness-failures').flush(failures);

    const c = fixture.componentInstance;
    expect(c.failuresState().status).toBe('loaded');
    expect(c.loadedFailures).toHaveLength(1);
    expect(c.loadedFailures[0].checkName).toBe('Dose budget');
  });

  it('treats an empty readiness-failures response as a real "no blocking failures" state, not an error', () => {
    const fixture = TestBed.createComponent(MissionReadinessComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);

    fixture.componentInstance.lookupReadiness();
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/missions/1/readiness-failures').flush([]);

    const c = fixture.componentInstance;
    expect(c.failuresState().status).toBe('loaded');
    expect(c.loadedFailures).toHaveLength(0);
  });

  it('shows a real error state, not fake data, when the missions endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(MissionReadinessComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.missionsState().status).toBe('error');
  });
});
