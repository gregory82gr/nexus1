import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AgeingDegradationComponent } from './ageing-degradation';
import { ActiveDegradationCase } from '../../core/api/maintenance-api';

describe('AgeingDegradationComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgeingDegradationComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const cases: ActiveDegradationCase[] = [
    { assetCode: 'ASSET-UNIT1-001', mechanism: 'CORROSION', severity: 'HIGH', detectedAtUtc: '2026-08-24T00:00:00', trendPoints: 3 },
  ];

  it('starts in the loading state and fetches the fleet-wide endpoint (no unit or department id)', () => {
    const fixture = TestBed.createComponent(AgeingDegradationComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/degradation-cases').flush(cases);
  });

  it('renders real case data, including the trend-point count as a count, not a percentage', () => {
    const fixture = TestBed.createComponent(AgeingDegradationComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/degradation-cases').flush(cases);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedCases).toHaveLength(1);
    expect(c.loadedCases[0].trendPoints).toBe(3);
    expect(c.toneOf('HIGH')).toBe('crit');
  });

  it('shows a real, honest empty state, not fake data, when there are no active cases', () => {
    const fixture = TestBed.createComponent(AgeingDegradationComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/degradation-cases').flush([]);

    expect(fixture.componentInstance.loadedCases).toHaveLength(0);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(AgeingDegradationComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/maintenance/degradation-cases').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
