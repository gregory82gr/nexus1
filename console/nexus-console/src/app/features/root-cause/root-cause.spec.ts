import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { RootCauseComponent } from './root-cause';
import { DiagnosisResponse } from '../../core/api/root-cause-diagnosis-api';
import { IncidentGraph } from '../../core/api/root-cause-graph-api';

const DIAG_URL = 'http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0418/diagnoses';
const GRAPH_URL = 'http://localhost:5103/api/v1/root-cause/incidents/EVT-2026-0418/graph';

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

// The seeded EVT-2026-0418 topology: 10 nodes, 9 edges (7 backbone + 1 learned + 1 rejected).
const graphResponse: IncidentGraph = {
  incidentId: 'EVT-2026-0418',
  nodes: [
    { componentId: 1, tag: 'FV-104', kind: 'valve', status: 'degrading', alarmCount: 1, illustrativeRole: null, illustrativeWeight: 0.66 },
    { componentId: 2, tag: 'SG-1', kind: 'condition', status: 'degrading', alarmCount: 2, illustrativeRole: null, illustrativeWeight: null },
    { componentId: 3, tag: 'FWP-2A', kind: 'pump', status: 'degrading', alarmCount: 1, illustrativeRole: null, illustrativeWeight: null },
    { componentId: 4, tag: 'PB-2A', kind: 'sensor', status: 'failed', alarmCount: 2, illustrativeRole: 'proximate', illustrativeWeight: 0.2 },
    { componentId: 5, tag: 'LF-1', kind: 'condition', status: 'degrading', alarmCount: 2, illustrativeRole: null, illustrativeWeight: null },
    { componentId: 6, tag: 'CT-1', kind: 'condition', status: 'degrading', alarmCount: 2, illustrativeRole: null, illustrativeWeight: null },
    { componentId: 7, tag: 'RT-1', kind: 'condition', status: 'failed', alarmCount: 1, illustrativeRole: null, illustrativeWeight: null },
    { componentId: 8, tag: 'RCP-1B', kind: 'pump', status: 'degrading', alarmCount: 2, illustrativeRole: 'parallel', illustrativeWeight: 0.07 },
    { componentId: 9, tag: 'BUS-2A', kind: 'bus', status: 'degrading', alarmCount: 1, illustrativeRole: 'contributing', illustrativeWeight: 0.05 },
    { componentId: 10, tag: 'FT-7', kind: 'sensor', status: 'ok', alarmCount: 0, illustrativeRole: 'ruled-out', illustrativeWeight: 0.02 },
  ],
  edges: [
    { fromComponentId: 1, toComponentId: 2, kind: 'backbone', delayMinSeconds: 36, delayMaxSeconds: 36, sourceRef: 'FT-FW-01' },
    { fromComponentId: 2, toComponentId: 3, kind: 'backbone', delayMinSeconds: 18, delayMaxSeconds: 18, sourceRef: 'FT-FW-02' },
    { fromComponentId: 3, toComponentId: 4, kind: 'backbone', delayMinSeconds: 42, delayMaxSeconds: 42, sourceRef: 'FT-FW-03' },
    { fromComponentId: 4, toComponentId: 5, kind: 'backbone', delayMinSeconds: 12, delayMaxSeconds: 12, sourceRef: 'FT-TH-01' },
    { fromComponentId: 5, toComponentId: 6, kind: 'backbone', delayMinSeconds: 30, delayMaxSeconds: 30, sourceRef: 'FT-TH-02' },
    { fromComponentId: 6, toComponentId: 7, kind: 'backbone', delayMinSeconds: 4, delayMaxSeconds: 4, sourceRef: 'FT-TH-03' },
    { fromComponentId: 8, toComponentId: 5, kind: 'backbone', delayMinSeconds: 10, delayMaxSeconds: 10, sourceRef: 'FT-TH-04' },
    { fromComponentId: 9, toComponentId: 3, kind: 'learned', delayMinSeconds: 2, delayMaxSeconds: 2, sourceRef: 'learned-2026Q1' },
    { fromComponentId: 10, toComponentId: 5, kind: 'rejected', delayMinSeconds: 0, delayMaxSeconds: 0, sourceRef: 'corr-flagged' },
  ],
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

  // The component fires two independent requests on construct: the slow diagnosis
  // POST and the fast graph GET. Helpers keep each test explicit about both.
  const flushGraph = () => httpMock.expectOne(GRAPH_URL).flush(graphResponse);
  const flushDiagnosis = (body: DiagnosisResponse = verdictResponse) => httpMock.expectOne(DIAG_URL).flush(body);

  it('fires both the diagnosis POST and the graph GET on construct', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    expect(fixture.componentInstance.state().status).toBe('loading');
    expect(fixture.componentInstance.graphState().status).toBe('loading');
    const diag = httpMock.expectOne(DIAG_URL);
    const graph = httpMock.expectOne(GRAPH_URL);
    expect(diag.request.method).toBe('POST');
    expect(graph.request.method).toBe('GET');
    diag.flush(verdictResponse);
    graph.flush(graphResponse);
  });

  it('draws the real seeded topology: 10 nodes, 9 edges (7 backbone + 1 learned + 1 rejected)', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    flushDiagnosis();
    flushGraph();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelectorAll('svg.rc-graph-svg g.rc-node').length).toBe(10);
    expect(el.querySelectorAll('svg.rc-graph-svg line.rc-edge').length).toBe(9);
    expect(el.querySelectorAll('line.rc-edge[data-kind=backbone]').length).toBe(7);
    expect(el.querySelectorAll('line.rc-edge[data-kind=learned]').length).toBe(1);
    expect(el.querySelectorAll('line.rc-edge[data-kind=rejected]').length).toBe(1);
  });

  it('labels walkable edges with their delay window, and the rejected edge with none', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    flushDiagnosis();
    flushGraph();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    // 8 labelled edges (7 backbone + 1 learned); the rejected edge carries no delay window.
    expect(el.querySelectorAll('text.rc-edge-label').length).toBe(8);
    expect(el.textContent).toContain('36s'); // FV-104 -> SG-1 backbone delay
    const fv104ToSg1 = el.querySelector('line.rc-edge[data-edge="FV-104-SG-1"]');
    expect(fv104ToSg1?.getAttribute('data-kind')).toBe('backbone');
    const ft7Rejected = el.querySelector('line.rc-edge[data-edge="FT-7-LF-1"]');
    expect(ft7Rejected?.getAttribute('data-kind')).toBe('rejected');
  });

  it('marks the origin node distinctly from the ruled-out node', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    flushDiagnosis();
    flushGraph();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    const origin = el.querySelector('g.rc-node[data-tag="FV-104"]');
    expect(origin?.getAttribute('data-role')).toBe('origin');
    const ruledOut = el.querySelector('g.rc-node[data-tag="FT-7"]');
    expect(ruledOut?.getAttribute('data-role')).toBe('ruled-out');
  });

  it('shows a graph error state (not a fabricated graph) when the topology endpoint fails', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    flushDiagnosis();
    httpMock.expectOne(GRAPH_URL).flush(null, { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(fixture.componentInstance.graphState().status).toBe('error');
    expect(el.querySelectorAll('svg.rc-graph-svg').length).toBe(0);
    expect(el.textContent!.toLowerCase()).toContain('graph topology endpoint is unavailable');
  });

  it('renders the real verdict, ranked candidates, citations, and audit hash from the backend', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    flushDiagnosis();
    flushGraph();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(el.querySelector('[data-field=verdict]')?.textContent).toContain('FV-104');
    expect(el.querySelectorAll('[data-field=candidate-tag]').length).toBe(5);
    expect(el.querySelectorAll('[data-field=citation]').length).toBe(2);
    expect(el.querySelector('[data-field=audit-hash]')?.textContent).toContain('3f0986425052060b');
    expect(el.textContent!.toLowerCase()).toContain('illustrative');
  });

  it('shows a first-class abstention (not an error) when the pipeline abstains', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(DIAG_URL).flush({
      ...verdictResponse,
      verdict: null,
      abstained: true,
      abstainReason: 'telemetry corroboration failed: missing historian data for component 6',
    });
    flushGraph();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;

    expect(fixture.componentInstance.state().status).toBe('abstained');
    expect(el.textContent!.toLowerCase()).toContain('inconclusive');
    expect(el.querySelectorAll('[data-field=error]').length).toBe(0);
  });

  it('shows an honest empty state (404) for an incident the backend does not serve', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(DIAG_URL).flush(null, { status: 404, statusText: 'Not Found' });
    flushGraph();
    fixture.detectChanges();

    expect(fixture.componentInstance.state().status).toBe('unknown');
    expect((fixture.nativeElement as HTMLElement).textContent!.toLowerCase()).toContain('no diagnosis available');
  });

  it('shows a service-unavailable error, not fabricated data, on diagnosis 503', () => {
    const fixture = TestBed.createComponent(RootCauseComponent);
    httpMock.expectOne(DIAG_URL).flush(null, { status: 503, statusText: 'Service Unavailable' });
    flushGraph();
    fixture.detectChanges();

    expect(fixture.componentInstance.state().status).toBe('error');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('unavailable');
  });
});
