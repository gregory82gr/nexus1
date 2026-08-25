import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DependenciesComponent } from './dependencies';
import { UnitSignalReading } from '../../core/api/instrumentation-api';
import { PlantStateService } from '../../core/state/plant-state';

describe('DependenciesComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DependenciesComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const signals: UnitSignalReading[] = [
    { tag: 'UNIT1-NI-001', name: 'Neutron Flux Channel 1', categoryCode: 'NEUTRONICS', latestValue: 93.5, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-25T09:00:00Z' },
    { tag: 'NX1-U1.RX.POWER', name: 'Reactor Power', categoryCode: 'POWER', latestValue: 100.1, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-25T09:00:00Z' },
    { tag: 'UNIT1-TURB-001', name: 'Main Turbine Shaft Speed', categoryCode: 'TURBINE', latestValue: 3001.1, latestQualityCode: 'GOOD', latestTimestampUtc: '2026-08-25T09:00:00Z' },
  ];

  function flushSignals(fixture: ReturnType<typeof TestBed.createComponent<DependenciesComponent>>, data: UnitSignalReading[]) {
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/instrumentation/units/${unitId}/signals`).flush(data);
    fixture.detectChanges();
  }

  it('shows the illustrative-topology banner regardless of which tab is active', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    for (const t of ['graph', 'matrix', 'chain'] as const) {
      fixture.componentInstance.setTab(t);
      fixture.detectChanges();
      const banner = fixture.nativeElement.querySelector('[data-banner="illustrative"]');
      expect(banner).toBeTruthy();
      expect(banner.textContent).toMatch(/illustrative/i);
    }
  });

  it('types every edge as illustrative-topology on the component itself', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);
    expect(fixture.componentInstance.allIllustrative).toBe(true);
  });

  it('derives real node status from the real fetched signal for backed nodes', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const fluxNode = fixture.componentInstance.nodes.find((n) => n.id === 'flux')!;
    const status = fixture.componentInstance.statusFor(fluxNode);
    expect(status).toEqual({ kind: 'real', value: 93.5, qualityCode: 'GOOD' });
  });

  it('never assigns a real/loading/error status to a node with no real backing, even after signals load', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const rodsNode = fixture.componentInstance.nodes.find((n) => n.id === 'rods')!;
    expect(fixture.componentInstance.statusFor(rodsNode)).toEqual({ kind: 'no-source' });
  });

  it('renders gap nodes with a visually distinct class (dashed/hatched), never the same "real" class as live nodes', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const el: HTMLElement = fixture.nativeElement;
    const rodsCard = el.querySelector('[data-node="rods"]')!;
    const fluxCard = el.querySelector('[data-node="flux"]')!;
    expect(rodsCard.classList.contains('gap')).toBe(true);
    expect(rodsCard.classList.contains('real')).toBe(false);
    expect(fluxCard.classList.contains('real')).toBe(true);
    expect(fluxCard.classList.contains('gap')).toBe(false);
  });

  it('never shows a fabricated numeric value on a gap node', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const rodsCard = el(fixture, '[data-node="rods"]');
    expect(rodsCard.querySelector('.node-value')).toBeNull();
    expect(rodsCard.textContent).toMatch(/NO SOURCE/i);
  });

  it('renders an actual node-link diagram on the Graph tab -- one real SVG line per edge, not just a text list', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const lines = fixture.nativeElement.querySelectorAll('svg.graph-svg line');
    expect(lines.length).toBe(fixture.componentInstance.edges.length);
  });

  it('draws the two negative-feedback edges with a visually distinct dashed-violet line class', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const feedbackLines = fixture.nativeElement.querySelectorAll('svg.graph-svg line.feedback-line');
    expect(feedbackLines.length).toBe(2);
  });

  it('gives every node card a real screen position derived from the shared layout, not left to default flow', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);

    const rodsCard = fixture.nativeElement.querySelector('[data-node="rods"]') as HTMLElement;
    expect(rodsCard.style.left).not.toBe('');
    expect(rodsCard.style.top).not.toBe('');
  });

  it('matrix tab reads the same typed edge dataset as the graph', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);
    fixture.componentInstance.setTab('matrix');
    fixture.detectChanges();

    expect(fixture.componentInstance.cell('rods', 'reactivity')?.weight).toBe(0.92);
  });

  it('chain tab walks a forward path with no negative-feedback edges', () => {
    const fixture = TestBed.createComponent(DependenciesComponent);
    flushSignals(fixture, signals);
    fixture.componentInstance.setTab('chain');
    fixture.detectChanges();

    expect(fixture.componentInstance.chain.length).toBeGreaterThan(0);
    expect(fixture.componentInstance.chain.every((e) => e.weight > 0)).toBe(true);
  });

  function el(fixture: ReturnType<typeof TestBed.createComponent<DependenciesComponent>>, selector: string): HTMLElement {
    return (fixture.nativeElement as HTMLElement).querySelector(selector)!;
  }
});
