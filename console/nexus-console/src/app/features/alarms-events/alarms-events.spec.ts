import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AlarmsEventsComponent } from './alarms-events';
import { ActiveAlarm } from '../../core/api/alarm-management-api';

describe('AlarmsEventsComponent', () => {
  let httpMock: HttpTestingController;
  const ACTIVE_URL = 'http://localhost:5103/api/v1/alarm-management/alarms/active';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlarmsEventsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const alarms: ActiveAlarm[] = [
    { alarmEventId: 90001, unitId: 9001, message: 'High containment dose rate', severity: 'Critical', raisedAtUtc: '2026-08-25T09:00:00Z' },
    { alarmEventId: 90002, unitId: 9002, message: 'Pump vibration threshold', severity: 'High', raisedAtUtc: '2026-08-25T09:05:00Z' },
  ];

  it('starts in the loading state and fetches the real fleet-wide active-alarms endpoint', () => {
    const fixture = TestBed.createComponent(AlarmsEventsComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne(ACTIVE_URL).flush(alarms);
  });

  it('groups the real alarms by severity, no decorative data added', () => {
    const fixture = TestBed.createComponent(AlarmsEventsComponent);
    httpMock.expectOne(ACTIVE_URL).flush(alarms);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.totalCount).toBe(2);
    expect(c.loadedGroups.map((g) => g.severity)).toEqual(['Critical', 'High']);
  });

  it('acknowledge() calls the real write endpoint and refetches the list on success', () => {
    const fixture = TestBed.createComponent(AlarmsEventsComponent);
    httpMock.expectOne(ACTIVE_URL).flush(alarms);

    fixture.componentInstance.acknowledge(90001);
    expect(fixture.componentInstance.isAcknowledging(90001)).toBe(true);

    const ackReq = httpMock.expectOne('http://localhost:5103/api/v1/alarm-management/alarms/90001/acknowledge');
    expect(ackReq.request.method).toBe('POST');
    expect(ackReq.request.body).toEqual({ acknowledgedByUserId: '11111111-1111-1111-1111-111111111111' });
    ackReq.flush(null);

    // A real acknowledged alarm drops off the active list on refetch, not
    // updated in place -- proving the effect came from the server, not a
    // client-side guess.
    httpMock.expectOne(ACTIVE_URL).flush([alarms[1]]);
    expect(fixture.componentInstance.totalCount).toBe(1);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(AlarmsEventsComponent);
    httpMock.expectOne(ACTIVE_URL).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
