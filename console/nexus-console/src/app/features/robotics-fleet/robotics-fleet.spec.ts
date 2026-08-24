import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { RoboticsFleetComponent } from './robotics-fleet';
import { UnitRoboticsOverview } from '../../core/api/robotics-api';

describe('RoboticsFleetComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoboticsFleetComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const overview: UnitRoboticsOverview = {
    unitId: 1,
    robots: [
      { robotCode: 'ROBOT-1', robotName: 'Demonstrator Robot 1', robotStatus: 'AVAILABLE', latestBatteryPercent: 82, latestBatteryStatus: 'NORMAL', latestCommunicationStatus: 'CONNECTED', latestSnapshotAtUtc: '2026-08-22T10:00:00' },
      { robotCode: 'ROBOT-2', robotName: 'Demonstrator Robot 2 (no health yet)', robotStatus: 'AVAILABLE', latestBatteryPercent: null, latestBatteryStatus: null, latestCommunicationStatus: null, latestSnapshotAtUtc: null },
    ],
    missions: [],
  };

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(RoboticsFleetComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);
  });

  it('renders the real robot list, including a robot with no health data yet', () => {
    const fixture = TestBed.createComponent(RoboticsFleetComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').flush(overview);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedRobots).toHaveLength(2);
    expect(c.loadedRobots[1].latestBatteryPercent).toBeNull();
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(RoboticsFleetComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/robotics/units/1').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
