import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { OptimizationComponent } from './optimization';
import { ClampedRecommendation, PolicyGridEntry } from '../../core/api/reinforcement-learning-api';

describe('OptimizationComponent', () => {
  let httpMock: HttpTestingController;
  const POLICY_URL = 'http://localhost:5103/api/v1/reinforcement-learning/policy';
  const RECS_URL = 'http://localhost:5103/api/v1/reinforcement-learning/recommendations';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OptimizationComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const entries: PolicyGridEntry[] = [
    { stateIndex: 0, stateCode: 'S0', bestActionCode: 'WITHDRAW_2', bestQValue: 0.62, actionMargin: 0.7 },
    { stateIndex: 1, stateCode: 'S1', bestActionCode: 'HOLD', bestQValue: 1.4, actionMargin: 2.25 },
  ];
  const recs: ClampedRecommendation[] = [
    { advisoryRecommendationId: 1, requestedAtUtc: '2026-08-21T14:05:00Z', stateCode: 'S0', recommendedActionCode: 'WITHDRAW_2', clampedActionCode: 'HOLD', clampReason: 'Recommended withdraw exceeded validated band; clamped to hold.' },
  ];

  it('starts both panels loading and fetches the real fleet-wide endpoints', () => {
    const fixture = TestBed.createComponent(OptimizationComponent);
    expect(fixture.componentInstance.policyState().status).toBe('loading');
    expect(fixture.componentInstance.recommendationsState().status).toBe('loading');
    httpMock.expectOne(POLICY_URL).flush(entries);
    httpMock.expectOne(RECS_URL).flush(recs);
  });

  it('shows the real policy grid entries once loaded', () => {
    const fixture = TestBed.createComponent(OptimizationComponent);
    httpMock.expectOne(POLICY_URL).flush(entries);
    httpMock.expectOne(RECS_URL).flush(recs);

    expect(fixture.componentInstance.loadedPolicyEntries).toHaveLength(2);
    expect(fixture.componentInstance.loadedPolicyEntries[0].bestActionCode).toBe('WITHDRAW_2');
  });

  it('maps a real 404 (no policy extracted yet) to a distinct no-policy state, not an error', () => {
    const fixture = TestBed.createComponent(OptimizationComponent);
    httpMock.expectOne(POLICY_URL).flush(null, { status: 404, statusText: 'Not Found' });
    httpMock.expectOne(RECS_URL).flush(recs);

    expect(fixture.componentInstance.policyState().status).toBe('no-policy');
  });

  it('shows real clamped-recommendation history, not a live suggestion', () => {
    const fixture = TestBed.createComponent(OptimizationComponent);
    httpMock.expectOne(POLICY_URL).flush(entries);
    httpMock.expectOne(RECS_URL).flush(recs);

    const loaded = fixture.componentInstance.loadedRecommendations;
    expect(loaded).toHaveLength(1);
    expect(loaded[0].clampedActionCode).toBe('HOLD');
    expect(loaded[0].clampReason).toContain('validated band');
  });

  it('shows a real error state for each panel independently on a genuine connectivity failure', () => {
    const fixture = TestBed.createComponent(OptimizationComponent);
    httpMock.expectOne(POLICY_URL).error(new ProgressEvent('error'), { status: 500 });
    httpMock.expectOne(RECS_URL).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.policyState().status).toBe('error');
    expect(fixture.componentInstance.recommendationsState().status).toBe('error');
  });
});
