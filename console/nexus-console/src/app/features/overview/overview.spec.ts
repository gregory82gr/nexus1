import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { OverviewComponent } from './overview';
import { Overview } from '../../core/api/overview-api';

describe('OverviewComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OverviewComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const fullOverview: Overview = {
    unitId: 1,
    unit: { id: 1, code: 'UNIT-1', name: 'Demonstrator Unit 1', latestPowerPercent: 95, latestPowerRecordedAtUtc: '2026-08-22T11:00:00', recentPowerSnapshots: [] },
    activeAlarms: [{ alarmEventId: 1, message: 'SG-2 level deviation', severity: 'CRIT', raisedAtUtc: '2026-08-24T04:09:00' }],
    radiation: { unitId: 1, monitors: [], zones: [] },
    signals: [],
    errors: {},
  };

  it('starts in the loading state', () => {
    const fixture = TestBed.createComponent(OverviewComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/overview/units/1').flush(fullOverview);
  });

  it('derives the alarm count from the list, not a literal', () => {
    const fixture = TestBed.createComponent(OverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/overview/units/1').flush(fullOverview);

    expect(fixture.componentInstance.state().status).toBe('loaded');
    expect(fixture.componentInstance.alarmCount()).toBe(1);
  });

  it('shows a real per-section error without failing the whole screen (partial failure)', () => {
    const partial: Overview = {
      ...fullOverview,
      radiation: null,
      errors: { radiation: 'The RadiationMonitoring section is currently unreachable.' },
    };

    const fixture = TestBed.createComponent(OverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/overview/units/1').flush(partial);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.sectionError('radiation')).toBe('The RadiationMonitoring section is currently unreachable.');
    // The other three sections are untouched by radiation's failure.
    expect(c.sectionError('unit')).toBeNull();
    expect(c.alarmCount()).toBe(1);
  });

  it('shows a whole-request error state, not fake data, when the endpoint itself is unreachable', () => {
    const fixture = TestBed.createComponent(OverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/overview/units/1').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });

  it('reports a 404 honestly when the unit does not exist', () => {
    const fixture = TestBed.createComponent(OverviewComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/overview/units/1').flush(
      { error: 'Unit 1 does not exist.' },
      { status: 404, statusText: 'Not Found' },
    );

    const s = fixture.componentInstance.state();
    expect(s.status).toBe('error');
    expect(fixture.componentInstance.errorMessage).toContain('does not exist');
  });
});
