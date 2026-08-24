import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AbsenceStressTestComponent } from './absence-stress-test';
import { StaffingScenarioGap } from '../../core/api/organization-api';

describe('AbsenceStressTestComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AbsenceStressTestComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const gaps: StaffingScenarioGap[] = [
    { positionId: 1, requiredCount: 2, availableCount: 1, gapCount: 1, notes: null },
    { positionId: 2, requiredCount: 1, availableCount: 1, gapCount: 0, notes: null },
  ];

  it('starts in the loading state and fetches scenario 1 by default', () => {
    const fixture = TestBed.createComponent(AbsenceStressTestComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/1/gaps').flush(gaps);
  });

  it('counts breached positions honestly from the real gap counts', () => {
    const fixture = TestBed.createComponent(AbsenceStressTestComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/1/gaps').flush(gaps);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.breachedCount()).toBe(1);
    expect(c.loadedGaps).toHaveLength(2);
  });

  it('treats an empty response as a real "not evaluated" state, not an error', () => {
    const fixture = TestBed.createComponent(AbsenceStressTestComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/1/gaps').flush([]);

    const c = fixture.componentInstance;
    expect(c.state().status).toBe('loaded');
    expect(c.loadedGaps).toHaveLength(0);
    expect(c.breachedCount()).toBe(0);
  });

  it('re-fetches for a new scenario id entered in the picker', () => {
    const fixture = TestBed.createComponent(AbsenceStressTestComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/1/gaps').flush(gaps);

    fixture.componentInstance.onScenarioIdInput('2');
    expect(fixture.componentInstance.scenarioId()).toBe(2);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/2/gaps').flush([]);

    expect(fixture.componentInstance.loadedGaps).toHaveLength(0);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(AbsenceStressTestComponent);
    httpMock.expectOne('http://localhost:5103/api/v1/organization/staffing-scenarios/1/gaps').error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
