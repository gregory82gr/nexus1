import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { RootCauseComponent } from './root-cause';
import { DiagnosisResponse } from '../../core/api/root-cause-diagnosis-api';

const URL = 'http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0418/diagnoses';

const verdictResponse: DiagnosisResponse = {
  diagnosisRunId: 7,
  incidentId: 'EVT-2026-0418',
  verdict: 'FV-104',
  abstained: false,
  abstainReason: null,
  candidates: [
    { tag: 'FV-104', role: 'origin', weight: 0.66, coverage: 0.7857 },
    { tag: 'PB-2A', role: 'proximate', weight: 0.2, coverage: 0.5 },
    { tag: 'RCP-1B', role: 'parallel', weight: 0.07, coverage: 0.5 },
    { tag: 'BUS-2A', role: 'contributing', weight: 0.05, coverage: 0.0714 },
    { tag: 'FT-7', role: 'ruled-out', weight: 0.02, coverage: 0 },
  ],
  citations: [
    { chunkId: 1, sourceLabel: 'From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.1-2' },
    { chunkId: 2, sourceLabel: 'From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.2 (delay windows)' },
  ],
  auditHash: '3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81',
};

describe('RootCauseComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RootCauseComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('starts loading and POSTs the diagnosis run for the fixed incident', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    const req = httpMock.expectOne(URL);
    expect(req.request.method).toBe('POST');
    req.flush(verdictResponse);
  });

  it('renders the real verdict, ranked candidates, citations, and audit hash from the backend', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(URL).flush(verdictResponse);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('[data-field=verdict]')?.textContent).toContain('FV-104');
    expect(el.querySelectorAll('[data-field=candidate-tag]').length).toBe(5);
    expect(el.textContent).toContain('FT-7'); // the ruled-out candidate is kept, not hidden
    expect(el.querySelectorAll('[data-field=citation]').length).toBe(2);
    expect(el.textContent).toContain('From Flood to Cause');
    expect(el.querySelector('[data-field=audit-hash]')?.textContent).toContain('3f0986425052060b');
    expect(el.textContent!.toLowerCase()).toContain('illustrative');
  });

  it('does not draw a fault-tree graph (topology endpoint pending) and says so honestly', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(URL).flush(verdictResponse);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelectorAll('svg').length).toBe(0); // no hand-drawn graph
    expect(el.textContent!.toLowerCase()).toContain('graph visualization');
    expect(el.textContent!.toLowerCase()).toContain('pending');
  });

  it('shows a first-class abstention (not an error) when the pipeline abstains', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(URL).flush({
      ...verdictResponse,
      verdict: null,
      abstained: true,
      abstainReason: 'telemetry corroboration failed: missing historian data for component 6',
    });
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(fixture.componentInstance.state().status).toBe('abstained');
    expect(el.textContent!.toLowerCase()).toContain('inconclusive');
    expect(el.querySelector('[data-field=abstain-reason]')?.textContent).toContain('telemetry corroboration failed');
    expect(el.querySelectorAll('[data-field=error]').length).toBe(0); // an abstention is never an error
  });

  it('shows an honest empty state (404) for an incident the backend does not serve', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(URL).flush(null, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(fixture.componentInstance.state().status).toBe('unknown');
    expect(el.textContent!.toLowerCase()).toContain('no diagnosis available');
  });

  it('shows a service-unavailable error, not fabricated data, on 503', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(URL).flush(null, { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();

    expect(fixture.componentInstance.state().status).toBe('error');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('unavailable');
  });
});
