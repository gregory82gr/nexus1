import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AiDiagnosticsComponent } from './ai-diagnostics';
import { RootCauseCase } from '../../core/api/root-cause-cases-api';
import { PlantStateService } from '../../core/state/plant-state';

describe('AiDiagnosticsComponent', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AiDiagnosticsComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  const cases: RootCauseCase[] = [
    { caseId: 1, unitId: 1, alarmFloodId: 101, status: 'VerdictIssued', verdict: 'Loose fitting confirmed as cause.', openedAtUtc: '2026-08-20T09:00:00Z', verdictIssuedAtUtc: '2026-08-20T11:30:00Z' },
    { caseId: 4, unitId: 1, alarmFloodId: 104, status: 'Open', verdict: null, openedAtUtc: '2026-08-25T05:00:00Z', verdictIssuedAtUtc: null },
  ];

  it('starts in the loading state and fetches real case history for the selected unit', () => {
    const fixture = TestBed.createComponent(AiDiagnosticsComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    expect(fixture.componentInstance.state().status).toBe('loading');
    httpMock.expectOne(`http://localhost:5103/api/v1/reporting/units/${unitId}`).flush(cases);
  });

  it('shows real case status and verdict text, not a risk score', () => {
    const fixture = TestBed.createComponent(AiDiagnosticsComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/reporting/units/${unitId}`).flush(cases);

    const loaded = fixture.componentInstance.loadedCases;
    expect(loaded).toHaveLength(2);
    expect(loaded[0].status).toBe('VerdictIssued');
    expect(loaded[0].verdict).toBe('Loose fitting confirmed as cause.');
    expect(loaded[1].status).toBe('Open');
    expect(loaded[1].verdict).toBeNull();
  });

  it('shows a real empty state, not fabricated cases, when none exist', () => {
    const fixture = TestBed.createComponent(AiDiagnosticsComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/reporting/units/${unitId}`).flush([]);

    expect(fixture.componentInstance.loadedCases).toHaveLength(0);
  });

  it('never renders a case status using the alarm/safety ok/warn/crit pill classes', () => {
    const fixture = TestBed.createComponent(AiDiagnosticsComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/reporting/units/${unitId}`).flush(cases);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelectorAll('.pill.ok, .pill.warn, .pill.crit').length).toBe(0);
    expect(el.querySelectorAll('.case-status').length).toBe(2);
  });

  it('shows a real error state, not fake data, when the endpoint is unreachable', () => {
    const fixture = TestBed.createComponent(AiDiagnosticsComponent);
    const unitId = TestBed.inject(PlantStateService).selectedId();
    httpMock.expectOne(`http://localhost:5103/api/v1/reporting/units/${unitId}`).error(new ProgressEvent('error'));

    expect(fixture.componentInstance.state().status).toBe('error');
  });
});
