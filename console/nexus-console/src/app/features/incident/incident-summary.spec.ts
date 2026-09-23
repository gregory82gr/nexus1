import { readFileSync } from 'fs';
import { join } from 'path';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { IncidentSummaryComponent } from './incident-summary';
import { RootCauseComponent } from '../root-cause/root-cause';
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
  ],
  citations: [{ chunkId: 1, sourceLabel: 'From Flood to Cause -- NEXUS-1 Companion, worked example, Ch.1-2' }],
  auditHash: '3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81',
};

// The source of the incident feature -- what a hand-authored hypothesis list
// would live in (Ch.29's retired CAND array), scanned structurally.
const featureSource = (): string =>
  readFileSync(join(__dirname, 'incident-summary.ts'), 'utf8') +
  readFileSync(join(__dirname, 'incident-summary.html'), 'utf8');

describe('IncidentSummaryComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IncidentSummaryComponent, RootCauseComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('shows the same top cause as the Root Cause graph, for the same incident (one computation)', () => {
    // Both screens read the same RootCauseService -- one shared fetch, one answer.
    const graph = TestBed.createComponent(RootCauseComponent);
    const summary = TestBed.createComponent(IncidentSummaryComponent);
    httpMock.expectOne(URL).flush(verdictResponse); // exactly one network run for both screens
    graph.detectChanges();
    summary.detectChanges();

    const graphTop = verdictResponse.candidates[0];
    const summaryEl: HTMLElement = summary.nativeElement;
    expect(summaryEl.querySelector('[data-field=top-cause-name]')?.textContent).toContain(graphTop.tag);
    expect(summaryEl.querySelector('[data-field=top-cause-pct]')?.textContent).toContain(String(Math.round(graphTop.weight * 100)));
    expect(graph.nativeElement.querySelector('[data-field=verdict]')?.textContent).toContain(graphTop.tag);
  });

  it('authors no local CAND-style hypothesis array of its own', () => {
    expect(featureSource()).not.toMatch(/const\s+CAND\s*=/);
  });

  it('spec-of-specs: the no-CAND guard actually detects a reintroduced CAND fixture (Ch.33 seen-to-fail ritual)', () => {
    // Deliberately prove the guard is load-bearing, not decorative: its own regex
    // must match a real CAND fixture. If a future edit reintroduced a local
    // hypothesis array, the guard above would fail exactly as this shows it would.
    const reintroduced = 'const CAND = [{ nm: "RCP-1B", causal: 0.74, note: "confirmed by an engineer" }];';
    expect(reintroduced).toMatch(/const\s+CAND\s*=/);
    // ...and the real feature contains none.
    expect(featureSource()).not.toMatch(/const\s+CAND\s*=/);
    // NOTE: the full live cross-screen E2E (Ch.33 operator-session) remains a named
    // deferred follow-up -- it needs the whole stack (BFF + Host + Ollama) running.
  });

  it('never claims "confirmed by an engineer" (a provenance this build cannot support)', () => {
    const fixture = TestBed.createComponent(IncidentSummaryComponent);
    httpMock.expectOne(URL).flush(verdictResponse);
    fixture.detectChanges();
    // The rendered UI never claims it (the meaningful guard -- a source scan would
    // trip on the doc comment that explains why the claim is deliberately absent).
    expect((fixture.nativeElement as HTMLElement).textContent).not.toMatch(/confirmed by an engineer/i);
  });

  it('starts loading with an honest wait note, then shows the top cause', () => {
    const fixture = TestBed.createComponent(IncidentSummaryComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).textContent!.toLowerCase()).toContain('running diagnosis');
    httpMock.expectOne(URL).flush(verdictResponse);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[data-field=top-cause-name]')?.textContent).toContain('FV-104');
  });

  it('shows a first-class abstention, not an error', () => {
    const fixture = TestBed.createComponent(IncidentSummaryComponent);
    httpMock.expectOne(URL).flush({ ...verdictResponse, verdict: null, abstained: true, abstainReason: 'no grounding: corpus retrieval returned no passages' });
    fixture.detectChanges();
    expect(fixture.componentInstance.state().status).toBe('abstained');
    expect(fixture.nativeElement.querySelector('[data-field=abstain-reason]')?.textContent).toContain('no grounding');
    expect(fixture.nativeElement.querySelectorAll('[data-field=error]').length).toBe(0);
  });

  it('shows a service-unavailable error on 502/503, never a fabricated top cause', () => {
    const fixture = TestBed.createComponent(IncidentSummaryComponent);
    httpMock.expectOne(URL).flush(null, { status: 502, statusText: 'Bad Gateway' });
    fixture.detectChanges();
    expect(fixture.componentInstance.state().status).toBe('error');
    expect(fixture.nativeElement.querySelectorAll('[data-field=top-cause-name]').length).toBe(0);
  });
});
